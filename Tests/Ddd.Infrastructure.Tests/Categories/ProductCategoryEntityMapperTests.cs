using Ddd.Domain.Exceptions;
using Ddd.Infrastructure.Categories;
using Ddd.Infrastructure.Entities;

namespace Ddd.Infrastructure.Tests.Categories;

/// <summary>
/// <see cref="ProductCategoryEntityMapper"/>(Entity → Category)の単体テスト(DB不要)。
/// </summary>
/// <remarks>
/// 変換は問合せ方向(ToDomain)のみ。<c>category_uuid</c> は <see cref="Guid"/> 型のため、
/// Java版にあった「UUID形式でない文字列」のテストは型で排除され不要(<see cref="Guid.Empty"/> を不正として検証)。
/// </remarks>
[TestClass]
[TestCategory("Infrastructure.Categories")]
public sealed class ProductCategoryEntityMapperTests
{
    private readonly ProductCategoryEntityMapper _mapper = new();

    private static ProductCategoryEntity Entity(Guid categoryUuid, string name)
        => new() { CategoryUuid = categoryUuid, Name = name };

    [TestMethod]
    public void ToDomain_有効なEntityをCategoryに変換できる()
    {
        var uuid = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var category = _mapper.ToDomain(Entity(uuid, "文房具"));

        Assert.AreEqual(uuid, category.CategoryId.Value);
        Assert.AreEqual("文房具", category.Name.Value);
    }

    [TestMethod]
    public void ToDomain_Entityがnullなら例外()
    {
        Assert.ThrowsExactly<DomainException>(() => _mapper.ToDomain(null!));
    }

    [TestMethod]
    public void ToDomain_category_uuidが空なら例外()
    {
        Assert.ThrowsExactly<DomainException>(() => _mapper.ToDomain(Entity(Guid.Empty, "文房具")));
    }

    [TestMethod]
    public void ToDomain_nameが空白なら例外()
    {
        var uuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Assert.ThrowsExactly<DomainException>(() => _mapper.ToDomain(Entity(uuid, "   ")));
    }
}