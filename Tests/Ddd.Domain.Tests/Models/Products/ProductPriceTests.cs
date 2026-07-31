using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Products;

namespace Ddd.Domain.Tests.Models.Products;

/// <summary>
/// <see cref="ProductPrice"/>(商品単価の値オブジェクト)の単体テスト。
/// </summary>
/// <remarks>
/// 検証するドメインルール: 有効範囲は 50〜10000(境界値を含む)、値による等価。
/// <para>
/// 補足: 内部型が <see cref="int"/> のため「null は必須エラー」は型レベルで保証され、
/// Java版の null 検証テストに相当するものは不要(コンパイル時に排除される)。
/// </para>
/// </remarks>
[TestClass]
[TestCategory("Domain.Models.Products")]
public sealed class ProductPriceTests
{
    // ---- 正常系・境界値 ----

    [TestMethod]
    public void 下限50で生成できる()
    {
        Assert.AreEqual(50, ProductPrice.Create(50).Value);
    }

    [TestMethod]
    public void 上限10000で生成できる()
    {
        Assert.AreEqual(10000, ProductPrice.Create(10000).Value);
    }

    // ---- 異常系 ----

    [TestMethod]
    public void 下限未満49は範囲エラー()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() => ProductPrice.Create(49));
        StringAssert.Contains(ex.Message, "50 以上 10000 以下");
    }

    [TestMethod]
    public void 上限超過10001は範囲エラー()
    {
        Assert.ThrowsExactly<DomainException>(() => ProductPrice.Create(10001));
    }

    // ---- 等価性 ----

    [TestMethod]
    public void 同じ値は等価でhashCodeも一致()
    {
        Assert.AreEqual(ProductPrice.Create(500), ProductPrice.Create(500));
        Assert.AreEqual(ProductPrice.Create(500).GetHashCode(), ProductPrice.Create(500).GetHashCode());
    }
}