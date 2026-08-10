using Ddd.Application.Adapters;
using Ddd.Application.Categories.Services;
using Ddd.Application.Dtos;
using Ddd.Application.Factories;
using Ddd.Application.Products.Services;
using Ddd.Application.Products.Usecases;
using Ddd.Application.Products.Usecases.Interactors;
using Ddd.Domain.Adapters;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;
using Microsoft.Extensions.DependencyInjection;

namespace Ddd.Application.Extensions;

/// <summary>
/// アプリケーション層の依存関係を DI コンテナへ登録する拡張メソッドを提供する。
/// </summary>
/// <remarks>
/// 合成ルート(<c>Ddd.Api</c> の <c>Program.cs</c>)から <see cref="AddApplication"/> を呼び出し、
/// DTO ⇔ ドメインの Adapter・集約 ⇔ DTO の Factory・アプリケーションサービス・ユースケース(Interactor)を
/// 登録する。永続化(Repository・UnitOfWork・DbContext)は <c>AddInfrastructure</c> が担う。
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// アプリケーション層のサービス(Adapter・Factory・Service・Usecase)を登録する。
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <returns>連鎖呼び出し用の <paramref name="services"/>。</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 腐敗防止層(ACL)の Adapter(DTO ⇔ ドメイン)。ドメインの Adapter ポート型で登録する。
        services.AddScoped<IDomainBiAdapter<ProductDto, Product>, ProductDtoAdapter>();
        services.AddScoped<IDomainBiAdapter<CategoryDto, Category>, CategoryDtoAdapter>();
        services.AddScoped<IDomainBiAdapter<StockDto, Stock>, StockDtoAdapter>();

        // 集約 ⇔ DTO の Factory。ドメインの汎用ポート(集約ルート×外部の集約ルート)で登録する。
        services.AddScoped<IFactory<Product, ProductDto>, ProductDtoFactory>();

        // アプリケーションサービス。
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();

        // ユースケース(Interactor)。ユースケースのポート型で登録する。
        services.AddScoped<ISearchProductByNameUsecase, SearchProductByNameInteractor>();
        services.AddScoped<IRegisterProductUsecase, RegisterProductInteractor>();
        services.AddScoped<IUpdateProductUsecase, UpdateProductInteractor>();

        return services;
    }
}