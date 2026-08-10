using Ddd.Application.Exceptions;
using Ddd.Application.Products.Services;
using Ddd.Application.Tests.Fakes;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Application.Tests.Products.Services;

/// <summary>
/// <see cref="ProductService"/> のテスト。Fake リポジトリをシードし、テスト対象は DI から解決する。
/// </summary>
[TestClass]
[TestCategory("Application.Services")]
public sealed class ProductServiceTests : ApplicationTestBase
{
    private IProductService Service => GetRequiredService<IProductService>();
    private FakeProductRepository Products => GetRequiredService<FakeProductRepository>();

    private static Product SampleProduct(string name = "万年筆")
        => Product.CreateNew(
            ProductName.Create(name), ProductPrice.Create(3000),
            Category.CreateNew(CategoryName.Create("文房具")), StockQuantity.Create(10));

    // ---- ExistsProduct ----

    [TestMethod(DisplayName = "登録済みの商品名はExistsException")]
    public async Task ExistsProduct_ThrowsWhenExisting()
    {
        Products.Seed(SampleProduct("万年筆"));

        await Assert.ThrowsExactlyAsync<ExistsException>(
            () => Service.ExistsProductAsync(ProductName.Create("万年筆")));
    }

    [TestMethod(DisplayName = "未登録の商品名は例外を投げない")]
    public async Task ExistsProduct_DoesNotThrowWhenMissing()
    {
        await Service.ExistsProductAsync(ProductName.Create("未登録商品"));
    }

    // ---- ExistsProductExcept ----

    [TestMethod(DisplayName = "同名が更新対象自身なら例外を投げない")]
    public async Task ExistsProductExcept_DoesNotThrowForSameProduct()
    {
        var product = SampleProduct("万年筆");
        Products.Seed(product);

        await Service.ExistsProductExceptAsync(ProductName.Create("万年筆"), product.ProductId);
    }

    [TestMethod(DisplayName = "同名が別商品ならExistsException")]
    public async Task ExistsProductExcept_ThrowsForDifferentProduct()
    {
        Products.Seed(SampleProduct("万年筆"));

        await Assert.ThrowsExactlyAsync<ExistsException>(
            () => Service.ExistsProductExceptAsync(ProductName.Create("万年筆"), ProductId.New()));
    }

    [TestMethod(DisplayName = "同名が存在しなければ例外を投げない")]
    public async Task ExistsProductExcept_DoesNotThrowWhenMissing()
    {
        await Service.ExistsProductExceptAsync(ProductName.Create("未登録商品"), ProductId.New());
    }

    // ---- GetProductById / GetProductByName ----

    [TestMethod(DisplayName = "Idで商品を取得する")]
    public async Task GetProductById_ReturnsMatching()
    {
        var product = SampleProduct("万年筆");
        Products.Seed(product);

        var result = await Service.GetProductByIdAsync(product.ProductId);

        Assert.AreEqual(product.ProductId, result.ProductId);
    }

    [TestMethod(DisplayName = "存在しないIdはNotFoundException")]
    public async Task GetProductById_ThrowsWhenMissing()
    {
        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => Service.GetProductByIdAsync(ProductId.New()));
    }

    [TestMethod(DisplayName = "名前で商品を取得する")]
    public async Task GetProductByName_ReturnsMatching()
    {
        Products.Seed(SampleProduct("万年筆"));

        var result = await Service.GetProductByNameAsync(ProductName.Create("万年筆"));

        Assert.AreEqual("万年筆", result.Name.Value);
    }

    [TestMethod(DisplayName = "存在しない名前はNotFoundException")]
    public async Task GetProductByName_ThrowsWhenMissing()
    {
        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => Service.GetProductByNameAsync(ProductName.Create("未登録商品")));
    }

    // ---- AddProduct / UpdateProduct ----

    [TestMethod(DisplayName = "商品を登録できる")]
    public async Task AddProduct_Persists()
    {
        var product = SampleProduct("新商品");

        await Service.AddProductAsync(product);

        var found = await Service.GetProductByNameAsync(ProductName.Create("新商品"));
        Assert.AreEqual(product.ProductId, found.ProductId);
    }

    [TestMethod(DisplayName = "商品を更新できる")]
    public async Task UpdateProduct_Persists()
    {
        var product = SampleProduct("万年筆");
        Products.Seed(product);

        product.Reprice(ProductPrice.Create(5000));
        await Service.UpdateProductAsync(product);

        var found = await Service.GetProductByIdAsync(product.ProductId);
        Assert.AreEqual(5000, found.Price.Value);
    }
}