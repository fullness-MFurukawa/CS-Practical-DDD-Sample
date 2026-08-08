using Ddd.Domain.Adapters;
using Ddd.Domain.Exceptions;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Entities;

namespace Ddd.Infrastructure.Products;

/// <summary>
/// Product 集約の合成/分解を担うファクトリの EF Core 実装。
/// ドメインの <see cref="IProductFactory{TProduct,TCategory,TStock}"/> を、受け皿(EF エンティティ)に
/// バインドして実装する。
/// </summary>
/// <remarks>
/// 責務は<b>型変換と合成/分解のみ</b>で、永続化(SQL 実行)は Repository が担う。
/// 個々の受け皿 ↔ ドメイン の変換は各 Adapter(インターフェイス経由)に委譲する。
/// </remarks>
public sealed class ProductFactory(
    IDomainBiAdapter<ProductEntity, Product> productAdapter,
    IToDomainAdapter<ProductCategoryEntity, Category> categoryAdapter,
    IDomainBiAdapter<ProductStockEntity, Stock> stockAdapter)
    : IProductFactory<ProductEntity, ProductCategoryEntity, ProductStockEntity>
{
    /// <inheritdoc />
    public Product Assemble(ProductEntity product, ProductCategoryEntity category, ProductStockEntity stock)
    {
        if (product is null)
        {
            throw new DomainException("ProductEntity が null です。");
        }
        if (category is null)
        {
            throw new DomainException("ProductCategoryEntity が null です。");
        }
        if (stock is null)
        {
            throw new DomainException("ProductStockEntity が null です。");
        }

        // 骨格を復元し、カテゴリ・在庫を後から合成する
        var aggregate = productAdapter.ToDomain(product); // skeleton
        aggregate.AttachCategory(categoryAdapter.ToDomain(category));
        aggregate.AttachStock(stockAdapter.ToDomain(stock));
        return aggregate;
    }

    /// <inheritdoc />
    public ProductEntity ToProduct(Product product)
    {
        if (product is null)
        {
            throw new DomainException("Product が null です。");
        }
        return productAdapter.FromDomain(product);
    }

    /// <inheritdoc />
    public ProductStockEntity ToStock(Product product)
    {
        if (product is null)
        {
            throw new DomainException("Product が null です。");
        }
        var stock = product.Stock;
        if (stock is null)
        {
            throw new DomainException("Product に Stock が設定されていません。");
        }
        return stockAdapter.FromDomain(stock);
    }

    /// <inheritdoc />
    public Guid ExtractCategoryUuid(Product product)
    {
        if (product is null)
        {
            throw new DomainException("Product が null です。");
        }
        var category = product.Category;
        if (category is null)
        {
            throw new DomainException("Product に Category が設定されていません。");
        }
        return category.CategoryId.Value;
    }
}