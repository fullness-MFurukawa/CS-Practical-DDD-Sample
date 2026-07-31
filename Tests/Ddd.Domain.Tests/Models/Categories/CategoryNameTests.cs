using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Categories;

namespace Ddd.Domain.Tests.Models.Categories;

/// <summary>
/// <see cref="CategoryName"/>(カテゴリ名の値オブジェクト)の単体テスト。
/// </summary>
/// <remarks>
/// 検証するドメインルール: 必須(null/空/空白のみ不可)、最大20文字(境界値を含む)、
/// 前後空白はトリムして保持、値による等価。
/// </remarks>
[TestClass]
[TestCategory("Domain.Models.Categories")]
public sealed class CategoryNameTests
{
    // ---- 正常系・境界値 ----

    [TestMethod]
    public void 通常の名称で生成できる()
    {
        Assert.AreEqual("文房具", CategoryName.Create("文房具").Value);
    }

    [TestMethod]
    public void 前後の空白はトリムされる()
    {
        Assert.AreEqual("文房具", CategoryName.Create("  文房具  ").Value);
    }

    [TestMethod]
    public void 最大長20文字ちょうどは許可()
    {
        var name = new string('a', 20);
        Assert.AreEqual(name, CategoryName.Create(name).Value);
    }

    // ---- 異常系 ----

    [TestMethod]
    public void nullは必須エラー()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() => CategoryName.Create(null!));
        Assert.AreEqual("カテゴリ名は必須です。", ex.Message);
    }

    [TestMethod]
    public void 空白のみは空エラー()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() => CategoryName.Create("   "));
        Assert.AreEqual("カテゴリ名は空にできません。", ex.Message);
    }

    [TestMethod]
    public void 二十一文字は最大長エラー()
    {
        var name = new string('a', 21);
        var ex = Assert.ThrowsExactly<DomainException>(() => CategoryName.Create(name));
        StringAssert.Contains(ex.Message, "20文字以内");
    }

    // ---- 等価性 ----

    [TestMethod]
    public void 同じ値は等価()
    {
        Assert.AreEqual(CategoryName.Create("雑貨"), CategoryName.Create("雑貨"));
        Assert.AreEqual(CategoryName.Create("雑貨").GetHashCode(), CategoryName.Create("雑貨").GetHashCode());
    }
}