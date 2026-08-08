using Ddd.Domain.Adapters;
using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Entities;

namespace Ddd.Infrastructure.Tests.Products;

/// <summary>
/// <see cref="Ddd.Infrastructure.Products.ProductEntityAdapter"/>(Entity ⇔ Product)の
/// 単体テスト(DB不要)。テスト対象は DI コンテナから解決する。
/// </summary>
/// <remarks>
/// <para>
/// ToDomain は「カテゴリ・在庫を伴わない骨格(skeleton)」の <see cref="Product"/> を返す(手書き・検証あり)。
/// FromDomain は <c>category_id</c> を設定しない(Repositoryが補完する / Mapperly 生成)。
/// </para>
/// <para>
/// Java版との差異: <c>price</c> は <see cref="int"/> 型のため「price が null」テストは不要。
/// FromDomain は Mapperly 生成のため「fromDomain(null) で例外」テストは移植しない。
/// </para>
/// </remarks>
[TestClass]
[TestCategory("Infrastructure.Products")]
public sealed class ProductEntityAdapterTests : InfrastructureTestBase
{
    private IDomainBiAdapter<ProductEntity, Product> Adapter
        => GetRequiredService<IDomainBiAdapter<ProductEntity, Product>>();

    private static readonly Guid Uuid = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static ProductEntity Entity(Guid productUuid, string name, int price)
        => new() { ProductUuid = productUuid, Name = name, Price = price };

    [TestMethod(DisplayName = "有効なEntityを骨格Productに変換できる_カテゴリと在庫はnull")]
    public void ToDomain_ConvertsValidEntityToSkeleton()
    {
        var product = Adapter.ToDomain(Entity(Uuid, "油性ボールペン", 120));

        Assert.AreEqual(Uuid, product.ProductId.Value);
        Assert.AreEqual("油性ボールペン", product.Name.Value);
        Assert.AreEqual(120, product.Price.Value);
        // skeleton なのでカテゴリ・在庫は未設定(後段の ProductFactory が Attach する)
        Assert.IsNull(product.Category);
        Assert.IsNull(product.Stock);
    }

    [TestMethod(DisplayName = "Entityがnullなら例外")]
    public void ToDomain_ThrowsWhenEntityIsNull()
    {
        Assert.ThrowsExactly<DomainException>(() => Adapter.ToDomain(null!));
    }

    [TestMethod(DisplayName = "product_uuidが空なら例外")]
    public void ToDomain_ThrowsWhenUuidIsEmpty()
    {
        Assert.ThrowsExactly<DomainException>(() => Adapter.ToDomain(Entity(Guid.Empty, "商品", 120)));
    }

    [TestMethod(DisplayName = "nameが空白なら例外")]
    public void ToDomain_ThrowsWhenNameIsBlank()
    {
        Assert.ThrowsExactly<DomainException>(() => Adapter.ToDomain(Entity(Uuid, "   ", 120)));
    }

    [TestMethod(DisplayName = "priceが範囲外なら例外")]
    public void ToDomain_ThrowsWhenPriceOutOfRange()
    {
        Assert.ThrowsExactly<DomainException>(() => Adapter.ToDomain(Entity(Uuid, "商品", 10)));
    }

    [TestMethod(DisplayName = "ProductをEntityに変換できる_categoryIdと主キーは未設定")]
    public void FromDomain_ConvertsProductWithKeysUnset()
    {
        var category = Category.CreateNew(CategoryName.Create("文房具"));
        var product = Product.CreateNew(
            ProductName.Create("油性ボールペン"),
            ProductPrice.Create(120),
            category,
            StockQuantity.Create(80));

        var entity = Adapter.FromDomain(product);

        Assert.AreEqual(product.ProductId.Value, entity.ProductUuid);
        Assert.AreEqual("油性ボールペン", entity.Name);
        Assert.AreEqual(120, entity.Price);
        Assert.AreEqual(0, entity.CategoryId); // 外部キーは未設定(Repositoryが補完)
        Assert.AreEqual(0, entity.Id);         // 主キーは未採番
    }
}