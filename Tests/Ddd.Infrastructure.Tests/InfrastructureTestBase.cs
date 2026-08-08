using Ddd.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddd.Infrastructure.Tests;

/// <summary>
/// インフラストラクチャ層テストの共通基盤。<see cref="DependencyInjection.AddInfrastructure"/> で
/// 構成した DI コンテナからテストターゲットを解決する。
/// </summary>
/// <remarks>
/// <para>
/// ルートプロバイダは全テストで一度だけ構築し、各テストではスコープを作成して
/// <see cref="GetRequiredService{T}"/> で対象(Adapter / Factory / Repository)を解決する。
/// これにより、対象の振る舞いだけでなく <c>AddInfrastructure</c> の DI 登録の正しさも検証できる。
/// </para>
/// <para>
/// 接続文字列は <c>appsettings.Test.json</c> の <c>Postgres</c> を用いる。DB を参照しないテスト
/// (Adapter / Factory)は接続を行わないため、DB が停止していても実行できる。
/// </para>
/// </remarks>
public abstract class InfrastructureTestBase
{
    private static readonly IServiceProvider RootProvider = BuildRootProvider();

    private IServiceScope _scope = null!;

    /// <summary>現在のテストスコープのサービスプロバイダ。</summary>
    protected IServiceProvider Services => _scope.ServiceProvider;

    /// <summary>DI コンテナからサービスを解決する。</summary>
    protected T GetRequiredService<T>() where T : notnull => Services.GetRequiredService<T>();

    /// <summary>各テスト開始時: DI スコープを作成する。</summary>
    [TestInitialize]
    public void InitializeScope()
    {
        _scope = RootProvider.CreateScope();
    }

    /// <summary>各テスト終了時: DI スコープを破棄する(スコープ内の DbContext 等も破棄される)。</summary>
    [TestCleanup]
    public void DisposeScope()
    {
        _scope.Dispose();
    }

    private static IServiceProvider BuildRootProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "接続文字列 'Postgres' が appsettings.Test.json に設定されていません。");

        var services = new ServiceCollection();
        services.AddInfrastructure(connectionString);

        // スコープ検証を有効化し、Scoped サービスをスコープ外で解決していないことを保証する
        return services.BuildServiceProvider(validateScopes: true);
    }
}