// Apps/Ddd.Api/OpenApi/ApiDocumentTransformer.cs
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;


namespace Ddd.Api.OpenApi;

/// <summary>
/// 生成される OpenAPI ドキュメントに、API 全体のメタ情報(タイトル・バージョン・説明)と
/// タグ(サイドバーのグループ)の日本語説明を付与するドキュメントトランスフォーマ。
/// </summary>
/// <remarks>
/// Java 版の <c>OpenApiConfig</c>(<c>@OpenAPIDefinition</c>)に相当する。合成ルートを簡潔に保つため、
/// ドキュメント整形の責務を本クラスに切り出し、<see cref="OpenApiExtensions.AddApiDocumentation"/> で登録する。
/// </remarks>
internal sealed class ApiDocumentTransformer : IOpenApiDocumentTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info.Title = "商品管理API";
        document.Info.Version = "v1.0";
        document.Info.Description = "ドメイン駆動設計実践 C#編 サンプルAPIドキュメント(.NET 10)";

        // タグ(Scalar サイドバーのグループ)に日本語説明を付け、用途を一目で分かるようにする。
        document.Tags = new HashSet<OpenApiTag>
        {
            new() { Name = "SearchProducts", Description = "商品検索: 商品名で商品を取得します。" },
            new() { Name = "RegisterProducts", Description = "商品登録: カテゴリ参照・存在チェック・新規登録を行います。" },
        };

        return Task.CompletedTask;
    }
}