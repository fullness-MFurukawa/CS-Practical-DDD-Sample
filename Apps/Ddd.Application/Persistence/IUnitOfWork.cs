namespace Ddd.Application.Persistence;

/// <summary>
/// トランザクション境界(Unit of Work)を表すポート。
/// </summary>
/// <remarks>
/// <para>
/// 一連のドメイン操作を「ひとまとまり」として実行し、成功すればコミット、例外が発生すればロールバックする。
/// 契約(インターフェイス)はドメイン層に置き、実装はインフラストラクチャ層(EF Core のトランザクション)が担う。
/// トランザクションの<b>境界を決めるのはユースケース層</b>であり、ユースケースが本ポート越しに
/// 「この処理をひとまとまりで実行する」ことを表明する。
/// </para>
/// <para>
/// リポジトリやサービスは個々の操作に責務を絞り、複数操作の原子性(all-or-nothing)は本ポートで束ねる。
/// これにより、アプリケーション層は EF Core などの永続化技術に直接依存せずにトランザクション境界を制御できる。
/// </para>
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>指定した処理をトランザクション内で実行し、結果を返す。成功時はコミット、例外時はロールバックする。</summary>
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);

    /// <summary>指定した処理をトランザクション内で実行する(戻り値なし)。成功時はコミット、例外時はロールバックする。</summary>
    Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
}