    using Ddd.Application.Events;
using Ddd.Domain.Models.Products.Events;
using Microsoft.Extensions.Logging;

namespace Ddd.Infrastructure.Events.Handlers;

/// <summary>
/// <see cref="ProductRenamed"/>(商品名変更)に反応して、変更内容をログに記録するサンプルハンドラ。
/// </summary>
/// <remarks>
/// ドメインイベントへの「反応(副作用)」の一例。ロギングは技術的関心事のためインフラストラクチャ層に置く。
/// </remarks>
/// <param name="logger">ロガー。</param>
public sealed class ProductRenamedLoggingHandler(ILogger<ProductRenamedLoggingHandler> logger)
    : IDomainEventHandler<ProductRenamed>
{
    /// <inheritdoc />
    public Task HandleAsync(ProductRenamed domainEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "商品名が変更されました。ProductId={ProductId}, {OldName} -> {NewName}",
            domainEvent.ProductId, domainEvent.OldName.Value, domainEvent.NewName.Value);
        return Task.CompletedTask;
    }
}