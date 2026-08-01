using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Entities;
using Ddd.Infrastructure.Products;

namespace Ddd.Infrastructure.Tests.Products;

/// <summary>
/// <see cref="ProductStockEntityMapper"/>(Entity ⇔ Stock)の単体テスト(DB不要)。
/// </summary>
/// <remarks>
/// <para>ToDomain は手書き(検証あり)、FromDomain は Mapperly 生成。</para>
/// <para>
/// Java版との差異: <c>Quantity</c> は <see cref="int"/> 型のため「在庫数が null」テストは不要。
/// FromDomain は Mapperly 生成で null 入力時の挙動が Mapperly 側のもの(<see cref="DomainException"/> ではない)に
/// なるため、Java版の「fromDomain(null) で例外」テストは移植しない。
/// </para>
/// </remarks>
[TestClass]
[TestCategory("Infrastructure.Products")]
public sealed class ProductStockEntityMapperTests
{
    private readonly ProductStockEntityMapper _mapper = new();

    private static readonly Guid Uuid = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static ProductStockEntity Entity(Guid stockUuid, int quantity)
        => new() { StockUuid = stockUuid, Quantity = quantity };

    // ---- ToDomain: Entity → Stock ----

    [TestMethod]
    public void ToDomain_有効なEntityをStockに変換できる()
    {
        var stock = _mapper.ToDomain(Entity(Uuid, 50));

        Assert.AreEqual(Uuid, stock.StockId.Value);
        Assert.AreEqual(50, stock.Quantity.Value);
    }

    [TestMethod]
    public void ToDomain_Entityがnullなら例外()
    {
        Assert.ThrowsExactly<DomainException>(() => _mapper.ToDomain(null!));
    }

    [TestMethod]
    public void ToDomain_stock_uuidが空なら例外()
    {
        Assert.ThrowsExactly<DomainException>(() => _mapper.ToDomain(Entity(Guid.Empty, 50)));
    }

    [TestMethod]
    public void ToDomain_在庫数が範囲外なら例外()
    {
        Assert.ThrowsExactly<DomainException>(() => _mapper.ToDomain(Entity(Uuid, 101)));
    }

    // ---- FromDomain: Stock → Entity(Mapperly 生成) ----

    [TestMethod]
    public void FromDomain_StockをEntityに変換できる_外部キーと主キーは未設定()
    {
        var stock = Stock.Restore(StockId.From(Uuid), StockQuantity.Create(30));

        var entity = _mapper.FromDomain(stock);

        Assert.AreEqual(Uuid, entity.StockUuid);
        Assert.AreEqual(30, entity.Quantity);
        Assert.AreEqual(0, entity.ProductId); // 外部キーは未設定(Repositoryが補完)
        Assert.AreEqual(0, entity.Id);        // 主キーは未採番
    }
}