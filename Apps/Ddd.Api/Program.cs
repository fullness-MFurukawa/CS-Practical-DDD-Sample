using Ddd.Api.Middleware;
using Ddd.Api.Products.Schemas;
using Ddd.Application.Extensions;
using Ddd.Infrastructure.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ===== サービス登録(合成ルート) =====

// MVC コントローラ。
builder.Services.AddControllers();

// OpenAPI ドキュメント生成。Info(タイトル・バージョン等)はドキュメントトランスフォーマで設定する。
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "商品管理API";
        document.Info.Version = "v1.0";
        document.Info.Description = "ドメイン駆動設計実践サンプルAPIドキュメント(ORM:EF Core)";
        return Task.CompletedTask;
    });
});

// アプリケーション層・インフラストラクチャ層の登録。
var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("接続文字列 'Postgres' が設定されていません。");
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

// プレゼンテーション層: 商品登録スキーマ → ProductDto のアダプタ(状態を持たないため Singleton)。
builder.Services.AddSingleton<ProductCreateSchemaAdapter>();

var app = builder.Build();

// ===== HTTP パイプライン =====

// 例外処理は先頭付近に配置し、後続で発生した例外を捕捉して ProblemDetails へ変換する。
app.UseApiExceptionHandling();

// OpenAPI ドキュメント(/openapi/v1.json)と Scalar UI(/scalar/v1)。
app.MapOpenApi();
app.MapScalarApiReference();

// ルートを Scalar UI へリダイレクト(Java 版で "/" を Swagger UI に向けていたのと同じ意図)。
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.MapControllers();

app.Run();
