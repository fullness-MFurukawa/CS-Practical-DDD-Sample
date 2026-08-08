using Ddd.Domain.Adapters;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Categories;
using Ddd.Infrastructure.Entities;
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

        // 集約の合成/分解を担う Factory。ドメインのポート(閉じたジェネリック)型で登録する。
        services.AddScoped<IProductFactory<ProductEntity, ProductCategoryEntity, ProductStockEntity>,
            ProductFactory>();


        // Repository。ドメインのポート(インターフェイス)型で登録する。
        // ※ IStockRepository は本サンプルでは未実装(在庫は Product 集約経由で永続化)。
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}