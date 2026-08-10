using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;
using Ddd.Domain.Adapters;
using Ddd.Domain.Models.Categories;

namespace Ddd.Application.Tests.Adapters;

/// <summary>
/// <see cref="Ddd.Application.Adapters.CategoryDtoAdapter"/>(<see cref="CategoryDto"/> ⇔ <see cref="Category"/>)の
/// テスト。テスト対象は DI コンテナ(<c>AddApplication</c>)から解決する。
/// </summary>
[TestClass]
[TestCategory("Application.Adapters")]
public sealed class CategoryDtoAdapterTests : ApplicationTestBase
{
    private IDomainBiAdapter<CategoryDto, Category> Adapter
        => GetRequiredService<IDomainBiAdapter<CategoryDto, Category>>();

    private const string Uuid = "0f8fad5b-d9cb-469f-a165-70867728950e";

    [TestMethod(DisplayName = "Id未指定なら新規カテゴリを生成する")]
    public void ToDomain_CreatesNew_WhenIdMissing()
    {
        var category = Adapter.ToDomain(new CategoryDto(null, "文房具"));

        Assert.AreEqual("文房具", category.Name.Value);
        Assert.AreNotEqual(Guid.Empty, category.CategoryId.Value);
    }

    [TestMethod(DisplayName = "Id指定なら復元する")]
    public void ToDomain_Restores_WhenIdGiven()
    {
        var category = Adapter.ToDomain(new CategoryDto(Uuid, "文房具"));

        Assert.AreEqual(Uuid, category.CategoryId.ToString());
        Assert.AreEqual("文房具", category.Name.Value);
    }

    [TestMethod(DisplayName = "DTOがnullなら例外")]
    public void ToDomain_ThrowsWhenNull()
        => Assert.ThrowsExactly<InvalidInputException>(() => Adapter.ToDomain(null!));

    [TestMethod(DisplayName = "カテゴリ名が空白なら例外")]
    public void ToDomain_ThrowsWhenNameBlank()
    {
        var ex = Assert.ThrowsExactly<InvalidInputException>(
            () => Adapter.ToDomain(new CategoryDto(Uuid, "  ")));
        Assert.AreEqual("商品カテゴリ名は必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "CategoryをDTOに変換する")]
    public void FromDomain_ConvertsToDto()
    {
        var category = Category.Restore(CategoryId.Parse(Uuid), CategoryName.Create("文房具"));

        var dto = Adapter.FromDomain(category);

        Assert.AreEqual(Uuid, dto.Id);
        Assert.AreEqual("文房具", dto.Name);
    }

    [TestMethod(DisplayName = "Categoryがnullなら例外")]
    public void FromDomain_ThrowsWhenNull()
        => Assert.ThrowsExactly<InvalidInputException>(() => Adapter.FromDomain(null!));
}