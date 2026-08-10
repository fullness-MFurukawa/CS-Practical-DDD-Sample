using Ddd.Domain.Events;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Domain.Models.Products.Events;

/// <summary>在庫数が変更された、というドメインイベント。</summary>
/// <param name="ProductId">対象の商品Id。</param>
/// <param name="StockId">対象の在庫Id。</param>
/// <param name="OldQuantity">変更前の在庫数。</param>
/// <param name="NewQuantity">変更後の在庫数。</param>
public sealed record StockQuantityChanged(
    ProductId ProductId,
    StockId StockId,
    StockQuantity OldQuantity,
    StockQuantity NewQuantity) : IDomainEvent;