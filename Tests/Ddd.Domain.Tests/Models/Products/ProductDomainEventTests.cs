using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Products.Events;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Domain.Tests.Models.Products;

/// <summary>
/// <see cref="Product"/> のドメインイベント発行の単体テスト。
/// </summary>
/// <remarks>
/// 検証する仕様: 変更メソッド(Rename/Reprice/ChangeStock)は「実際に値が変わったときだけ」対応イベントを
/// 積み、同じ値への変更やCreateNewでは積まない。複数変更は発生順に蓄積され、PullDomainEvents で
/// 取り出すとクリアされる。
/// </remarks>
[TestClass]
[TestCategory("Domain.Models.Products")]
public sealed class ProductDomainEventTests
{
    private static Product SampleProduct()
        => Product.CreateNew(
            ProductName.Create("万年筆"), ProductPrice.Create(3000),
            Category.CreateNew(CategoryName.Create("文房具")), StockQuantity.Create(10));

    // ---- 生成時は発行しない ----

    [TestMethod(DisplayName = "CreateNewではイベントを発行しない")]
    public void CreateNew_RaisesNoEvent()
    {
        var product = SampleProduct();
        Assert.IsEmpty(product.PullDomainEvents());
    }

    // ---- Rename ----

    [TestMethod(DisplayName = "Renameで名前が変わるとProductRenamedを発行する")]
    public void Rename_RaisesProductRenamed_WhenChanged()
    {
        var product = SampleProduct();

        product.Rename(ProductName.Create("筆ペン"));

        var events = product.PullDomainEvents();
        Assert.HasCount(1, events);
        var e = (ProductRenamed)events[0];
        Assert.AreEqual(product.ProductId, e.ProductId);
        Assert.AreEqual("万年筆", e.OldName.Value);
        Assert.AreEqual("筆ペン", e.NewName.Value);
    }

    [TestMethod(DisplayName = "Renameで同じ名前ならイベントを発行しない")]
    public void Rename_RaisesNothing_WhenUnchanged()
    {
        var product = SampleProduct();

        product.Rename(ProductName.Create("万年筆")); // 現在と同じ

        Assert.IsEmpty(product.PullDomainEvents());
    }

    // ---- Reprice ----

    [TestMethod(DisplayName = "Repriceで単価が変わるとProductRepricedを発行する")]
    public void Reprice_RaisesProductRepriced_WhenChanged()
    {
        var product = SampleProduct();

        product.Reprice(ProductPrice.Create(3500));

        var events = product.PullDomainEvents();
        Assert.HasCount(1, events);
        var e = (ProductRepriced)events[0];
        Assert.AreEqual(3000, e.OldPrice.Value);
        Assert.AreEqual(3500, e.NewPrice.Value);
    }

    [TestMethod(DisplayName = "Repriceで同じ単価ならイベントを発行しない")]
    public void Reprice_RaisesNothing_WhenUnchanged()
    {
        var product = SampleProduct();

        product.Reprice(ProductPrice.Create(3000)); // 現在と同じ

        Assert.IsEmpty(product.PullDomainEvents());
    }

    // ---- ChangeStock ----

    [TestMethod(DisplayName = "ChangeStockで数量が変わるとStockQuantityChangedを発行する")]
    public void ChangeStock_RaisesStockQuantityChanged_WhenChanged()
    {
        var product = SampleProduct();
        var stockId = product.Stock!.StockId;

        product.ChangeStock(StockQuantity.Create(42));

        var events = product.PullDomainEvents();
        Assert.HasCount(1, events);
        var e = (StockQuantityChanged)events[0];
        Assert.AreEqual(product.ProductId, e.ProductId);
        Assert.AreEqual(stockId, e.StockId);       // 在庫の同一性(StockId)は保持される
        Assert.AreEqual(10, e.OldQuantity.Value);
        Assert.AreEqual(42, e.NewQuantity.Value);
    }

    [TestMethod(DisplayName = "ChangeStockで同じ数量ならイベントを発行しない")]
    public void ChangeStock_RaisesNothing_WhenUnchanged()
    {
        var product = SampleProduct();

        product.ChangeStock(StockQuantity.Create(10)); // 現在と同じ

        Assert.IsEmpty(product.PullDomainEvents());
    }

    // ---- 複数変更・取り出し ----

    [TestMethod(DisplayName = "複数の変更は発生順に蓄積される")]
    public void MultipleChanges_AccumulateInOrder()
    {
        var product = SampleProduct();

        product.Rename(ProductName.Create("筆ペン"));
        product.Reprice(ProductPrice.Create(3500));
        product.ChangeStock(StockQuantity.Create(42));

        var events = product.PullDomainEvents();
        Assert.HasCount(3, events);
        Assert.IsInstanceOfType<ProductRenamed>(events[0]);
        Assert.IsInstanceOfType<ProductRepriced>(events[1]);
        Assert.IsInstanceOfType<StockQuantityChanged>(events[2]);
    }

    [TestMethod(DisplayName = "PullDomainEventsは取り出し後にクリアされる")]
    public void PullDomainEvents_ClearsAfterPull()
    {
        var product = SampleProduct();
        product.Rename(ProductName.Create("筆ペン"));

        Assert.HasCount(1, product.PullDomainEvents()); // 1回目は取得できる
        Assert.IsEmpty(product.PullDomainEvents()); // 2回目はクリア済み
    }
}