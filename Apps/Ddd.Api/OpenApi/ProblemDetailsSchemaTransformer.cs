using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Ddd.Api.OpenApi;

/// <summary>
/// フレームワーク型 <see cref="ProblemDetails"/> のスキーマに、日本語の説明を付与するスキーマトランスフォーマ。
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ProblemDetails"/> は .NET が提供する型であり、こちらの XML ドキュメントコメントを付けられない
/// (XML 取り込みの対象外)。そこで、OpenAPI 生成時にスキーマを直接編集して説明を注入する。
/// </para>
/// <para>
/// エラー応答(例外 → HTTP ステータス変換)の形式説明を、スキーマ本体と主要プロパティに付ける。
/// </para>
/// </remarks>
internal sealed class ProblemDetailsSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        Microsoft.OpenApi.OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        // 対象は ProblemDetails のスキーマのときだけ。
        if (context.JsonTypeInfo.Type != typeof(ProblemDetails))
        {
            return Task.CompletedTask;
        }

        schema.Description =
            "エラー応答(RFC 7807 ProblemDetails 形式)。各層の例外を HTTP ステータスに対応させて返す。";

        SetPropertyDescription(schema, "status", "HTTP ステータスコード(例: 400 / 404 / 409 / 500)。");
        SetPropertyDescription(schema, "title", "エラー種別を表す短いタイトル。");
        SetPropertyDescription(schema, "detail", "エラーの詳細メッセージ(500 系は汎用メッセージ)。");
        SetPropertyDescription(schema, "instance", "エラーが発生したリクエストのパス。");

        return Task.CompletedTask;
    }

    /// <summary>指定プロパティが存在すれば説明を設定する(存在しなければ何もしない)。</summary>
    private static void SetPropertyDescription(OpenApiSchema schema, string propertyName, string description)
    {
        if (schema.Properties is not null
            && schema.Properties.TryGetValue(propertyName, out var property)
            && property is OpenApiSchema concrete)
        {
            concrete.Description = description;
            
        }
    }
}