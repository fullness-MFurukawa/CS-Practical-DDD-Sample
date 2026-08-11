using System.Net;
using System.Net.Http.Json;
using Ddd.Application.Dtos;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Api.Tests.Products;

/// <summary>
/// <c>UpdateProductController</c>(<c>GET /api/products/{id}</c>・<c>PUT /api/products/{id}</c>)の
/// HTTP 経由テスト。
/// </summary>
/// <remarks>
/// PUT のボディは実クライアントを模して匿名オブジェクト(<c>name</c>/<c>price</c>/<c>stockQuantity</c>)で送り、
/// 商品Idは URI のパスで渡す。ステータス・本文・検証(400)・不在(404)・重複(409)を検証する。
/// ドメインイベントの配送はユースケース内で行われ、ハンドラ(ログ出力のみ)は本物のまま動作する。
/// </remarks>
[TestClass]
[TestCategory("Api.Products")]
public sealed class UpdateProductControllerTests : ApiTestBase
{
    private static Product SampleProduct(string name = "万年筆", int price = 3000, int qty = 10)
        => Product.CreateNew(
            ProductName.Create(name), ProductPrice.Create(price),
            Category.CreateNew(CategoryName.Create("文房具")), StockQuantity.Create(qty));

    // ---- GET /api/products/{id} ----

    [TestMethod(DisplayName = "商品をIdで200で返す(変更用取得)")]
    public async Task GetById_ReturnsOk()
    {
        var product = SampleProduct("万年筆");
        Factory.Products.Seed(product);

        var response = await Client.GetAsync($"/api/products/{product.ProductId}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.AreEqual("万年筆", dto!.Name);
        Assert.AreEqual(3000, dto.Price);
        Assert.AreEqual(10, dto.Stock!.Quantity);
    }

    [TestMethod(DisplayName = "存在しないIdの取得は404")]
    public async Task GetById_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/products/{ProductId.New()}");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---- PUT /api/products/{id} : 正常系 ----

    [TestMethod(DisplayName = "商品を変更すると200と変更後DTOを返す")]
    public async Task Update_ReturnsOk()
    {
        var product = SampleProduct("万年筆", 3000, 10);
        Factory.Products.Seed(product);

        var body = new { name = "筆ペン", price = 3500, stockQuantity = 42 };
        var response = await Client.PutAsJsonAsync($"/api/products/{product.ProductId}", body);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.AreEqual("筆ペン", dto!.Name);
        Assert.AreEqual(3500, dto.Price);
        Assert.AreEqual(42, dto.Stock!.Quantity);
        // カテゴリは変更対象外のため元のまま
        Assert.AreEqual("文房具", dto.Category!.Name);
    }

    // ---- PUT /api/products/{id} : 検証・不在・重複 ----

    [TestMethod(DisplayName = "単価が範囲外だと400")]
    public async Task Update_ReturnsBadRequest_WhenPriceOutOfRange()
    {
        var product = SampleProduct("万年筆", 3000, 10);
        Factory.Products.Seed(product);

        // 単価は 50〜10000 が有効。範囲外(10001)を送る。
        var body = new { name = "万年筆", price = 10001, stockQuantity = 10 };
        var response = await Client.PutAsJsonAsync($"/api/products/{product.ProductId}", body);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod(DisplayName = "商品名が欠けると400")]
    public async Task Update_ReturnsBadRequest_WhenNameMissing()
    {
        var product = SampleProduct("万年筆", 3000, 10);
        Factory.Products.Seed(product);

        // name を欠落させる
        var body = new { price = 3500, stockQuantity = 42 };
        var response = await Client.PutAsJsonAsync($"/api/products/{product.ProductId}", body);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod(DisplayName = "存在しない商品の変更は404")]
    public async Task Update_ReturnsNotFound()
    {
        var body = new { name = "筆ペン", price = 3500, stockQuantity = 42 };
        var response = await Client.PutAsJsonAsync($"/api/products/{ProductId.New()}", body);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod(DisplayName = "変更後の名前が別商品で使用中なら409")]
    public async Task Update_ReturnsConflict_WhenNameUsedByAnother()
    {
        var target = SampleProduct("万年筆", 3000, 10);
        var other = SampleProduct("筆ペン", 3000, 10); // 別商品が「筆ペン」を使用中
        Factory.Products.Seed(target, other);

        // target を「筆ペン」に変更しようとすると重複
        var body = new { name = "筆ペン", price = 3000, stockQuantity = 10 };
        var response = await Client.PutAsJsonAsync($"/api/products/{target.ProductId}", body);

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }
}