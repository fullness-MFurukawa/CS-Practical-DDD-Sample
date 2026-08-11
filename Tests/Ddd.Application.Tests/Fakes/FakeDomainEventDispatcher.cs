using Ddd.Application.Events;
using Ddd.Domain.Events;

namespace Ddd.Application.Tests.Fakes;

/// <summary>
/// テスト用の <see cref="IDomainEventDispatcher"/>。実際には配送せず、渡されたイベントを記録するだけ。
/// </summary>
/// <remarks>
/// 「ユースケースが期待どおりのイベントを配送したか」を <see cref="Dispatched"/> で検証するために使う。
/// </remarks>
public sealed class FakeDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly List<IDomainEvent> _dispatched = new();

    /// <summary>これまでに配送(記録)されたイベント(発生順)。</summary>
    public IReadOnlyList<IDomainEvent> Dispatched => _dispatched;

    /// <inheritdoc />
    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        _dispatched.AddRange(domainEvents);
        return Task.CompletedTask;
    }
}