using Ddd.Application.Events;
using Ddd.Application.Persistence;
using Ddd.Domain.Adapters;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Products.Events;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Categories;
using Ddd.Infrastructure.Entities;
using Ddd.Infrastructure.Events;
using Ddd.Infrastructure.Events.Handlers;
using Ddd.Infrastructure.Persistence;
using Ddd.Infrastructure.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ddd.Infrastructure.Extensions;

/// <summary>
/// インフラストラクチャ層の依存関係を DI コンテナへ登録する拡張メソッドを提供する。
/// </summary>
/// <remarks>
/// 合成ルート(<c>Ddd.Api</c> の <c>Program.cs</c>)から <see cref="AddInfrastructure"/> を呼び出し、
/// <see cref="AppDbContext"/>(Npgsql)・各 Mapper・<see cref="ProductAssembler"/>・各 Repository を登録する。
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// インフラストラクチャ層のサービス(DbContext・Mapper・Assembler・Repository)を登録する。
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <param name="connectionString">PostgreSQL への接続文字列。</param>
    /// <returns>連鎖呼び出し用の <paramref name="services"/>。</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // データアクセス手段: EF Core (Npgsql)。AddDbContext により Scoped で登録される。
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        // 腐敗防止層(ACL)の Adapter。ドメインの Adapter ポート型で登録する。
        services.AddScoped<IToDomainAdapter<ProductCategoryEntity, Category>, ProductCategoryEntityAdapter>();
        services.AddScoped<IDomainBiAdapter<ProductEntity, Product>, ProductEntityAdapter>();
        services.AddScoped<IDomainBiAdapter<ProductStockEntity, Stock>, ProductStockEntityAdapter>();

        // 集約の合成/分解を担う Factory。ドメインの汎用ポート(集約ルート×外部の集約ルート)で登録する。
        services.AddScoped<IFactory<Product, ProductEntity>, ProductFactory>();

        // トランザクション境界(Unit of Work)。アプリケーション層のポート型で登録する。
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ドメインイベントのディスパッチャ(自作インプロセス)と、各イベントのハンドラ。
        // ディスパッチャは実行時のイベント型で IDomainEventHandler<具体型> を DI から解決する。
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<ProductRenamed>, ProductRenamedLoggingHandler>();
        services.AddScoped<IDomainEventHandler<ProductRepriced>, ProductRepricedLoggingHandler>();
        services.AddScoped<IDomainEventHandler<StockQuantityChanged>, StockQuantityChangedLoggingHandler>();

        // Repository。ドメインのポート(インターフェイス)型で登録する。
        // ※ IStockRepository は本サンプルでは未実装(在庫は Product 集約経由で永続化)。
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}