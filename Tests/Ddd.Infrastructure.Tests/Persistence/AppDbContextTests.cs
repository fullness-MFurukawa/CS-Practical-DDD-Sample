using Ddd.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ddd.Infrastructure.Tests.Persistence;

/// <summary>
/// <see cref="Ddd.Infrastructure.Persistence.AppDbContext"/> の結合テスト。
/// </summary>
/// <remarks>
/// 実 DB への接続可否と、モデル(テーブル・列・uuid型・採番PK)がスキーマと整合していることを確認する。
/// トランザクションで囲まれ、書き込みは <see cref="DatabaseTestBase"/> によりロールバックされる。
/// </remarks>
[TestClass]
[TestCategory("Infrastructure.Persistence")]
public sealed class AppDbContextTests : DatabaseTestBase
{
    [TestMethod]
    public async Task データベースに接続できる()
    {
        Assert.IsTrue(await DbContext.Database.CanConnectAsync());
    }

    [TestMethod]
    public async Task 三つのテーブルを問合せできる_モデルとスキーマが整合()
    {
        // 例外なく問い合わせできれば、テーブル名・列マッピングがスキーマと一致している
        _ = await DbContext.ProductCategories.AsNoTracking().Take(1).ToListAsync();
        _ = await DbContext.Products.AsNoTracking().Take(1).ToListAsync();
        _ = await DbContext.ProductStocks.AsNoTracking().Take(1).ToListAsync();
    }

    [TestMethod]
    public async Task カテゴリをINSERTして読み戻せる_uuid列と採番PKのマッピング()
    {
        var uuid = Guid.NewGuid();
        var entity = new ProductCategoryEntity { CategoryUuid = uuid, Name = "検証用カテゴリ" };

        DbContext.ProductCategories.Add(entity);
        await DbContext.SaveChangesAsync();

        Assert.IsGreaterThan(0, entity.Id, "採番PKが割り当てられること");

        var reloaded = await DbContext.ProductCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryUuid == uuid);

        Assert.IsNotNull(reloaded);
        Assert.AreEqual("検証用カテゴリ", reloaded!.Name);
        Assert.AreEqual(uuid, reloaded.CategoryUuid);
    }
}