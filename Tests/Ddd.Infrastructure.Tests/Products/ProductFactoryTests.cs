using Ddd.Domain.Exceptions;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Entities;
using Ddd.Infrastructure.Tests.Persistence;

namespace Ddd.Infrastructure.Tests.Products;

/// <summary>
/// <see cref="Ddd.Infrastructure.Products.ProductFactory"/>(受け皿群 ⇔ Product集約 の合成/分解)の
/// テスト(DB不要)。テスト対象は DI コンテナから解決するため、実物の Factory ＋ 実物の Adapter が
/// 注入される結合寄りの検証となる。
/// </summary>
[TestClass]
[TestCategory("Infrastructure.Products")]
public sealed class ProductFactoryTests : InfrastructureTestBase
{
    private IProductFactory<ProductEntity, ProductCategoryEntity, ProductStockEntity> Factory
        => GetRequiredService<IProductFactory<ProductEntity, ProductCategoryEntity, ProductStockEntity>>();

    private static Category SampleCategory() => Category.CreateNew(CategoryName.Create("文房具"));

    private static Product SampleSkeleton() => Product.RestoreSkeleton(
        ProductId.New(), ProductName.Create("油性ボールペン"), ProductPrice.Create(120));

    private static Product SampleFullProduct() => Product.CreateNew(
        ProductName.Create("油性ボールペン"), ProductPrice.Create(120), SampleCategory(), StockQuantity.Create(80));

    // ---- Assemble: 受け皿群 → Product集約 の合成 ----

    [TestMethod(DisplayName = "受け皿3種からProduct集約を合成する")]
    public void Assemble_ComposesAggregateFromEntities()
    {
        var productUuid = Guid.NewGuid();
        var categoryUuid = Guid.NewGuid();
        var stockUuid = Guid.NewGuid();

        var product = Factory.Assemble(
            new ProductEntity { ProductUuid = productUuid, Name = "油性ボールペン", Price = 120 },
            new ProductCategoryEntity { CategoryUuid = categoryUuid, Name = "文房具" },
            new ProductStockEntity { StockUuid = stockUuid, Quantity = 80 });

        Assert.AreEqual(productUuid, product.ProductId.Value);
        Assert.AreEqual("油性ボールペン", product.Name.Value);
        Assert.AreEqual(120, product.Price.Value);
        Assert.AreEqual(categoryUuid, product.Category!.CategoryId.Value);
        Assert.AreEqual("文房具", product.Category.Name.Value);
        Assert.AreEqual(stockUuid, product.Stock!.StockId.Value);
        Assert.AreEqual(80, product.Stock.Quantity.Value);
    }

    [TestMethod(DisplayName = "ProductEntityがnullなら例外")]
    public void Assemble_ThrowsWhenProductIsNull()
    {
        Assert.ThrowsExactly<DomainException>(
            () => Factory.Assemble(null!, new ProductCategoryEntity(), new ProductStockEntity()));
    }

    [TestMethod(DisplayName = "ProductCategoryEntityがnullなら例外")]
    public void Assemble_ThrowsWhenCategoryIsNull()
    {
        Assert.ThrowsExactly<DomainException>(
            () => Factory.Assemble(new ProductEntity(), null!, new ProductStockEntity()));
    }

    [TestMethod(DisplayName = "ProductStockEntityがnullなら例外")]
    public void Assemble_ThrowsWhenStockIsNull()
    {
        Assert.ThrowsExactly<DomainException>(
            () => Factory.Assemble(new ProductEntity(), new ProductCategoryEntity(), null!));
    }

    // ---- 分解: Product集約 → 受け皿 ----

    [TestMethod(DisplayName = "集約からProduct受け皿を作る_外部キーと主キーは未設定")]
    public void ToProduct_ConvertsWithKeysUnset()
    {
        var product = SampleFullProduct();

        var entity = Factory.ToProduct(product);

        Assert.AreEqual(product.ProductId.Value, entity.ProductUuid);
        Assert.AreEqual("油性ボールペン", entity.Name);
        Assert.AreEqual(120, entity.Price);
        Assert.AreEqual(0, entity.CategoryId); // 外部キーは未設定(Repositoryが補完)
        Assert.AreEqual(0, entity.Id);         // 主キーは未採番
    }

    [TestMethod(DisplayName = "ToProductはnullなら例外")]
    public void ToProduct_ThrowsWhenNull()
    {
        Assert.ThrowsExactly<DomainException>(() => Factory.ToProduct(null!));
    }

    [TestMethod(DisplayName = "集約からStock受け皿を作る_外部キーと主キーは未設定")]
    public void ToStock_ConvertsWithKeysUnset()
    {
        var product = SampleFullProduct();

        var entity = Factory.ToStock(product);

        Assert.AreEqual(product.Stock!.StockId.Value, entity.StockUuid);
        Assert.AreEqual(80, entity.Quantity);
        Assert.AreEqual(0, entity.ProductId); // 外部キーは未設定(Repositoryが補完)
        Assert.AreEqual(0, entity.Id);        // 主キーは未採番
    }

    [TestMethod(DisplayName = "ToStockはStock未設定なら例外")]
    public void ToStock_ThrowsWhenStockNotAttached()
    {
        Assert.ThrowsExactly<DomainException>(() => Factory.ToStock(SampleSkeleton()));
    }

    [TestMethod(DisplayName = "集約からカテゴリUuidを返す")]
    public void ExtractCategoryUuid_ReturnsUuid()
    {
        var category = SampleCategory();
        var product = Product.CreateNew(
            ProductName.Create("油性ボールペン"), ProductPrice.Create(120), category, StockQuantity.Create(80));

        Assert.AreEqual(category.CategoryId.Value, Factory.ExtractCategoryUuid(product));
    }

    [TestMethod(DisplayName = "ExtractCategoryUuidはCategory未設定なら例外")]
    public void ExtractCategoryUuid_ThrowsWhenCategoryNotAttached()
    {
        Assert.ThrowsExactly<DomainException>(() => Factory.ExtractCategoryUuid(SampleSkeleton()));
    }
}