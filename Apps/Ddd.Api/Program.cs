using Ddd.Api.Extensions;
using Ddd.Api.Middleware;
using Ddd.Api.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// サービス登録(合成ルート): プレゼンテーション層 + 下位層をまとめて登録する。
builder.Services.AddPresentation(builder.Configuration);

var app = builder.Build();

// ===== HTTP パイプライン =====

app.UseApiExceptionHandling();
app.MapApiDocumentation();
app.MapControllers();

app.Run();
public partial class Program { }
