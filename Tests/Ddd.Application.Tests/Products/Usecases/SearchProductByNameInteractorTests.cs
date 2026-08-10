using Ddd.Application.Exceptions;
using Ddd.Application.Products.Usecases;
using Ddd.Application.Tests.Fakes;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Application.Tests.Products;

/// <summary>
/// <see cref="Ddd.Application.Products.Usecases.Interactors.SearchProductByNameInteractor"/> のテスト。
/// Fake リポジトリをシードし、テスト対象(ユースケース)は DI から解決する。
/// </summary>
[TestClass]
[TestCategory("Application.Usecases")]
public sealed class SearchProductByNameInteractorTests : ApplicationTestBase
{
    private ISearchProductByNameUsecase Usecase => GetRequiredService<ISearchProductByNameUsecase>();
    private FakeProductRepository Products => GetRequiredService<FakeProductRepository>();

    [TestMethod(DisplayName = "商品名で検索しネストしたDTOを返す")]
    public async Task Search_ReturnsNestedDto()
    {
        var product = Product.CreateNew(
            ProductName.Create("万年筆"), ProductPrice.Create(3000),
            Category.CreateNew(CategoryName.Create("文房具")), StockQuantity.Create(10));
        Products.Seed(product);

        var dto = await Usecase.SearchAsync("万年筆");

        Assert.AreEqual("万年筆", dto.Name);
        Assert.AreEqual(3000, dto.Price);
        Assert.AreEqual("文房具", dto.Category!.Name);
        Assert.AreEqual(10, dto.Stock!.Quantity);
    }

    [TestMethod(DisplayName = "存在しない商品名はNotFoundException")]
    public async Task Search_ThrowsWhenMissing()
    {
        await Assert.ThrowsExactlyAsync<NotFoundException>(() => Usecase.SearchAsync("未登録商品"));
    }
}