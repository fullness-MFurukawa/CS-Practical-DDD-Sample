using Ddd.Application.Exceptions;
using Ddd.Domain.Exceptions;
using Ddd.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Ddd.Api.Middleware;

/// <summary>
/// 各層でスローされた例外を一括で捕捉し、適切な HTTP ステータス(ProblemDetails)へ変換するミドルウェア。
/// </summary>
/// <remarks>
/// <para>
/// パイプラインの先頭付近に配置し、後続(コントローラ・ユースケース・値オブジェクト等)で発生した例外を
/// 捕捉して統一的なエラーレスポンスに変換する。<c>[ApiController]</c> による入力検証エラー(400)は
/// 例外ではなくフレームワークが自動生成するため、本ミドルウェアの対象外である。
/// </para>
/// <para>
/// 写像: <see cref="InvalidInputException"/> / <see cref="DomainException"/> → 400、
/// <see cref="NotFoundException"/> → 404、<see cref="ExistsException"/> → 409、
/// <see cref="InternalException"/> とその他 → 500。500 系は詳細をログにのみ記録し、
/// クライアントには汎用メッセージを返す(内部情報を漏らさない)。
/// </para>
/// </remarks>
/// <param name="next">次のミドルウェア。</param>
/// <param name="logger">ロガー。</param>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <summary>リクエストを処理し、例外が発生した場合はエラーレスポンスへ変換する。</summary>
    /// <param name="context">HTTP コンテキスト。</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        int status;
        string detail;

        switch (exception)
        {
            case InvalidInputException:
            case DomainException:
                status = StatusCodes.Status400BadRequest;
                detail = exception.Message;
                break;
            case NotFoundException:
                status = StatusCodes.Status404NotFound;
                detail = exception.Message;
                break;
            case ExistsException:
                status = StatusCodes.Status409Conflict;
                detail = exception.Message;
                break;
            case InternalException:
                logger.LogError(exception, "内部エラーが発生しました。");
                status = StatusCodes.Status500InternalServerError;
                detail = "サーバ内部エラーが発生しました。";
                break;
            default:
                logger.LogError(exception, "想定外のエラーが発生しました。");
                status = StatusCodes.Status500InternalServerError;
                detail = "サーバ内部エラーが発生しました。";
                break;
        }

        // 既にレスポンス送信が始まっている場合は変換できない(ヘッダを書き換えられない)。
        if (context.Response.HasStarted)
        {
            logger.LogWarning("レスポンス送信が既に開始されているため、例外をエラーレスポンスへ変換できません。");
            return;
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = TitleOf(status),
            Detail = detail,
            Instance = context.Request.Path,
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }

    private static string TitleOf(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "Internal Server Error",
    };
}

/// <summary>
/// <see cref="ExceptionHandlingMiddleware"/> を登録するための拡張メソッド。
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>例外処理ミドルウェアをパイプラインに追加する。</summary>
    /// <param name="app">アプリケーションビルダー。</param>
    /// <returns>連鎖呼び出し用の <paramref name="app"/>。</returns>
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}