using Ddd.Domain.Models.Categories;
using Ddd.Infrastructure.Categories;
using Ddd.Infrastructure.Tests.Persistence;

namespace Ddd.Infrastructure.Tests.Categories;

/// <summary>
/// <see cref="CategoryRepository"/> の結合テスト(実 PostgreSQL / サンプルデータ前提)。
/// </summary>
/// <remarks>
/// サンプルのカテゴリ「文房具」が投入済みであることを前提とする。書き込みは行わないが、
/// <see cref="DatabaseTestBase"/> のトランザクションで囲まれる。
/// </remarks>
[TestClass]
[TestCategory("Infrastructure.Categories")]
public sealed class CategoryRepositoryTests : DatabaseTestBase
{
    private CategoryRepository CreateRepository()
        => new(DbContext, new ProductCategoryEntityMapper());

    [TestMethod]
    public async Task FindById_カテゴリが存在すれば取得できる()
    {
        var repository = CreateRepository();

        // category_uuid はランダム生成なので、FindAll から「文房具」を取り、その実在IDで引き直す
        var all = await repository.FindAllAsync();
        var stationery = all.FirstOrDefault(c => c.Name.Value == "文房具");
        Assert.IsNotNull(stationery, "サンプルの『文房具』カテゴリが見つからない");

        var found = await repository.FindByIdAsync(stationery!.CategoryId);

        Assert.IsNotNull(found);
        Assert.AreEqual("文房具", found!.Name.Value);
        Assert.AreEqual(stationery.CategoryId.Value, found.CategoryId.Value);
    }

    [TestMethod]
    public async Task FindById_存在しないカテゴリIdならnullを返す()
    {
        var repository = CreateRepository();

        var found = await repository.FindByIdAsync(CategoryId.New());

        Assert.IsNull(found);
    }

    [TestMethod]
    public async Task FindAll_サンプルのカテゴリ文房具が取得できる()
    {
        var repository = CreateRepository();

        var all = await repository.FindAllAsync();

        Assert.IsTrue(all.Any(c => c.Name.Value == "文房具"));
    }
}