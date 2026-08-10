using Ddd.Domain.Events;

namespace Ddd.Application.Events;

/// <summary>
/// ドメインイベントを、対応する <see cref="IDomainEventHandler{TEvent}"/> へ配送するディスパッチャの契約(ポート)。
/// </summary>
/// <remarks>
/// <para>
/// 集約から取り出したイベント群を受け取り、各イベントの実際の型に一致するハンドラを解決して順に実行する。
/// 「集約は発行するだけ」「ユースケースは境界を決めるだけ」で、実際の配送はこのポート越しに行う。
/// </para>
/// <para>
/// 契約(ポート)はアプリケーション層に置き、実装(ハンドラの解決・呼び出し)は外側の層
/// (インフラストラクチャ層)が担う。UnitOfWork と同じ方針。
/// </para>
/// </remarks>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// 指定されたドメインイベント群を、それぞれに対応するハンドラへ配送する。
    /// </summary>
    /// <param name="domainEvents">配送するドメインイベント(発生順)。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}