using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Categories;

namespace Ddd.Domain.Tests.Models.Categories;

/// <summary>
/// <see cref="Category"/>(商品カテゴリエンティティ)の単体テスト。
/// </summary>
/// <remarks>
/// 検証する仕様: CreateNew は新しいIDを採番し指定の名称で生成する。Restore は指定IDで再構築する。
/// 不変条件として id/name は null 不可。Rename で名称を変更でき null は拒否する。
/// 同一性(CategoryId)による等価: IDが同じなら名称が違っても等価。
/// </remarks>
[TestClass]
[TestCategory("Domain.Models.Categories")]
public sealed class CategoryTests
{
    // ---- 生成 ----

    [TestMethod]
    public void CreateNewはIDを採番し名称を保持する()
    {
        var c = Category.CreateNew(CategoryName.Create("文房具"));
        Assert.IsNotNull(c.CategoryId);
        Assert.AreEqual("文房具", c.Name.Value);
    }

    [TestMethod]
    public void Restoreは指定IDで再構築する()
    {
        var id = CategoryId.New();
        var c = Category.Restore(id, CategoryName.Create("雑貨"));
        Assert.AreEqual(id, c.CategoryId);
    }

    // ---- 不変条件 ----

    [TestMethod]
    public void idがnullなら例外()
    {
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Category.Restore(null!, CategoryName.Create("文房具")));
        Assert.AreEqual("カテゴリIDは必須です。", ex.Message);
    }

    [TestMethod]
    public void nameがnullなら例外()
    {
        Assert.ThrowsExactly<DomainException>(
            () => Category.Restore(CategoryId.New(), null!));
    }

    // ---- 振る舞い ----

    [TestMethod]
    public void renameで名称を変更できる()
    {
        var c = Category.CreateNew(CategoryName.Create("文房具"));
        c.Rename(CategoryName.Create("事務用品"));
        Assert.AreEqual("事務用品", c.Name.Value);
    }

    [TestMethod]
    public void renameにnullは拒否()
    {
        var c = Category.CreateNew(CategoryName.Create("文房具"));
        Assert.ThrowsExactly<DomainException>(() => c.Rename(null!));
    }

    // ---- 同一性による等価 ----

    [TestMethod]
    public void IDが同じなら名称が違っても等価()
    {
        var id = CategoryId.New();
        var a = Category.Restore(id, CategoryName.Create("文房具"));
        var b = Category.Restore(id, CategoryName.Create("雑貨"));
        Assert.AreEqual(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void IDが異なれば非等価()
    {
        var a = Category.CreateNew(CategoryName.Create("文房具"));
        var b = Category.CreateNew(CategoryName.Create("文房具"));
        Assert.AreNotEqual(a, b);
    }
}