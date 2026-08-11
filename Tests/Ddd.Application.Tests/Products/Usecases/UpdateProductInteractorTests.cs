using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;
using Ddd.Application.Products.Usecases;
using Ddd.Application.Tests.Fakes;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Products.Events;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Application.Tests.Products.Usecases;

/// <summary>
/// <see cref="Ddd.Application.Products.Usecases.Interactors.UpdateProductInteractor"/> のテスト。
/// Fake リポジトリ・Fake <c>IUnitOfWork</c>・記録用 Fake ディスパッチャを用い、テスト対象は DI から解決する。
/// </summary>
[TestClass]
[TestCategory("Application.Usecases")]
public sealed class UpdateProductInteractorTests : ApplicationTestBase
{
    private IUpdateProductUsecase Usecase => GetRequiredService<IUpdateProductUsecase>();
    private FakeProductRepository Products => GetRequiredService<FakeProductRepository>();
    private FakeDomainEventDispatcher Dispatcher => GetRequiredService<FakeDomainEventDispatcher>();

    private static Product SampleProduct(string name = "万年筆", int price = 3000, int qty = 10)
        => Product.CreateNew(
            ProductName.Create(name), ProductPrice.Create(price),
            Category.CreateNew(CategoryName.Create("文房具")), StockQuantity.Create(qty));

    /// <summary>変更用の入力DTO(カテゴリは変更対象外のため null)。</summary>
    private static ProductDto UpdateDto(string id, string name, int price, int qty)
        => new(id, name, price, category: null, stock: new StockDto(null, qty));

    // ---- GetProduct ----

    [TestMethod(DisplayName = "GetProductでIdから商品DTOを取得する")]
    public async Task GetProduct_ReturnsDto()
    {
        var product = SampleProduct();
        Products.Seed(product);

        var dto = await Usecase.GetProductAsync(product.ProductId.ToString());

        Assert.AreEqual("万年筆", dto.Name);
        Assert.AreEqual(3000, dto.Price);
        Assert.AreEqual(10, dto.Stock!.Quantity);
    }

    [TestMethod(DisplayName = "GetProductでId空白はInvalidInputException")]
    public async Task GetProduct_ThrowsWhenIdBlank()
        => await Assert.ThrowsExactlyAsync<InvalidInputException>(() => Usecase.GetProductAsync("  "));

    [TestMethod(DisplayName = "GetProductで存在しないIdはNotFoundException")]
    public async Task GetProduct_ThrowsWhenMissing()
        => await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => Usecase.GetProductAsync(ProductId.New().ToString()));

    // ---- Update: 正常系とイベント配送 ----

    [TestMethod(DisplayName = "全項目を変更すると3つのイベントが配送され結果DTOを返す")]
    public async Task Update_ChangesAll_DispatchesThreeEvents()
    {
        var product = SampleProduct("万年筆", 3000, 10);
        Products.Seed(product);
        var id = product.ProductId.ToString();

        var result = await Usecase.UpdateProductAsync(UpdateDto(id, "筆ペン", 3500, 42));

        // 変更結果の DTO
        Assert.AreEqual("筆ペン", result.Name);
        Assert.AreEqual(3500, result.Price);
        Assert.AreEqual(42, result.Stock!.Quantity);

        // 配送されたイベント(発生順に3件)
        var events = Dispatcher.Dispatched;
        Assert.HasCount(3, events);
        Assert.IsInstanceOfType<ProductRenamed>(events[0]);
        Assert.IsInstanceOfType<ProductRepriced>(events[1]);
        Assert.IsInstanceOfType<StockQuantityChanged>(events[2]);
    }

    [TestMethod(DisplayName = "単価だけ変更するとProductRepricedだけ配送される")]
    public async Task Update_ChangesPriceOnly_DispatchesOnlyRepriced()
    {
        var product = SampleProduct("万年筆", 3000, 10);
        Products.Seed(product);
        var id = product.ProductId.ToString();

        // 名前・在庫は現在と同じ、単価だけ変える
        await Usecase.UpdateProductAsync(UpdateDto(id, "万年筆", 5000, 10));

        var events = Dispatcher.Dispatched;
        Assert.HasCount(1, events);
        var e = (ProductRepriced)events[0];
        Assert.AreEqual(3000, e.OldPrice.Value);
        Assert.AreEqual(5000, e.NewPrice.Value);
    }

    [TestMethod(DisplayName = "何も変えなければイベントは配送されない")]
    public async Task Update_NoChange_DispatchesNothing()
    {
        var product = SampleProduct("万年筆", 3000, 10);
        Products.Seed(product);
        var id = product.ProductId.ToString();

        await Usecase.UpdateProductAsync(UpdateDto(id, "万年筆", 3000, 10));

        Assert.IsEmpty(Dispatcher.Dispatched);
    }

    // ---- Update: 重複・ガード ----

    [TestMethod(DisplayName = "変更後の名前が別商品で使用中ならExistsException")]
    public async Task Update_ThrowsWhenNameUsedByAnother()
    {
        var target = SampleProduct("万年筆", 3000, 10);
        var other = SampleProduct("筆ペン", 3000, 10); // 別商品が「筆ペン」を使用中
        Products.Seed(target, other);

        await Assert.ThrowsExactlyAsync<ExistsException>(
            () => Usecase.UpdateProductAsync(UpdateDto(target.ProductId.ToString(), "筆ペン", 3000, 10)));

        // 例外時はイベントを配送しない
        Assert.IsEmpty(Dispatcher.Dispatched);
    }

    [TestMethod(DisplayName = "存在しない商品の変更はNotFoundException")]
    public async Task Update_ThrowsWhenProductMissing()
        => await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => Usecase.UpdateProductAsync(UpdateDto(ProductId.New().ToString(), "筆ペン", 3500, 42)));

    [TestMethod(DisplayName = "ProductDtoがnullならInvalidInputException")]
    public async Task Update_ThrowsWhenNull()
        => await Assert.ThrowsExactlyAsync<InvalidInputException>(() => Usecase.UpdateProductAsync(null!));

    [TestMethod(DisplayName = "Idが空白ならInvalidInputException")]
    public async Task Update_ThrowsWhenIdBlank()
        => await Assert.ThrowsExactlyAsync<InvalidInputException>(
            () => Usecase.UpdateProductAsync(UpdateDto("  ", "筆ペン", 3500, 42)));
}