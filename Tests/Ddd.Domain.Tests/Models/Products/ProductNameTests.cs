using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Products;

namespace Ddd.Domain.Tests.Models.Products;

/// <summary>
/// <see cref="ProductName"/>(商品名の値オブジェクト)の単体テスト。
/// </summary>
/// <remarks>
/// 検証するドメインルール: 必須(null/空/空白のみ不可)、最大30文字(境界値を含む)、
/// 前後空白はトリムして保持、値による等価。
/// </remarks>
[TestClass]
[TestCategory("Domain.Models.Products")]
public sealed class ProductNameTests
{
    // ---- 正常系・境界値 ----

    [TestMethod]
    public void 通常の名称で生成できる()
    {
        Assert.AreEqual("万年筆", ProductName.Create("万年筆").Value);
    }

    [TestMethod]
    public void 前後の空白はトリムされる()
    {
        Assert.AreEqual("万年筆", ProductName.Create("  万年筆  ").Value);
    }

    [TestMethod]
    public void 最大長30文字ちょうどは許可()
    {
        var name = new string('a', 30);
        Assert.AreEqual(name, ProductName.Create(name).Value);
    }

    // ---- 異常系 ----

    [TestMethod]
    public void nullは必須エラー()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() => ProductName.Create(null!));
        Assert.AreEqual("商品名は必須です。", ex.Message);
    }

    [TestMethod]
    public void 空白のみは空エラー()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() => ProductName.Create("   "));
        Assert.AreEqual("商品名は空にできません。", ex.Message);
    }

    [TestMethod]
    public void 三十一文字は最大長エラー()
    {
        var name = new string('a', 31);
        var ex = Assert.ThrowsExactly<DomainException>(() => ProductName.Create(name));
        StringAssert.Contains(ex.Message, "30文字以内");
    }

    // ---- 等価性 ----

    [TestMethod]
    public void 同じ値は等価()
    {
        Assert.AreEqual(ProductName.Create("ノート"), ProductName.Create("ノート"));
        Assert.AreEqual(ProductName.Create("ノート").GetHashCode(), ProductName.Create("ノート").GetHashCode());
    }
}