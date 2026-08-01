using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Entities;
using Ddd.Infrastructure.Products;

namespace Ddd.Infrastructure.Tests.Products;

/// <summary>
/// <see cref="ProductEntityMapper"/>(Entity ⇔ Product)の単体テスト(DB不要)。
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
public sealed class ProductEntityMapperTests
{
    private readonly ProductEntityMapper _mapper = new();

    private static readonly Guid Uuid = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static ProductEntity Entity(Guid productUuid, string name, int price)
        => new() { ProductUuid = productUuid, Name = name, Price = price };

    // ---- ToDomain: Entity → Product(骨格) ----

    [TestMethod]
    public void ToDomain_有効なEntityを骨格Productに変換できる_カテゴリと在庫はnull()
    {
        var product = _mapper.ToDomain(Entity(Uuid, "油性ボールペン", 120));

        Assert.AreEqual(Uuid, product.ProductId.Value);
        Assert.AreEqual("油性ボールペン", product.Name.Value);
        Assert.AreEqual(120, product.Price.Value);
        // skeleton なのでカテゴリ・在庫は未設定(後段の Assembler が Attach する)
        Assert.IsNull(product.Category);
        Assert.IsNull(product.Stock);
    }

    [TestMethod]
    public void ToDomain_Entityがnullなら例外()
    {
        Assert.ThrowsExactly<DomainException>(() => _mapper.ToDomain(null!));
    }

    [TestMethod]
    public void ToDomain_product_uuidが空なら例外()
    {
        Assert.ThrowsExactly<DomainException>(() => _mapper.ToDomain(Entity(Guid.Empty, "商品", 120)));
    }

    [TestMethod]
    public void ToDomain_nameが空白なら例外()
    {
        Assert.ThrowsExactly<DomainException>(() => _mapper.ToDomain(Entity(Uuid, "   ", 120)));
    }

    [TestMethod]
    public void ToDomain_priceが範囲外なら例外()
    {
        Assert.ThrowsExactly<DomainException>(() => _mapper.ToDomain(Entity(Uuid, "商品", 10)));
    }

    // ---- FromDomain: Product → Entity(Mapperly 生成) ----

    [TestMethod]
    public void FromDomain_ProductをEntityに変換できる_categoryIdと主キーは未設定()
    {
        var category = Category.CreateNew(CategoryName.Create("文房具"));
        var product = Product.CreateNew(
            ProductName.Create("油性ボールペン"),
            ProductPrice.Create(120),
            category,
            StockQuantity.Create(80));

        var entity = _mapper.FromDomain(product);

        Assert.AreEqual(product.ProductId.Value, entity.ProductUuid);
        Assert.AreEqual("油性ボールペン", entity.Name);
        Assert.AreEqual(120, entity.Price);
        Assert.AreEqual(0, entity.CategoryId); // 外部キーは未設定(Repositoryが補完)
        Assert.AreEqual(0, entity.Id);         // 主キーは未採番
    }
}