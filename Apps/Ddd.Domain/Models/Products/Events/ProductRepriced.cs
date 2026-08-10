using Ddd.Domain.Events;

namespace Ddd.Domain.Models.Products.Events;

/// <summary>商品単価が変更された、というドメインイベント。</summary>
/// <param name="ProductId">対象の商品Id。</param>
/// <param name="OldPrice">変更前の単価。</param>
/// <param name="NewPrice">変更後の単価。</param>
public sealed record ProductRepriced(ProductId ProductId, ProductPrice OldPrice, ProductPrice NewPrice) : IDomainEvent;