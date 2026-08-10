using Ddd.Application.Dtos;
using Ddd.Application.Products.Services;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Products;

namespace Ddd.Application.Products.Usecases.Interactors;

/// <summary>
/// ユースケース「商品を名前で検索する」の実装(Interactor)。
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IProductService"/> で商品名から <see cref="Product"/> 集約を取得し、
/// 汎用ファクトリ <see cref="IFactory{TAggregate, TExternal}"/>(<c>Product</c> ⇔ <see cref="ProductDto"/>)の
/// <c>Disassemble</c> で DTO へ変換して返す。
/// </para>
/// <para>
/// 検索は読み取りのみのため、明示的なトランザクション境界(<c>IUnitOfWork</c>)は用いない。
/// </para>
/// </remarks>
/// <param name="service">商品のアプリケーションサービス。</param>
/// <param name="factory">商品集約 ⇔ 商品 DTO のファクトリ。</param>
public sealed class SearchProductByNameInteractor(
    IProductService service,
    IFactory<Product, ProductDto> factory) : ISearchProductByNameUsecase
{
    /// <inheritdoc />
    public async Task<ProductDto> SearchAsync(string name, CancellationToken cancellationToken = default)
    {
        // 名前で商品を検索する(該当なしは NotFoundException)。
        var product = await service.GetProductByNameAsync(ProductName.Create(name), cancellationToken);

        // Product 集約を ProductDto に変換して返す。
        return factory.Disassemble(product);
    }
}