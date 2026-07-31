using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Domain.Tests.Models.Stocks;

/// <summary>
/// <see cref="StockQuantity"/>(在庫数の値オブジェクト)の単体テスト。
/// </summary>
/// <remarks>
/// 検証するドメインルール: 有効範囲は 0〜100(境界値を含む)、値による等価。
/// テスト自体を「在庫数の仕様書」として読めることを意図している。
/// <para>
/// 補足: 内部型が <see cref="int"/> のため「null は必須エラー」は型レベルで保証され、
/// Java版の null 検証テストに相当するものは不要。
/// </para>
/// </remarks>
[TestClass]
[TestCategory("Domain.Models.Stocks")]
public sealed class StockQuantityTests
{
    // ---- 正常系・境界値 ----

    [TestMethod]
    public void 最小値0で生成できる()
    {
        Assert.AreEqual(0, StockQuantity.Create(0).Value);
    }

    [TestMethod]
    public void 最大値100で生成できる()
    {
        Assert.AreEqual(100, StockQuantity.Create(100).Value);
    }

    [TestMethod]
    public void 中間値で生成できる()
    {
        Assert.AreEqual(50, StockQuantity.Create(50).Value);
    }

    // ---- 異常系 ----

    [TestMethod]
    public void 下限未満マイナス1は範囲エラー()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() => StockQuantity.Create(-1));
        StringAssert.Contains(ex.Message, "0 以上 100 以下");
    }

    [TestMethod]
    public void 上限超過101は範囲エラー()
    {
        Assert.ThrowsExactly<DomainException>(() => StockQuantity.Create(101));
    }

    // ---- 等価性 ----

    [TestMethod]
    public void 同じ値は等価でhashCodeも一致()
    {
        var a = StockQuantity.Create(10);
        var b = StockQuantity.Create(10);
        Assert.AreEqual(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void 異なる値は非等価()
    {
        Assert.AreNotEqual(StockQuantity.Create(10), StockQuantity.Create(20));
    }
}