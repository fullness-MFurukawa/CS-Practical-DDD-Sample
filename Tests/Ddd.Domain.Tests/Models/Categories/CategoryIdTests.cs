using System.Text.RegularExpressions;
using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Categories;

namespace Ddd.Domain.Tests.Models.Categories;

/// <summary>
/// <see cref="CategoryId"/>(カテゴリ識別子の値オブジェクト)の単体テスト。
/// </summary>
/// <remarks>
/// 検証するドメインルール:
/// New は canonical(小文字・ハイフン付き36文字)なUUIDを一意に発行する。
/// Parse は必須・UUID形式を検証し、大文字入力は小文字へ正規化する。
/// 正規化後の値が同じなら等価(大小文字の違いを吸収する)。
/// </remarks>
[TestClass]
[TestCategory("Domain.Models.Categories")]
public sealed class CategoryIdTests
{
    private static readonly Regex Canonical =
        new("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$");

    // ---- New(新規採番) ----

    [TestMethod]
    public void New_canonical小文字ハイフン付き36文字で生成される()
    {
        var v = CategoryId.New().ToString();
        Assert.AreEqual(36, v.Length);
        Assert.AreEqual(v.ToLowerInvariant(), v);
        Assert.IsTrue(Canonical.IsMatch(v));
    }

    [TestMethod]
    public void New_生成の都度異なるIDになる()
    {
        Assert.AreNotEqual(CategoryId.New(), CategoryId.New());
    }

    // ---- Parse(既存値からの復元) ----

    [TestMethod]
    public void Parse_大文字入力は小文字に正規化される()
    {
        const string upper = "0F8FAD5B-D9CB-469F-A165-70867728950E";
        Assert.AreEqual(upper.ToLowerInvariant(), CategoryId.Parse(upper).ToString());
    }

    [TestMethod]
    public void Parse_前後空白はトリムされる()
    {
        const string raw = "  0f8fad5b-d9cb-469f-a165-70867728950e  ";
        Assert.AreEqual("0f8fad5b-d9cb-469f-a165-70867728950e", CategoryId.Parse(raw).ToString());
    }

    [TestMethod]
    public void Parse_nullは必須エラー()
    {
        var ex = Assert.ThrowsExactly<DomainException>(() => CategoryId.Parse(null!));
        Assert.AreEqual("カテゴリIDは必須です。", ex.Message);
    }

    [TestMethod]
    public void Parse_空白のみは必須エラー()
    {
        Assert.ThrowsExactly<DomainException>(() => CategoryId.Parse("   "));
    }

    [TestMethod]
    public void Parse_UUID形式でない文字列はエラー()
    {
        Assert.ThrowsExactly<DomainException>(() => CategoryId.Parse("not-a-uuid"));
    }

    // ---- 等価性 ----

    [TestMethod]
    public void 大小文字違いでも正規化後に等価()
    {
        var a = CategoryId.Parse("0F8FAD5B-D9CB-469F-A165-70867728950E");
        var b = CategoryId.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");
        Assert.AreEqual(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }
}