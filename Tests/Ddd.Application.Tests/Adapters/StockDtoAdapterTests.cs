using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;
using Ddd.Domain.Adapters;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Application.Tests.Adapters;

/// <summary>
/// <see cref="Ddd.Application.Adapters.StockDtoAdapter"/>(<see cref="StockDto"/> ⇔ <see cref="Stock"/>)の
/// テスト。テスト対象は DI コンテナ(<c>AddApplication</c>)から解決する。
/// </summary>
[TestClass]
[TestCategory("Application.Adapters")]
public sealed class StockDtoAdapterTests : ApplicationTestBase
{
    private IDomainBiAdapter<StockDto, Stock> Adapter
        => GetRequiredService<IDomainBiAdapter<StockDto, Stock>>();

    private const string Uuid = "0f8fad5b-d9cb-469f-a165-70867728950e";

    [TestMethod(DisplayName = "Id未指定なら新規在庫を生成する")]
    public void ToDomain_CreatesNew_WhenIdMissing()
    {
        var stock = Adapter.ToDomain(new StockDto(null, 30));

        Assert.AreEqual(30, stock.Quantity.Value);
        Assert.AreNotEqual(Guid.Empty, stock.StockId.Value);
    }

    [TestMethod(DisplayName = "Id指定なら復元する")]
    public void ToDomain_Restores_WhenIdGiven()
    {
        var stock = Adapter.ToDomain(new StockDto(Uuid, 30));

        Assert.AreEqual(Uuid, stock.StockId.ToString());
        Assert.AreEqual(30, stock.Quantity.Value);
    }

    [TestMethod(DisplayName = "DTOがnullなら例外")]
    public void ToDomain_ThrowsWhenNull()
        => Assert.ThrowsExactly<InvalidInputException>(() => Adapter.ToDomain(null!));

    [TestMethod(DisplayName = "在庫数が未指定なら例外")]
    public void ToDomain_ThrowsWhenQuantityNull()
    {
        var ex = Assert.ThrowsExactly<InvalidInputException>(
            () => Adapter.ToDomain(new StockDto(Uuid, null)));
        Assert.AreEqual("在庫数は必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "StockをDTOに変換する")]
    public void FromDomain_ConvertsToDto()
    {
        var stock = Stock.Restore(StockId.Parse(Uuid), StockQuantity.Create(30));

        var dto = Adapter.FromDomain(stock);

        Assert.AreEqual(Uuid, dto.Id);
        Assert.AreEqual(30, dto.Quantity);
    }

    [TestMethod(DisplayName = "Stockがnullなら例外")]
    public void FromDomain_ThrowsWhenNull()
        => Assert.ThrowsExactly<InvalidInputException>(() => Adapter.FromDomain(null!));
}