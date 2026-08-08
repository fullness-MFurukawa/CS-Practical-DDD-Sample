using Ddd.Domain.Adapters;
using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Entities;

namespace Ddd.Infrastructure.Tests.Products;

/// <summary>
/// <see cref="Ddd.Infrastructure.Products.ProductStockEntityAdapter"/>(Entity ⇔ Stock)の
/// 単体テスト(DB不要)。テスト対象は DI コンテナから解決する。
/// </summary>
/// <remarks>
/// <para>ToDomain は手書き(検証あり)、FromDomain は Mapperly 生成。</para>
/// <para>
/// Java版との差異: <c>Quantity</c> は <see cref="int"/> 型のため「在庫数が null」テストは不要。
/// FromDomain は Mapperly 生成で null 入力時の挙動が Mapperly 側のものになるため、
/// Java版の「fromDomain(null) で例外」テストは移植しない。
/// </para>
/// </remarks>
[TestClass]
[TestCategory("Infrastructure.Products")]
public sealed class ProductStockEntityAdapterTests : InfrastructureTestBase
{
    private IDomainBiAdapter<ProductStockEntity, Stock> Adapter
        => GetRequiredService<IDomainBiAdapter<ProductStockEntity, Stock>>();

    private static readonly Guid Uuid = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static ProductStockEntity Entity(Guid stockUuid, int quantity)
        => new() { StockUuid = stockUuid, Quantity = quantity };

    [TestMethod(DisplayName = "有効なEntityをStockに変換できる")]
    public void ToDomain_ConvertsValidEntity()
    {
        var stock = Adapter.ToDomain(Entity(Uuid, 50));

        Assert.AreEqual(Uuid, stock.StockId.Value);
        Assert.AreEqual(50, stock.Quantity.Value);
    }

    [TestMethod(DisplayName = "Entityがnullなら例外")]
    public void ToDomain_ThrowsWhenEntityIsNull()
    {
        Assert.ThrowsExactly<DomainException>(() => Adapter.ToDomain(null!));
    }

    [TestMethod(DisplayName = "stock_uuidが空なら例外")]
    public void ToDomain_ThrowsWhenUuidIsEmpty()
    {
        Assert.ThrowsExactly<DomainException>(() => Adapter.ToDomain(Entity(Guid.Empty, 50)));
    }

    [TestMethod(DisplayName = "在庫数が範囲外なら例外")]
    public void ToDomain_ThrowsWhenQuantityOutOfRange()
    {
        Assert.ThrowsExactly<DomainException>(() => Adapter.ToDomain(Entity(Uuid, 101)));
    }

    [TestMethod(DisplayName = "StockをEntityに変換できる_外部キーと主キーは未設定")]
    public void FromDomain_ConvertsStockWithKeysUnset()
    {
        var stock = Stock.Restore(StockId.From(Uuid), StockQuantity.Create(30));

        var entity = Adapter.FromDomain(stock);

        Assert.AreEqual(Uuid, entity.StockUuid);
        Assert.AreEqual(30, entity.Quantity);
        Assert.AreEqual(0, entity.ProductId); // 外部キーは未設定(Repositoryが補完)
        Assert.AreEqual(0, entity.Id);        // 主キーは未採番
    }
}