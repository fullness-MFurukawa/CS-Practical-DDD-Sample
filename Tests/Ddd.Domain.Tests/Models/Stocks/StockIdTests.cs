using System.Text.RegularExpressions;
using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Domain.Tests.Models.Stocks;

/// <summary>
/// <see cref="StockId"/>(在庫識別子の値オブジェクト)の単体テスト。
/// CategoryId / ProductId と同型(New/Parse、canonical正規化、値等価)。
/// </summary>
[TestClass]
[TestCategory("Domain.Models.Stocks")]
public sealed class StockIdTests
{
    private static readonly Regex Canonical =
        new("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$");

    // ---- New(新規採番) ----

    [TestMethod]
    public void New_canonicalなUUIDを一意に発行する()
    {
        var v = StockId.New().ToString();
        Assert.IsTrue(Canonical.IsMatch(v));
        Assert.AreNotEqual(StockId.New(), StockId.New());
    }

    // ---- Parse(既存値からの復元) ----

    [TestMethod]
    public void Parse_大文字入力は小文字へ正規化される()
    {
        const string upper = "0F8FAD5B-D9CB-469F-A165-70867728950E";
        Assert.AreEqual(upper.ToLowerInvariant(), StockId.Parse(upper).ToString());
    }

    [TestMethod]
    public void Parse_nullは必須エラー()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() => StockId.Parse(null!));
        Assert.AreEqual("在庫IDは必須です。", ex.Message);
    }

    [TestMethod]
    public void Parse_UUID形式でない値はエラー()
    {
        Assert.ThrowsExactly<DomainException>(() => StockId.Parse("xxxx"));
    }

    // ---- 等価性 ----

    [TestMethod]
    public void 大小文字違いでも正規化後に等価()
    {
        var a = StockId.Parse("0F8FAD5B-D9CB-469F-A165-70867728950E");
        var b = StockId.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");
        Assert.AreEqual(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }
}