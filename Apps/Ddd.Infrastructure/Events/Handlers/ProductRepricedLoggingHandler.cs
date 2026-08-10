using Ddd.Application.Events;
using Ddd.Domain.Models.Products.Events;
using Microsoft.Extensions.Logging;

namespace Ddd.Infrastructure.Events.Handlers;

/// <summary>
/// <see cref="ProductRepriced"/>(単価変更)に反応して、変更内容をログに記録するサンプルハンドラ。
/// </summary>
/// <param name="logger">ロガー。</param>
public sealed class ProductRepricedLoggingHandler(ILogger<ProductRepricedLoggingHandler> logger)
    : IDomainEventHandler<ProductRepriced>
{
    /// <inheritdoc />
    public Task HandleAsync(ProductRepriced domainEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "商品単価が変更されました。ProductId={ProductId}, {OldPrice}円 -> {NewPrice}円",
            domainEvent.ProductId, domainEvent.OldPrice.Value, domainEvent.NewPrice.Value);
        return Task.CompletedTask;
    }
}