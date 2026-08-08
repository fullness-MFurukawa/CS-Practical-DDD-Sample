using Ddd.Domain.Models.Categories;
using Ddd.Infrastructure.Tests.Persistence;

namespace Ddd.Infrastructure.Tests.Categories;

/// <summary>
/// <see cref="Ddd.Infrastructure.Categories.CategoryRepository"/> の結合テスト
/// (実 PostgreSQL / サンプルデータ前提)。テスト対象は DI コンテナから解決する。
/// </summary>
/// <remarks>
/// サンプルのカテゴリ「文房具」が投入済みであることを前提とする。書き込みは行わないが、
/// <see cref="DatabaseTestBase"/> のトランザクションで囲まれる。
/// </remarks>
[TestClass]
[TestCategory("Infrastructure.Categories")]
public sealed class CategoryRepositoryTests : DatabaseTestBase
{
  
    private ICategoryRepository Repository => GetRequiredService<ICategoryRepository>();

    [TestMethod(DisplayName = "カテゴリが存在すればFindByIdで取得できる")]
    public async Task FindById_ReturnsCategoryWhenExists()
    {
        // category_uuid はランダム生成なので、FindAll から「文房具」を取り、その実在IDで引き直す
        var all = await Repository.FindAllAsync();
        var stationery = all.FirstOrDefault(c => c.Name.Value == "文房具");
        Assert.IsNotNull(stationery, "サンプルの『文房具』カテゴリが見つからない");

        var found = await Repository.FindByIdAsync(stationery!.CategoryId);

        Assert.IsNotNull(found);
        Assert.AreEqual("文房具", found!.Name.Value);
        Assert.AreEqual(stationery.CategoryId.Value, found.CategoryId.Value);
    }

    [TestMethod(DisplayName = "存在しないカテゴリIdならnullを返す")]
    public async Task FindById_ReturnsNullWhenNotFound()
    {
        var found = await Repository.FindByIdAsync(CategoryId.New());

        Assert.IsNull(found);
    }

    [TestMethod(DisplayName = "FindAllでサンプルのカテゴリ文房具が取得できる")]
    public async Task FindAll_ContainsSampleCategory()
    {
        var all = await Repository.FindAllAsync();

        Assert.IsTrue(all.Any(c => c.Name.Value == "文房具"));
    }
}