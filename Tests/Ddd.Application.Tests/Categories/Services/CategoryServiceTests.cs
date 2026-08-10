using Ddd.Application.Categories.Services;
using Ddd.Application.Exceptions;
using Ddd.Application.Tests.Fakes;
using Ddd.Domain.Models.Categories;

namespace Ddd.Application.Tests.Categories.Services;

/// <summary>
/// <see cref="CategoryService"/> のテスト。Fake リポジトリをシードし、テスト対象は DI から解決する。
/// </summary>
[TestClass]
[TestCategory("Application.Services")]
public sealed class CategoryServiceTests : ApplicationTestBase
{
    private ICategoryService Service => GetRequiredService<ICategoryService>();
    private FakeCategoryRepository Categories => GetRequiredService<FakeCategoryRepository>();

    [TestMethod(DisplayName = "全カテゴリを取得する")]
    public async Task GetCategories_ReturnsAll()
    {
        Categories.Seed(
            Category.CreateNew(CategoryName.Create("文房具")),
            Category.CreateNew(CategoryName.Create("雑貨")));

        var result = await Service.GetCategoriesAsync();

        Assert.HasCount(2, result);
    }

    [TestMethod(DisplayName = "Idでカテゴリを取得する")]
    public async Task GetCategoryById_ReturnsMatching()
    {
        var category = Category.CreateNew(CategoryName.Create("文房具"));
        Categories.Seed(category);

        var result = await Service.GetCategoryByIdAsync(category.CategoryId);

        Assert.AreEqual(category.CategoryId, result.CategoryId);
        Assert.AreEqual("文房具", result.Name.Value);
    }

    [TestMethod(DisplayName = "存在しないIdはNotFoundException")]
    public async Task GetCategoryById_ThrowsWhenMissing()
    {
        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => Service.GetCategoryByIdAsync(CategoryId.New()));
    }
}