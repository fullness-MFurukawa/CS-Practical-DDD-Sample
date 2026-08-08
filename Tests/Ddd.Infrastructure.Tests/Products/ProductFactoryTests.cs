using Ddd.Infrastructure.Tests.Persistence;
using Ddd.Domain.Exceptions;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Entities;

namespace Ddd.Infrastructure.Tests.Products;

/// <summary>
/// <see cref="Ddd.Infrastructure.Products.ProductFactory"/>
/// (受け皿 <see cref="ProductEntity"/> ⇔ Product集約 の合成/分解)のテスト(DB不要)。
/// テスト対象は DI コンテナから解決するため、実物の Factory ＋ 実物の Adapter が注入される結合寄りの検証となる。
/// </summary>
[TestClass]
[TestCategory("Infrastructure.Products")]
public sealed class ProductFactoryTests : InfrastructureTestBase
{
    private IFactory<Product, ProductEntity> Factory
        => GetRequiredService<IFactory<Product, ProductEntity>>();

    private static Category SampleCategory() => Category.CreateNew(CategoryName.Create("文房具"));

    private static Product SampleSkeleton() => Product.RestoreSkeleton(
        ProductId.New(), ProductName.Create("油性ボールペン"), ProductPrice.Create(120));

    private static Product SampleFullProduct() => Product.CreateNew(
        ProductName.Create("油性ボールペン"), ProductPrice.Create(120), SampleCategory(), StockQuantity.Create(80));

    // ---- Assemble: 受け皿(ネストしたカテゴリ・在庫)→ Product集約 の合成 ----

    [TestMethod(DisplayName = "受け皿からProduct集約を合成する")]
    public void Assemble_ComposesAggregateFromEntity()
    {
        var productUuid = Guid.NewGuid();
        var categoryUuid = Guid.NewGuid();
        var stockUuid = Guid.NewGuid();

        var entity = new ProductEntity
        {
            ProductUuid = productUuid,
            Name = "油性ボールペン",
            Price = 120,
            Category = new ProductCategoryEntity { CategoryUuid = categoryUuid, Name = "文房具" },
            Stock = new ProductStockEntity { StockUuid = stockUuid, Quantity = 80 },
        };

        var product = Factory.Assemble(entity);

        Assert.AreEqual(productUuid, product.ProductId.Value);
        Assert.AreEqual("油性ボールペン", product.Name.Value);
        Assert.AreEqual(120, product.Price.Value);
        Assert.AreEqual(categoryUuid, product.Category!.CategoryId.Value);
        Assert.AreEqual("文房具", product.Category.Name.Value);
        Assert.AreEqual(stockUuid, product.Stock!.StockId.Value);
        Assert.AreEqual(80, product.Stock.Quantity.Value);
    }

    [TestMethod(DisplayName = "ProductEntityがnullなら例外")]
    public void Assemble_ThrowsWhenEntityIsNull()
    {
        Assert.ThrowsExactly<DomainException>(() => Factory.Assemble(null!));
    }

    [TestMethod(DisplayName = "カテゴリが読み込まれていなければ例外")]
    public void Assemble_ThrowsWhenCategoryNotLoaded()
    {
        var entity = new ProductEntity
        {
            ProductUuid = Guid.NewGuid(),
            Name = "油性ボールペン",
            Price = 120,
            Category = null, // Include されていない
            Stock = new ProductStockEntity { StockUuid = Guid.NewGuid(), Quantity = 80 },
        };

        Assert.ThrowsExactly<DomainException>(() => Factory.Assemble(entity));
    }

    [TestMethod(DisplayName = "在庫が読み込まれていなければ例外")]
    public void Assemble_ThrowsWhenStockNotLoaded()
    {
        var entity = new ProductEntity
        {
            ProductUuid = Guid.NewGuid(),
            Name = "油性ボールペン",
            Price = 120,
            Category = new ProductCategoryEntity { CategoryUuid = Guid.NewGuid(), Name = "文房具" },
            Stock = null, // Include されていない
        };

        Assert.ThrowsExactly<DomainException>(() => Factory.Assemble(entity));
    }

    // ---- Disassemble: Product集約 → 受け皿(在庫をネスト)の分解 ----

    [TestMethod(DisplayName = "集約を受け皿に分解する_在庫はネストされ主キーと外部キーは未設定")]
    public void Disassemble_ConvertsWithNestedStockAndKeysUnset()
    {
        var product = SampleFullProduct();

        var entity = Factory.Disassemble(product);

        Assert.AreEqual(product.ProductId.Value, entity.ProductUuid);
        Assert.AreEqual("油性ボールペン", entity.Name);
        Assert.AreEqual(120, entity.Price);
        Assert.AreEqual(0, entity.CategoryId); // 外部キーは未設定(Repositoryが解決)
        Assert.AreEqual(0, entity.Id);         // 主キーは未採番
        Assert.IsNull(entity.Category);        // カテゴリはネストしない(Repositoryが参照を解決)

        // 在庫は所有ナビゲーションとしてネストされる
        Assert.IsNotNull(entity.Stock);
        Assert.AreEqual(product.Stock!.StockId.Value, entity.Stock!.StockUuid);
        Assert.AreEqual(80, entity.Stock.Quantity);
        Assert.AreEqual(0, entity.Stock.Id);        // 主キーは未採番
        Assert.AreEqual(0, entity.Stock.ProductId); // 外部キーは未設定(EFが補完)
    }

    [TestMethod(DisplayName = "Disassembleはnullなら例外")]
    public void Disassemble_ThrowsWhenNull()
    {
        Assert.ThrowsExactly<DomainException>(() => Factory.Disassemble(null!));
    }

    [TestMethod(DisplayName = "Disassembleは在庫未設定なら例外")]
    public void Disassemble_ThrowsWhenStockNotAttached()
    {
        Assert.ThrowsExactly<DomainException>(() => Factory.Disassemble(SampleSkeleton()));
    }
}