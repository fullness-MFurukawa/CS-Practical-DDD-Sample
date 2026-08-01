using Ddd.Domain.Exceptions;
using Ddd.Domain.Mappers;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Entities;
using Ddd.Infrastructure.Products;

namespace Ddd.Infrastructure.Tests.Products;

/// <summary>
/// <see cref="ProductAssembler"/>(Entity群 ⇔ Product集約 の合成/分解)の単体テスト(DB不要)。
/// </summary>
/// <remarks>
/// Assembler の責務は「3つの Mapper への委譲」と「集約の合成/分解」なので、各 Mapper は
/// 手書きのフェイク(テストダブル)に差し替え、Assembler 自身のロジック(skeleton への Attach 合成・
/// null ガード・委譲)だけを検証する。Mapperly / EF Core には依存しない。
/// </remarks>
[TestClass]
[TestCategory("Infrastructure.Products")]
public sealed class ProductAssemblerTests
{
    // ---- テストダブル(フェイク Mapper) ----

    private sealed class FakeProductMapper : IDomainBiMapper<ProductEntity, Product>
    {
        public Product? ToDomainResult;
        public ProductEntity? FromDomainResult;
        public Product ToDomain(ProductEntity input) => ToDomainResult!;
        public ProductEntity FromDomain(Product domain) => FromDomainResult!;
    }

    private sealed class FakeCategoryMapper : IToDomainMapper<ProductCategoryEntity, Category>
    {
        public Category? ToDomainResult;
        public Category ToDomain(ProductCategoryEntity input) => ToDomainResult!;
    }

    private sealed class FakeStockMapper : IDomainBiMapper<ProductStockEntity, Stock>
    {
        public Stock? ToDomainResult;
        public ProductStockEntity? FromDomainResult;
        public Stock ToDomain(ProductStockEntity input) => ToDomainResult!;
        public ProductStockEntity FromDomain(Stock domain) => FromDomainResult!;
    }

    // ---- サンプル生成ヘルパ ----

    private static Category SampleCategory() => Category.CreateNew(CategoryName.Create("文房具"));

    private static Product SampleSkeleton() => Product.RestoreSkeleton(
        ProductId.New(), ProductName.Create("油性ボールペン"), ProductPrice.Create(120));

    private static Product SampleFullProduct() => Product.CreateNew(
        ProductName.Create("油性ボールペン"), ProductPrice.Create(120), SampleCategory(), StockQuantity.Create(80));

    // ---- Assemble: Entity群 → Product集約 の合成 ----

    [TestMethod]
    public void Assemble_骨格にCategoryとStockをAttachして合成する()
    {
        var skeleton = SampleSkeleton();
        var category = SampleCategory();
        var stock = Stock.CreateNew(StockQuantity.Create(80));

        var assembler = new ProductAssembler(
            new FakeProductMapper { ToDomainResult = skeleton },
            new FakeCategoryMapper { ToDomainResult = category },
            new FakeStockMapper { ToDomainResult = stock });

        var result = assembler.Assemble(new ProductEntity(), new ProductCategoryEntity(), new ProductStockEntity());

        // 返却されるのは Attach 済みの skeleton そのもの
        Assert.AreSame(skeleton, result);
        Assert.AreSame(category, result.Category);
        Assert.AreSame(stock, result.Stock);
    }

    [TestMethod]
    public void Assemble_ProductEntityがnullなら例外()
    {
        var assembler = new ProductAssembler(new FakeProductMapper(), new FakeCategoryMapper(), new FakeStockMapper());
        Assert.ThrowsExactly<DomainException>(
            () => assembler.Assemble(null!, new ProductCategoryEntity(), new ProductStockEntity()));
    }

    [TestMethod]
    public void Assemble_ProductCategoryEntityがnullなら例外()
    {
        var assembler = new ProductAssembler(new FakeProductMapper(), new FakeCategoryMapper(), new FakeStockMapper());
        Assert.ThrowsExactly<DomainException>(
            () => assembler.Assemble(new ProductEntity(), null!, new ProductStockEntity()));
    }

    [TestMethod]
    public void Assemble_ProductStockEntityがnullなら例外()
    {
        var assembler = new ProductAssembler(new FakeProductMapper(), new FakeCategoryMapper(), new FakeStockMapper());
        Assert.ThrowsExactly<DomainException>(
            () => assembler.Assemble(new ProductEntity(), new ProductCategoryEntity(), null!));
    }

    // ---- 分解: Product集約 → Entity ----

    [TestMethod]
    public void ToProductEntity_ProductMapperのFromDomainに委譲する()
    {
        var expected = new ProductEntity();
        var assembler = new ProductAssembler(
            new FakeProductMapper { FromDomainResult = expected }, new FakeCategoryMapper(), new FakeStockMapper());

        Assert.AreSame(expected, assembler.ToProductEntity(SampleFullProduct()));
    }

    [TestMethod]
    public void ToProductEntity_nullなら例外()
    {
        var assembler = new ProductAssembler(new FakeProductMapper(), new FakeCategoryMapper(), new FakeStockMapper());
        Assert.ThrowsExactly<DomainException>(() => assembler.ToProductEntity(null!));
    }

    [TestMethod]
    public void ToStockEntity_ProductのStockを取り出して委譲する()
    {
        var expected = new ProductStockEntity();
        var assembler = new ProductAssembler(
            new FakeProductMapper(), new FakeCategoryMapper(), new FakeStockMapper { FromDomainResult = expected });

        Assert.AreSame(expected, assembler.ToStockEntity(SampleFullProduct()));
    }

    [TestMethod]
    public void ToStockEntity_Stock未設定なら例外()
    {
        var assembler = new ProductAssembler(new FakeProductMapper(), new FakeCategoryMapper(), new FakeStockMapper());
        Assert.ThrowsExactly<DomainException>(() => assembler.ToStockEntity(SampleSkeleton()));
    }

    [TestMethod]
    public void ExtractCategoryUuid_CategoryのUuidを返す()
    {
        var category = SampleCategory();
        var product = Product.CreateNew(
            ProductName.Create("油性ボールペン"), ProductPrice.Create(120), category, StockQuantity.Create(80));
        var assembler = new ProductAssembler(new FakeProductMapper(), new FakeCategoryMapper(), new FakeStockMapper());

        Assert.AreEqual(category.CategoryId.Value, assembler.ExtractCategoryUuid(product));
    }

    [TestMethod]
    public void ExtractCategoryUuid_Category未設定なら例外()
    {
        var assembler = new ProductAssembler(new FakeProductMapper(), new FakeCategoryMapper(), new FakeStockMapper());
        Assert.ThrowsExactly<DomainException>(() => assembler.ExtractCategoryUuid(SampleSkeleton()));
    }
}