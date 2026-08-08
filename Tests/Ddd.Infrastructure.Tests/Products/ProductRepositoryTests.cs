using Ddd.Infrastructure.Tests.Persistence;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Infrastructure.Tests.Products;

/// <summary>
/// <see cref="Ddd.Infrastructure.Products.ProductRepository"/> の結合テスト
/// (実 PostgreSQL / サンプルデータ前提)。テスト対象は DI コンテナから解決する。
/// </summary>
/// <remarks>
/// <para>
/// サンプルデータに商品「油性ボールペン」(カテゴリ「文房具」/ 単価 120 / 在庫 80)が投入済みであることを前提とする。
/// </para>
/// <para>
/// 登録・更新を伴うテストも <see cref="DatabaseTestBase"/> のトランザクションで囲まれ、各テスト終了時に
/// ロールバックされる(<c>ExecuteUpdateAsync</c> も同一接続の開始済みトランザクションに参加する)。
/// Repository は基盤と同じスコープから解決されるため、<c>DbContext</c> と同一インスタンス・同一トランザクションで動く。
/// </para>
/// </remarks>
[TestClass]
[TestCategory("Infrastructure.Products")]
public sealed class ProductRepositoryTests : DatabaseTestBase
{
    /// <summary>サンプルデータに存在する商品(文房具 / 単価 120 / 在庫 80)。</summary>
    private const string ExistingName = "油性ボールペン";

    /// <summary>サンプルデータに存在しない商品名。</summary>
    private const string MissingName = "存在しない商品ZZZ";

    private IProductRepository Repository => GetRequiredService<IProductRepository>();

    /// <summary>既存商品から実在するカテゴリを借りる(外部キーが解決できることを保証する)。</summary>
    private async Task<Category> BorrowExistingCategoryAsync()
    {
        var existing = await Repository.FindByNameAsync(ProductName.Create(ExistingName));
        Assert.IsNotNull(existing, "前提のサンプル商品が見つからない");
        return existing!.Category!;
    }

    // ---- ExistsByName ----

    [TestMethod(DisplayName = "存在する商品名ならtrue")]
    public async Task ExistsByName_ReturnsTrueForExisting()
    {
        Assert.IsTrue(await Repository.ExistsByNameAsync(ProductName.Create(ExistingName)));
    }

    [TestMethod(DisplayName = "存在しない商品名ならfalse")]
    public async Task ExistsByName_ReturnsFalseForMissing()
    {
        Assert.IsFalse(await Repository.ExistsByNameAsync(ProductName.Create(MissingName)));
    }

    // ---- FindByName ----

    [TestMethod(DisplayName = "存在する商品を取得できる_カテゴリと在庫も合成される")]
    public async Task FindByName_ReturnsProductWithCategoryAndStock()
    {
        var found = await Repository.FindByNameAsync(ProductName.Create(ExistingName));

        Assert.IsNotNull(found, "サンプルデータの商品が取得できること");
        Assert.AreEqual(ExistingName, found!.Name.Value);
        Assert.AreEqual(120, found.Price.Value);
        // JOIN でカテゴリ・在庫まで合成されていること
        Assert.AreEqual("文房具", found.Category!.Name.Value);
        Assert.AreEqual(80, found.Stock!.Quantity.Value);
    }

    [TestMethod(DisplayName = "存在しない商品名ならnull")]
    public async Task FindByName_ReturnsNullForMissing()
    {
        Assert.IsNull(await Repository.FindByNameAsync(ProductName.Create(MissingName)));
    }

    // ---- Create → FindById(ラウンドトリップ) ----

    [TestMethod(DisplayName = "新規商品を登録しIdで取得できる")]
    public async Task Create_ThenFindById_RoundTrips()
    {
        var category = await BorrowExistingCategoryAsync();

        // 新規商品(Id はドメイン側で採番＝CreateNew 時点で確定する)
        var newProduct = Product.CreateNew(
            ProductName.Create("結合テスト商品"),
            ProductPrice.Create(500),
            category,
            StockQuantity.Create(15));
        var newId = newProduct.ProductId;

        await Repository.CreateAsync(newProduct);

        var found = await Repository.FindByIdAsync(newId);
        Assert.IsNotNull(found, "登録した商品が Id で取得できること");
        Assert.AreEqual("結合テスト商品", found!.Name.Value);
        Assert.AreEqual(500, found.Price.Value);
        Assert.AreEqual(category.CategoryId.Value, found.Category!.CategoryId.Value);
        Assert.AreEqual(15, found.Stock!.Quantity.Value);

        Assert.IsTrue(await Repository.ExistsByNameAsync(ProductName.Create("結合テスト商品")));
    }

    [TestMethod(DisplayName = "存在しないIdならnull")]
    public async Task FindById_ReturnsNullForMissing()
    {
        Assert.IsNull(await Repository.FindByIdAsync(ProductId.New()));
    }

    // ---- Update(名称・単価・在庫数の変更) ----

    [TestMethod(DisplayName = "既存商品の名称単価在庫数を変更しIdで取得して反映を確認できる")]
    public async Task Update_ChangesNamePriceAndStock()
    {
        var category = await BorrowExistingCategoryAsync();

        // 更新対象の商品を新規登録する
        var target = Product.CreateNew(
            ProductName.Create("変更前商品"),
            ProductPrice.Create(300),
            category,
            StockQuantity.Create(10));
        var id = target.ProductId;
        await Repository.CreateAsync(target);

        // 登録済みの集約を取得し、名称・単価・在庫数を変更する
        var loaded = await Repository.FindByIdAsync(id);
        Assert.IsNotNull(loaded, "登録した商品が取得できない");
        // 在庫行の同一性(stock_uuid)が保持されることを後で確認するため控えておく
        var stockUuidBefore = loaded!.Stock!.StockId.Value;

        loaded.Rename(ProductName.Create("変更後商品"));
        loaded.Reprice(ProductPrice.Create(750));
        loaded.ChangeStock(StockQuantity.Create(42));

        await Repository.UpdateAsync(loaded);

        // Id で取得して変更が反映されていることを検証
        var updated = await Repository.FindByIdAsync(id);
        Assert.IsNotNull(updated, "更新後の商品が取得できない");
        Assert.AreEqual("変更後商品", updated!.Name.Value);
        Assert.AreEqual(750, updated.Price.Value);
        Assert.AreEqual(42, updated.Stock!.Quantity.Value);

        // カテゴリは変更対象外なので不変であること
        Assert.AreEqual(category.CategoryId.Value, updated.Category!.CategoryId.Value);
        // 在庫行の同一性(stock_uuid)が保持されていること(＝再INSERTではなく更新である担保)
        Assert.AreEqual(stockUuidBefore, updated.Stock.StockId.Value);

        // 旧名では存在しなくなり、新名で存在すること
        Assert.IsFalse(await Repository.ExistsByNameAsync(ProductName.Create("変更前商品")));
        Assert.IsTrue(await Repository.ExistsByNameAsync(ProductName.Create("変更後商品")));
    }
}