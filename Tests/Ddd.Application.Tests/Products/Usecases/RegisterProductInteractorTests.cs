using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;
using Ddd.Application.Products.Usecases;
using Ddd.Application.Tests.Fakes;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Application.Tests.Products;

/// <summary>
/// <see cref="Ddd.Application.Products.Usecases.Interactors.RegisterProductInteractor"/> のテスト。
/// Fake リポジトリ・Fake <c>IUnitOfWork</c> を用い、テスト対象(ユースケース)は DI から解決する。
/// </summary>
[TestClass]
[TestCategory("Application.Usecases")]
public sealed class RegisterProductInteractorTests : ApplicationTestBase
{
    private IRegisterProductUsecase Usecase => GetRequiredService<IRegisterProductUsecase>();
    private FakeCategoryRepository Categories => GetRequiredService<FakeCategoryRepository>();
    private FakeProductRepository Products => GetRequiredService<FakeProductRepository>();

    private static Product ExistingProduct(string name)
        => Product.CreateNew(
            ProductName.Create(name), ProductPrice.Create(3000),
            Category.CreateNew(CategoryName.Create("文房具")), StockQuantity.Create(5));

    // ---- GetCategories / GetCategoryById ----

    [TestMethod(DisplayName = "全カテゴリをDTOで取得する")]
    public async Task GetCategories_ReturnsDtos()
    {
        Categories.Seed(
            Category.CreateNew(CategoryName.Create("文房具")),
            Category.CreateNew(CategoryName.Create("雑貨")));

        var result = await Usecase.GetCategoriesAsync();

        Assert.HasCount(2, result);
        CollectionAssert.AreEquivalent(
            new[] { "文房具", "雑貨" },
            result.Select(c => c.Name).ToArray());
    }

    [TestMethod(DisplayName = "IdでカテゴリDTOを取得する")]
    public async Task GetCategoryById_ReturnsDto()
    {
        var category = Category.CreateNew(CategoryName.Create("文房具"));
        Categories.Seed(category);

        var dto = await Usecase.GetCategoryByIdAsync(category.CategoryId.ToString());

        Assert.AreEqual(category.CategoryId.ToString(), dto.Id);
        Assert.AreEqual("文房具", dto.Name);
    }

    [TestMethod(DisplayName = "存在しないカテゴリIdはNotFoundException")]
    public async Task GetCategoryById_ThrowsWhenMissing()
    {
        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => Usecase.GetCategoryByIdAsync(CategoryId.New().ToString()));
    }

    // ---- ExistsProduct ----

    [TestMethod(DisplayName = "登録済みの商品名はExistsException")]
    public async Task ExistsProduct_ThrowsWhenExisting()
    {
        Products.Seed(ExistingProduct("既存商品"));

        await Assert.ThrowsExactlyAsync<ExistsException>(() => Usecase.ExistsProductAsync("既存商品"));
    }

    [TestMethod(DisplayName = "未登録の商品名は例外を投げない")]
    public async Task ExistsProduct_DoesNotThrowWhenMissing()
    {
        await Usecase.ExistsProductAsync("未登録商品");
    }

    // ---- AddProduct ----

    [TestMethod(DisplayName = "商品を登録し登録結果DTOを返す_カテゴリはDBの正で上書きされる")]
    public async Task AddProduct_RegistersAndReturnsDto()
    {
        // DBにあるカテゴリ(正)。クライアントが送る名前は無視され、この名前で上書きされる。
        var category = Category.CreateNew(CategoryName.Create("文房具"));
        Categories.Seed(category);

        var input = new ProductDto(
            null, "新商品", 3000,
            new CategoryDto(category.CategoryId.ToString(), "クライアント送信名(無視される)"),
            new StockDto(null, 10));

        var result = await Usecase.AddProductAsync(input);

        Assert.IsNotNull(result.Id);            // 採番された
        Assert.AreEqual("新商品", result.Name);
        Assert.AreEqual(3000, result.Price);
        Assert.AreEqual(category.CategoryId.ToString(), result.Category!.Id);
        Assert.AreEqual("文房具", result.Category.Name);   // DB の正で上書き
        Assert.AreEqual(10, result.Stock!.Quantity);

        // 実際に登録されていること(同名が存在するので例外になる)。
        await Assert.ThrowsExactlyAsync<ExistsException>(() => Usecase.ExistsProductAsync("新商品"));
    }

    [TestMethod(DisplayName = "ProductDtoがnullならInvalidInputException")]
    public async Task AddProduct_ThrowsWhenNull()
    {
        await Assert.ThrowsExactlyAsync<InvalidInputException>(() => Usecase.AddProductAsync(null!));
    }

    [TestMethod(DisplayName = "カテゴリIdが無ければInvalidInputException")]
    public async Task AddProduct_ThrowsWhenCategoryIdMissing()
    {
        var input = new ProductDto(null, "新商品", 3000, new CategoryDto(null, "文房具"), new StockDto(null, 10));

        var ex = await Assert.ThrowsExactlyAsync<InvalidInputException>(() => Usecase.AddProductAsync(input));
        Assert.AreEqual("商品カテゴリIDは必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "指定カテゴリが存在しなければNotFoundException")]
    public async Task AddProduct_ThrowsWhenCategoryNotFound()
    {
        var input = new ProductDto(
            null, "新商品", 3000,
            new CategoryDto(CategoryId.New().ToString(), "文房具"),
            new StockDto(null, 10));

        await Assert.ThrowsExactlyAsync<NotFoundException>(() => Usecase.AddProductAsync(input));
    }

    [TestMethod(DisplayName = "同名商品が既に存在すればExistsException")]
    public async Task AddProduct_ThrowsWhenDuplicateName()
    {
        var category = Category.CreateNew(CategoryName.Create("文房具"));
        Categories.Seed(category);
        Products.Seed(ExistingProduct("重複商品"));

        var input = new ProductDto(
            null, "重複商品", 3000,
            new CategoryDto(category.CategoryId.ToString(), "文房具"),
            new StockDto(null, 10));

        await Assert.ThrowsExactlyAsync<ExistsException>(() => Usecase.AddProductAsync(input));
    }
}