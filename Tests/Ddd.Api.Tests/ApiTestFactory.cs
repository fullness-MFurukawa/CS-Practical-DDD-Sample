using Ddd.Application.Persistence;
using Ddd.Application.Tests.Fakes;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddd.Api.Tests;

/// <summary>
/// コントローラ(プレゼンテーション層)テスト用の <see cref="WebApplicationFactory{TEntryPoint}"/>。
/// </summary>
/// <remarks>
/// <para>
/// アプリをインメモリのテストサーバとして起動する。永続化(リポジトリ・<see cref="IUnitOfWork"/>)は
/// インメモリ Fake に差し替え、実DBには一切触れない(接続文字列はダミー)。ドメインイベントの
/// ディスパッチャ・ハンドラは本物のまま(ログ出力のみ・DB不要)。
/// </para>
/// <para>
/// Fake は Singleton として登録し、テストから <see cref="Products"/> / <see cref="Categories"/> に
/// シード投入したデータが、続く HTTP リクエストの処理でも参照できるようにする。ファクトリはテストごとに
/// 生成するため、状態はテスト間で共有されない。
/// </para>
/// </remarks>
public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    /// <summary>テストからシード投入できる商品リポジトリ(Fake)。</summary>
    public FakeProductRepository Products { get; } = new();

    /// <summary>テストからシード投入できるカテゴリリポジトリ(Fake)。</summary>
    public FakeCategoryRepository Categories { get; } = new();

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 実DBに触れないためのダミー接続文字列(実際には Fake が使われる)。
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=localhost;Database=dummy;Username=x;Password=x",
            });
        });

        // 永続化を Fake に差し替える(後勝ちで本物の登録を上書き)。
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IProductRepository>(Products);
            services.AddSingleton<ICategoryRepository>(Categories);
            services.AddSingleton<IUnitOfWork, FakeUnitOfWork>();
        });
    }
}