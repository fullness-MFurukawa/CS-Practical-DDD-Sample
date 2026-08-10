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
        services.AddOpenApi(options => options.AddDocumentTransformer<ApiDocumentTransformer>());
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
        app.MapGet("/", () => Results.Redirect("/scalar/v1"));

        return app;
    }
}