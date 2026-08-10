using Ddd.Application.Events;
using Ddd.Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Ddd.Infrastructure.Events;

/// <summary>
/// <see cref="IDomainEventDispatcher"/> の実装(自作のインプロセス・ディスパッチャ)。
/// </summary>
/// <remarks>
/// <para>
/// 受け取ったイベント群を1つずつ処理する。各イベントは静的には <see cref="IDomainEvent"/> だが、
/// 実行時の型は具体イベント(例: <c>ProductRenamed</c>)である。その実行時型から閉じた総称型
/// <c>IDomainEventHandler&lt;具体型&gt;</c> を組み立て、DI コンテナに登録された当該ハンドラをすべて解決し、
/// <c>HandleAsync</c> を順に呼び出す(インプロセス=同一プロセス内で同期的に配送)。
/// </para>
/// <para>
/// 登録済みハンドラを実行時型で解決するため、<see cref="IServiceProvider"/> を直接用いる。
/// ハンドラが例外を投げた場合はそのまま伝播する(呼び出し側のトランザクション境界で巻き戻せる)。
/// </para>
/// </remarks>
/// <param name="serviceProvider">ハンドラを解決するためのサービスプロバイダ(現在のスコープ)。</param>
public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    /// <inheritdoc />
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            // 実行時のイベント型(例: ProductRenamed)から、対応するハンドラの閉じた総称型
            // IDomainEventHandler<ProductRenamed> を組み立てる。
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;

            // その型で登録されたハンドラを DI からすべて解決し、順に呼び出す。
            foreach (var handler in serviceProvider.GetServices(handlerType))
            {
                if (handler is null)
                {
                    continue;
                }

                await (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
            }
        }
    }
}