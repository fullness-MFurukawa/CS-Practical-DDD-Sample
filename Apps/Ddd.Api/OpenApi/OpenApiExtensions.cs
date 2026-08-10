using Scalar.AspNetCore;

namespace Ddd.Api.OpenApi;

/// <summary>
/// OpenAPI ドキュメント生成と Scalar UI に関する登録・配線をまとめた拡張メソッド。
/// </summary>
/// <remarks>
/// 合成ルート(<c>Program.cs</c>)を簡潔に保つため、OpenAPI 関連の設定を本クラスへ集約する。
/// </remarks>
public static class OpenApiExtensions
{
    /// <summary>
    /// OpenAPI ドキュメント生成を登録し、メタ情報・タグ説明を付与するトランスフォーマを適用する。
    /// </summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            // ドキュメント全体(Info・タグ説明)を整える。
            options.AddDocumentTransformer<ApiDocumentTransformer>();
            // 自前で XML コメントを付けられないフレームワーク型 ProblemDetails に説明を注入する。
            options.AddSchemaTransformer<ProblemDetailsSchemaTransformer>();
        });
        return services;
    }

    /// <summary>
    /// OpenAPI ドキュメント(/openapi/v1.json)と Scalar UI(/scalar/v1)を配線し、ルートを Scalar へリダイレクトする。
    /// </summary>
    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options => options
            .WithTitle("商品管理API")
            .WithTheme(ScalarTheme.BluePlanet));

        // ルートを Scalar UI へリダイレクト(Java 版で "/" を Swagger UI に向けていたのと同じ意図)。
        // OpenAPI ドキュメント上でも日本語で表示されるよう、タグ・要約・説明を付ける。
        app.MapGet("/", () => Results.Redirect("/scalar/v1"))
            .WithTags("ドキュメント")
            .WithSummary("APIドキュメントへ移動")
            .WithDescription("ルート(/)にアクセスすると、Scalar の API ドキュメント画面(/scalar/v1)へリダイレクトします。");

        return app;
    }
}