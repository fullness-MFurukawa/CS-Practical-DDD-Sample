using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Domain.Tests.Models.Products;

/// <summary>
/// <see cref="Product"/>(商品エンティティ・集約ルート)の単体テスト。
/// </summary>
/// <remarks>
/// 検証する仕様: CreateNew は Category と Stock を含む完全な集約を生成する。
/// 不変条件として id/name/price は null 不可。集約の完全性として Category と Stock は
/// 「両方指定」か「両方null(骨格)」のみ許可。RestoreSkeleton + Attach による段階的合成。
/// 骨格状態での CurrentStock/ChangeStock は明示的に例外(nullガード)。同一性(ProductId)による等価。
/// </remarks>
[TestClass]
[TestCategory("Domain.Models.Products")]
public sealed class ProductTests
{
    /// <summary>テスト用の有効なカテゴリを生成するヘルパ。</summary>
    private static Category ValidCategory() => Category.CreateNew(CategoryName.Create("文房具"));

    // ---- 生成(完全な集約) ----

    [TestMethod]
    public void CreateNewでカテゴリ在庫を含む商品を生成する()
    {
        var p = Product.CreateNew(
            ProductName.Create("万年筆"), ProductPrice.Create(3000),
            ValidCategory(), StockQuantity.Create(10));

        Assert.IsNotNull(p.ProductId);
        Assert.AreEqual("万年筆", p.Name.Value);
        Assert.AreEqual(3000, p.Price.Value);
        Assert.IsNotNull(p.Category);
        Assert.AreEqual(10, p.CurrentStock().Value); // 在庫が合成されている
    }

    // ---- 不変条件・集約の完全性 ----

    [TestMethod]
    public void nameがnullなら例外()
    {
        Assert.ThrowsExactly<DomainException>(() => Product.CreateNew(
            null!, ProductPrice.Create(3000), ValidCategory(), StockQuantity.Create(10)));
    }

    [TestMethod]
    public void Categoryのみ指定Stockなしの再構築は完全性エラー()
    {
        // XOR完全性チェック: 片方だけの指定は許可しない
        var ex = Assert.ThrowsExactly<DomainException>(() => Product.Restore(
            ProductId.New(), ProductName.Create("万年筆"), ProductPrice.Create(3000),
            ValidCategory(), null!));
        StringAssert.Contains(ex.Message, "両方");
    }

    // ---- 骨格再構築(RestoreSkeleton)と合成(Attach) ----

    [TestMethod]
    public void 骨格生成後にattachで合成できる()
    {
        var p = Product.RestoreSkeleton(
            ProductId.New(), ProductName.Create("万年筆"), ProductPrice.Create(3000));
        Assert.IsNull(p.Category); // 骨格ではまだ未設定

        p.AttachCategory(ValidCategory());
        p.AttachStock(Stock.CreateNew(StockQuantity.Create(5)));

        Assert.IsNotNull(p.Category);
        Assert.AreEqual(5, p.CurrentStock().Value);
    }

    [TestMethod]
    public void 在庫未設定でCurrentStockを呼ぶと明示的な例外()
    {
        var p = Product.RestoreSkeleton(
            ProductId.New(), ProductName.Create("万年筆"), ProductPrice.Create(3000));
        var ex = Assert.ThrowsExactly<DomainException>(() => p.CurrentStock());
        StringAssert.Contains(ex.Message, "在庫が未設定");
    }

    [TestMethod]
    public void AttachCategoryにnullは拒否()
    {
        var p = Product.RestoreSkeleton(
            ProductId.New(), ProductName.Create("万年筆"), ProductPrice.Create(3000));
        Assert.ThrowsExactly<DomainException>(() => p.AttachCategory(null!));
    }

    // ---- 同一性による等価 ----

    [TestMethod]
    public void IDが同じなら属性が違っても等価()
    {
        var id = ProductId.New();
        var a = Product.Restore(id, ProductName.Create("万年筆"), ProductPrice.Create(3000),
            ValidCategory(), Stock.CreateNew(StockQuantity.Create(1)));
        var b = Product.Restore(id, ProductName.Create("鉛筆"), ProductPrice.Create(100),
            ValidCategory(), Stock.CreateNew(StockQuantity.Create(2)));

        Assert.AreEqual(a, b); // 同一性(ProductId)で等価
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }
}