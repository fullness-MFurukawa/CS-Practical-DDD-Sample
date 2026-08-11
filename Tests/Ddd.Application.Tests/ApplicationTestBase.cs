using Ddd.Application.Events;
using Ddd.Application.Extensions;
using Ddd.Application.Persistence;
using Ddd.Application.Tests.Fakes;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Ddd.Application.Tests;

/// <summary>
/// アプリケーション層テストの基盤。<c>AddApplication()</c> による本番登録を用い、永続化
/// (リポジトリ・<see cref="IUnitOfWork"/>)のみインメモリ Fake に差し替えて DI から解決する。
/// </summary>
/// <remarks>
/// テスト対象は実物(Adapter・Factory・Service・Interactor)を DI から解決するため、
/// <c>AddApplication</c> の登録自体も併せて検証される。各テストはスコープ単位で実行され、
/// Scoped の Fake はテストごとに新しいインスタンスとなる(状態はテスト間で共有されない)。
/// </remarks>
public abstract class ApplicationTestBase
{
    private static readonly IServiceProvider RootProvider = BuildRootProvider();
    private IServiceScope _scope = null!;

    /// <summary>
    /// 現在のテストスコープのサービスプロバイダ。
    /// </summary>
    protected IServiceProvider Services => _scope.ServiceProvider;

    /// <summary>
    /// 現在のスコープから必須サービスを解決する。
    /// </summary>
    protected T GetRequiredService<T>() where T : notnull => Services.GetRequiredService<T>();

    /// <summary>
    /// 各テスト開始時にスコープを生成する。
    /// </summary>
    [TestInitialize]
    public void InitializeScope() => _scope = RootProvider.CreateScope();

    /// <summary>
    /// 各テスト終了時にスコープを破棄する。
    /// </summary>
    [TestCleanup]
    public void DisposeScope() => _scope.Dispose();

    private static IServiceProvider BuildRootProvider()
    {
        var services = new ServiceCollection();

        // 本番のアプリケーション層登録(Adapter・Factory・Service・Usecase)。
        services.AddApplication();

        // 永続化はインメモリ Fake に差し替える(実DB不要)。
        // 具象型でも登録し、テストからはそれを解決してシード投入する
        // (インターフェイス経由でも同一スコープでは同一インスタンスになる)。
        services.AddScoped<FakeProductRepository>();
        services.AddScoped<IProductRepository>(sp => sp.GetRequiredService<FakeProductRepository>());
        services.AddScoped<FakeCategoryRepository>();
        services.AddScoped<ICategoryRepository>(sp => sp.GetRequiredService<FakeCategoryRepository>());
        services.AddScoped<IUnitOfWork, FakeUnitOfWork>();

        // ドメインイベントのディスパッチャも Fake(記録用)に差し替える。
        services.AddScoped<FakeDomainEventDispatcher>();
        services.AddScoped<IDomainEventDispatcher>(sp => sp.GetRequiredService<FakeDomainEventDispatcher>());

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });
    }
}