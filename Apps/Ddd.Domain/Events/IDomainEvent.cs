namespace Ddd.Domain.Events;

/// <summary>
/// ドメインイベント(ドメインで意味のある「起きた出来事」)を表すマーカーインターフェイス。
/// </summary>
/// <remarks>
/// <para>
/// すべてのドメインイベント(<c>ProductRenamed</c> など)はこのインターフェイスを実装する。
/// 中身を持たない「目印」であり、集約が発行したイベントを <see cref="IDomainEvent"/> としてまとめて扱い、
/// ディスパッチャが実際の型ごとにハンドラへ振り分けられるようにするためのもの。
/// </para>
/// <para>
/// すべてのイベントに共通させたい性質(例: 発生時刻)が必要になれば、本インターフェイスに追加していく。
/// 現時点では分類のためのマーカーに徹する。
/// </para>
/// </remarks>
public interface IDomainEvent
{
}