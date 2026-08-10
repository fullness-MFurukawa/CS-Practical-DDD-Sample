using Ddd.Domain.Events;

namespace Ddd.Application.Events;
/// <summary>
/// 特定のドメインイベント <typeparamref name="TEvent"/> に反応するハンドラの契約。
/// </summary>
/// <remarks>
/// <para>
/// 型引数で「このハンドラが反応できるイベント種別」を明示する。これにより <see cref="HandleAsync"/> は
/// 具体型のイベントをそのまま受け取れ(キャスト不要・型安全)、DI コンテナはイベント種別ごとに
/// ハンドラを登録・解決できる。1つのイベントに対して複数のハンドラを登録してよい。
/// </para>
/// <para>
/// 契約(ポート)はアプリケーション層に置き、実装(反応の中身)は外側の層(インフラストラクチャ層など)に置く。
/// </para>
/// </remarks>
/// <typeparam name="TEvent">反応対象のドメインイベントの型。</typeparam>
public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// ドメインイベントに反応する処理を実行する。
    /// </summary>
    /// <param name="domainEvent">発生したドメインイベント。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}