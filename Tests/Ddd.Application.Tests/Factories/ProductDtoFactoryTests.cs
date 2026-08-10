using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Application.Tests.Factories;

/// <summary>
/// <see cref="Ddd.Application.Factories.ProductDtoFactory"/>
/// (<see cref="IFactory{TAggregate, TExternal}"/> = <c>Product</c> ⇔ <see cref="ProductDto"/>)のテスト。
/// テスト対象は DI から解決し、実物の Adapter が注入される結合寄りの検証となる。
/// </summary>
[TestClass]
[TestCategory("Application.Factories")]
public sealed class ProductDtoFactoryTests : ApplicationTestBase
{
    private IFactory<Product, ProductDto> Factory
        => GetRequiredService<IFactory<Product, ProductDto>>();

    private const string ProductUuid = "11111111-1111-1111-1111-111111111111";
    private const string CategoryUuid = "22222222-2222-2222-2222-222222222222";
    private const string StockUuid = "33333333-3333-3333-3333-333333333333";

    private static ProductDto FullDto() => new(
        ProductUuid, "万年筆", 3000,
        new CategoryDto(CategoryUuid, "文房具"),
        new StockDto(StockUuid, 10));

    [TestMethod(DisplayName = "DTOからProduct集約を合成する")]
    public void Assemble_ComposesAggregate()
    {
        var product = Factory.Assemble(FullDto());

        Assert.AreEqual(ProductUuid, product.ProductId.ToString());
        Assert.AreEqual("万年筆", product.Name.Value);
        Assert.AreEqual(3000, product.Price.Value);
        Assert.AreEqual(CategoryUuid, product.Category!.CategoryId.ToString());
        Assert.AreEqual("文房具", product.Category.Name.Value);
        Assert.AreEqual(10, product.CurrentStock().Value);
    }

    [TestMethod(DisplayName = "ProductDtoがnullなら例外")]
    public void Assemble_ThrowsWhenProductNull()
        => Assert.ThrowsExactly<InvalidInputException>(() => Factory.Assemble(null!));

    [TestMethod(DisplayName = "カテゴリDTOがnullなら例外")]
    public void Assemble_ThrowsWhenCategoryNull()
    {
        var dto = new ProductDto(ProductUuid, "万年筆", 3000, null, new StockDto(StockUuid, 10));
        var ex = Assert.ThrowsExactly<InvalidInputException>(() => Factory.Assemble(dto));
        Assert.AreEqual("CategoryDtoがnullです。", ex.Message);
    }

    [TestMethod(DisplayName = "在庫DTOがnullなら例外")]
    public void Assemble_ThrowsWhenStockNull()
    {
        var dto = new ProductDto(ProductUuid, "万年筆", 3000, new CategoryDto(CategoryUuid, "文房具"), null);
        var ex = Assert.ThrowsExactly<InvalidInputException>(() => Factory.Assemble(dto));
        Assert.AreEqual("StockDtoがnullです。", ex.Message);
    }

    [TestMethod(DisplayName = "Product集約をネストしたDTOに分解する")]
    public void Disassemble_ConvertsToNestedDto()
    {
        var product = Product.Restore(
            ProductId.Parse(ProductUuid), ProductName.Create("万年筆"), ProductPrice.Create(3000),
            Category.Restore(CategoryId.Parse(CategoryUuid), CategoryName.Create("文房具")),
            Stock.Restore(StockId.Parse(StockUuid), StockQuantity.Create(10)));

        var dto = Factory.Disassemble(product);

        Assert.AreEqual(ProductUuid, dto.Id);
        Assert.AreEqual("万年筆", dto.Name);
        Assert.AreEqual(3000, dto.Price);
        Assert.AreEqual(CategoryUuid, dto.Category!.Id);
        Assert.AreEqual("文房具", dto.Category.Name);
        Assert.AreEqual(StockUuid, dto.Stock!.Id);
        Assert.AreEqual(10, dto.Stock.Quantity);
    }

    [TestMethod(DisplayName = "Productがnullなら例外")]
    public void Disassemble_ThrowsWhenNull()
        => Assert.ThrowsExactly<InvalidInputException>(() => Factory.Disassemble(null!));
}