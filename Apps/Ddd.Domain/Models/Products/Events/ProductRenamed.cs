using Ddd.Domain.Events;

namespace Ddd.Domain.Models.Products.Events;

/// <summary>商品名が変更された、というドメインイベント。</summary>
/// <param name="ProductId">対象の商品Id。</param>
/// <param name="OldName">変更前の商品名。</param>
/// <param name="NewName">変更後の商品名。</param>
public sealed record ProductRenamed(ProductId ProductId, ProductName OldName, ProductName NewName) : IDomainEvent;