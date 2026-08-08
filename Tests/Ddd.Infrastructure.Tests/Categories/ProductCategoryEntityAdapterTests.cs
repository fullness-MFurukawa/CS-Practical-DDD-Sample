using Ddd.Domain.Adapters;
using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Categories;
using Ddd.Infrastructure.Entities;

namespace Ddd.Infrastructure.Tests.Categories;

/// <summary>
/// <see cref="Ddd.Infrastructure.Categories.ProductCategoryEntityAdapter"/>(Entity → Category)の
/// 単体テスト(DB不要)。テスト対象は DI コンテナから解決する。
/// </summary>
/// <remarks>
/// 変換は問合せ方向(ToDomain)のみ。<c>category_uuid</c> は <see cref="Guid"/> 型のため、
/// Java版にあった「UUID形式でない文字列」のテストは型で排除され不要(<see cref="Guid.Empty"/> を不正として検証)。
/// </remarks>
[TestClass]
[TestCategory("Infrastructure.Categories")]
public sealed class ProductCategoryEntityAdapterTests : InfrastructureTestBase
{
    private IToDomainAdapter<ProductCategoryEntity, Category> Adapter
        => GetRequiredService<IToDomainAdapter<ProductCategoryEntity, Category>>();

    private static ProductCategoryEntity Entity(Guid categoryUuid, string name)
        => new() { CategoryUuid = categoryUuid, Name = name };

    [TestMethod(DisplayName = "有効なEntityをCategoryに変換できる")]
    public void ToDomain_ConvertsValidEntity()
    {
        var uuid = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var category = Adapter.ToDomain(Entity(uuid, "文房具"));

        Assert.AreEqual(uuid, category.CategoryId.Value);
        Assert.AreEqual("文房具", category.Name.Value);
    }

    [TestMethod(DisplayName = "Entityがnullなら例外")]
    public void ToDomain_ThrowsWhenEntityIsNull()
    {
        Assert.ThrowsExactly<DomainException>(() => Adapter.ToDomain(null!));
    }

    [TestMethod(DisplayName =  "category_uuidが空なら例外")]
    public void ToDomain_ThrowsWhenUuidIsEmpty()
    {
        Assert.ThrowsExactly<DomainException>(() => Adapter.ToDomain(Entity(Guid.Empty, "文房具")));
    }

    [TestMethod(DisplayName =  "nameが空白なら例外")]
    public void ToDomain_ThrowsWhenNameIsBlank()
    {
        var uuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Assert.ThrowsExactly<DomainException>(() => Adapter.ToDomain(Entity(uuid, "   ")));
    }
}