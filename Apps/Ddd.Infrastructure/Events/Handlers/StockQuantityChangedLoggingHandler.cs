using Ddd.Application.Events;
using Ddd.Domain.Models.Products.Events;
using Microsoft.Extensions.Logging;

namespace Ddd.Infrastructure.Events.Handlers;

/// <summary>
/// <see cref="StockQuantityChanged"/>(在庫数変更)に反応して、変更内容をログに記録するサンプルハンドラ。
/// </summary>
/// <param name="logger">ロガー。</param>
public sealed class StockQuantityChangedLoggingHandler(ILogger<StockQuantityChangedLoggingHandler> logger)
    : IDomainEventHandler<StockQuantityChanged>
{
    /// <inheritdoc />
    public Task HandleAsync(StockQuantityChanged domainEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "在庫数が変更されました。ProductId={ProductId}, StockId={StockId}, {OldQuantity} -> {NewQuantity}",
            domainEvent.ProductId, domainEvent.StockId, domainEvent.OldQuantity.Value, domainEvent.NewQuantity.Value);
        return Task.CompletedTask;
    }
}