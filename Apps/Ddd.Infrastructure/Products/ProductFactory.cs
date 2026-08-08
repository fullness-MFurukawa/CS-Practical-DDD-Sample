using Ddd.Domain.Adapters;
using Ddd.Domain.Exceptions;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Entities;

namespace Ddd.Infrastructure.Products;

// Products/ProductFactory.cs
public sealed class ProductFactory(
    IDomainBiAdapter<ProductEntity, Product> productAdapter,
    IToDomainAdapter<ProductCategoryEntity, Category> categoryAdapter,
    IDomainBiAdapter<ProductStockEntity, Stock> stockAdapter)
    : IFactory<Product, ProductEntity>
{
    public Product Assemble(ProductEntity external)
    {
        if (external is null) throw new DomainException("ProductEntity が null です。");
        if (external.Category is null) throw new DomainException("商品にカテゴリが読み込まれていません。");
        if (external.Stock is null) throw new DomainException("商品に在庫が読み込まれていません。");

        var aggregate = productAdapter.ToDomain(external); // skeleton
        aggregate.AttachCategory(categoryAdapter.ToDomain(external.Category));
        aggregate.AttachStock(stockAdapter.ToDomain(external.Stock));
        return aggregate;
    }

    public ProductEntity Disassemble(Product domain)
    {
        if (domain is null) throw new DomainException("Product が null です。");
        if (domain.Stock is null) throw new DomainException("Product に Stock が設定されていません。");

        var entity = productAdapter.FromDomain(domain);        // product_uuid / name / price
        entity.Stock = stockAdapter.FromDomain(domain.Stock);  // 所有する在庫をネスト
        return entity;
    }
}