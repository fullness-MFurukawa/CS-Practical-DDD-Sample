using System.Net;
using System.Net.Http.Json;
using Ddd.Application.Dtos;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Api.Tests.Products;

/// <summary>
/// <c>RegisterProductController</c>(カテゴリ参照・存在チェック・商品登録)の HTTP 経由テスト。
/// </summary>
[TestClass]
[TestCategory("Api.Products")]
public sealed class RegisterProductControllerTests : ApiTestBase
{
    private static Category SampleCategory() => Category.CreateNew(CategoryName.Create("文房具"));

    private static Product SampleProduct(string name)
        => Product.CreateNew(
            ProductName.Create(name), ProductPrice.Create(3000), SampleCategory(), StockQuantity.Create(5));

    // ---- GET /categories ----

    [TestMethod(DisplayName = "カテゴリ一覧を200で返す")]
    public async Task GetCategories_ReturnsOk()
    {
        Factory.Categories.Seed(
            Category.CreateNew(CategoryName.Create("文房具")),
            Category.CreateNew(CategoryName.Create("雑貨")));

        var response = await Client.GetAsync("/api/products/categories");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        Assert.HasCount(2, list!);
    }

    // ---- GET /categories/{id} ----

    [TestMethod(DisplayName = "カテゴリをIdで200で返す")]
    public async Task GetCategoryById_ReturnsOk()
    {
        var category = SampleCategory();
        Factory.Categories.Seed(category);

        var response = await Client.GetAsync($"/api/products/categories/{category.CategoryId}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.AreEqual("文房具", dto!.Name);
    }

    [TestMethod(DisplayName = "存在しないカテゴリIdは404")]
    public async Task GetCategoryById_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/products/categories/{CategoryId.New()}");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---- GET /exists ----

    [TestMethod(DisplayName = "存在する商品名は409")]
    public async Task Exists_ReturnsConflict()
    {
        Factory.Products.Seed(SampleProduct("既存商品"));

        var response = await Client.GetAsync("/api/products/exists?name=既存商品");

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }

    [TestMethod(DisplayName = "未登録の商品名は204")]
    public async Task Exists_ReturnsNoContent()
    {
        var response = await Client.GetAsync("/api/products/exists?name=未登録商品");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ---- POST /api/products ----

    [TestMethod(DisplayName = "商品を登録すると201とLocationを返す")]
    public async Task Register_ReturnsCreated()
    {
        var category = SampleCategory();
        Factory.Categories.Seed(category);

        var body = new { name = "新商品", price = 3000, categoryId = category.CategoryId.ToString(), stockQuantity = 10 };
        var response = await Client.PostAsJsonAsync("/api/products", body);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(response.Headers.Location);
        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.AreEqual("新商品", dto!.Name);
        Assert.AreEqual("文房具", dto.Category!.Name); // DBの正で上書き
        Assert.IsNotNull(dto.Id);
    }

    [TestMethod(DisplayName = "必須項目が欠けると400")]
    public async Task Register_ReturnsBadRequest_WhenInvalid()
    {
        var category = SampleCategory();
        Factory.Categories.Seed(category);

        // name を欠落させる
        var body = new { price = 3000, categoryId = category.CategoryId.ToString(), stockQuantity = 10 };
        var response = await Client.PostAsJsonAsync("/api/products", body);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod(DisplayName = "指定カテゴリが無いと404")]
    public async Task Register_ReturnsNotFound_WhenCategoryMissing()
    {
        var body = new { name = "新商品", price = 3000, categoryId = CategoryId.New().ToString(), stockQuantity = 10 };
        var response = await Client.PostAsJsonAsync("/api/products", body);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod(DisplayName = "同名商品が既にあると409")]
    public async Task Register_ReturnsConflict_WhenDuplicate()
    {
        var category = SampleCategory();
        Factory.Categories.Seed(category);
        Factory.Products.Seed(SampleProduct("重複商品"));

        var body = new { name = "重複商品", price = 3000, categoryId = category.CategoryId.ToString(), stockQuantity = 10 };
        var response = await Client.PostAsJsonAsync("/api/products", body);

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }
}