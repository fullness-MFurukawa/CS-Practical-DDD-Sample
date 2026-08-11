using System.Net;
using System.Net.Http.Json;
using Ddd.Application.Dtos;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Api.Tests.Products;

/// <summary>
/// <c>SearchProductByNameController</c>(<c>GET /api/products/search</c>)の HTTP 経由テスト。
/// </summary>
[TestClass]
[TestCategory("Api.Products")]
public sealed class SearchProductControllerTests : ApiTestBase
{
    private static Product SampleProduct(string name = "万年筆")
        => Product.CreateNew(
            ProductName.Create(name), ProductPrice.Create(3000),
            Category.CreateNew(CategoryName.Create("文房具")), StockQuantity.Create(10));

    [TestMethod(DisplayName = "存在する商品名で200とネストしたDTOを返す")]
    public async Task Search_ReturnsOkWithProduct()
    {
        Factory.Products.Seed(SampleProduct("万年筆"));

        var response = await Client.GetAsync("/api/products/search?name=万年筆");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.IsNotNull(dto);
        Assert.AreEqual("万年筆", dto!.Name);
        Assert.AreEqual(3000, dto.Price);
        Assert.AreEqual("文房具", dto.Category!.Name);
        Assert.AreEqual(10, dto.Stock!.Quantity);
    }

    [TestMethod(DisplayName = "存在しない商品名は404を返す")]
    public async Task Search_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/products/search?name=存在しない商品");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod(DisplayName = "nameを指定しないと400を返す")]
    public async Task Search_ReturnsBadRequest_WhenNameMissing()
    {
        var response = await Client.GetAsync("/api/products/search");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}