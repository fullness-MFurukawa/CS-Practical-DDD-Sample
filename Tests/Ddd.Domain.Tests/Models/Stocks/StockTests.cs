using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Domain.Tests.Models.Stocks;

/// <summary>
/// <see cref="Stock"/>(在庫エンティティ)の単体テスト。
/// </summary>
/// <remarks>
/// 検証する仕様: CreateNew/Restore による生成と不変条件(null不可)。
/// Increase/Decrease は負数を拒否し、加減算後も 0〜100 に収める(範囲外は例外)。
/// ChangeQuantity による差し替え(null拒否)。IsOutOfStock/IsFullCapacity の判定。
/// 同一性(StockId)による等価。
/// </remarks>
[TestClass]
[TestCategory("Domain.Models.Stocks")]
public sealed class StockTests
{
    // ---- 生成 ----

    [TestMethod]
    public void CreateNewはIDを採番し初期在庫を保持する()
    {
        var s = Stock.CreateNew(StockQuantity.Create(10));
        Assert.IsNotNull(s.StockId);
        Assert.AreEqual(10, s.Quantity.Value);
    }

    [TestMethod]
    public void 初期在庫nullは例外()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() => Stock.CreateNew(null!));
        Assert.AreEqual("在庫数は必須です。", ex.Message);
    }

    // ---- Increase / Decrease ----

    [TestMethod]
    public void 加算できる()
    {
        var s = Stock.CreateNew(StockQuantity.Create(10));
        s.Increase(5);
        Assert.AreEqual(15, s.Quantity.Value);
    }

    [TestMethod]
    public void 減算できる()
    {
        var s = Stock.CreateNew(StockQuantity.Create(10));
        s.Decrease(8);
        Assert.AreEqual(2, s.Quantity.Value);
    }

    [TestMethod]
    public void 負数の加算は拒否()
    {
        var s = Stock.CreateNew(StockQuantity.Create(10));
        Assert.ThrowsExactly<DomainException>(() => s.Increase(-1));
    }

    [TestMethod]
    public void 上限超過となる加算は範囲エラー()
    {
        var s = Stock.CreateNew(StockQuantity.Create(100));
        Assert.ThrowsExactly<DomainException>(() => s.Increase(1)); // 101はStockQuantityが拒否
    }

    [TestMethod]
    public void 下限未満となる減算は範囲エラー()
    {
        var s = Stock.CreateNew(StockQuantity.Create(0));
        Assert.ThrowsExactly<DomainException>(() => s.Decrease(1)); // -1はStockQuantityが拒否
    }

    // ---- ChangeQuantity / 判定 ----

    [TestMethod]
    public void ChangeQuantityで差し替えできる()
    {
        var s = Stock.CreateNew(StockQuantity.Create(10));
        s.ChangeQuantity(StockQuantity.Create(30));
        Assert.AreEqual(30, s.Quantity.Value);
    }

    [TestMethod]
    public void ChangeQuantityにnullは拒否()
    {
        var s = Stock.CreateNew(StockQuantity.Create(10));
        Assert.ThrowsExactly<DomainException>(() => s.ChangeQuantity(null!));
    }

    [TestMethod]
    public void 在庫0は在庫切れ100は満杯()
    {
        Assert.IsTrue(Stock.CreateNew(StockQuantity.Create(0)).IsOutOfStock);
        Assert.IsTrue(Stock.CreateNew(StockQuantity.Create(100)).IsFullCapacity);
    }

    // ---- 同一性による等価 ----

    [TestMethod]
    public void IDが同じなら在庫数が違っても等価()
    {
        var id = StockId.New();
        var a = Stock.Restore(id, StockQuantity.Create(10));
        var b = Stock.Restore(id, StockQuantity.Create(20));
        Assert.AreEqual(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }
}