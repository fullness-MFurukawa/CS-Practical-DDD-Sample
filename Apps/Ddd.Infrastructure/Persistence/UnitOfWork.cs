
using Ddd.Application.Persistence;

namespace Ddd.Infrastructure.Persistence;

/// <summary>
/// <see cref="IUnitOfWork"/> の EF Core による実装。
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AppDbContext"/> のデータベーストランザクションで処理を囲み、成功時はコミット、
/// 例外時はロールバックする。<see cref="AppDbContext"/> と同一スコープで解決されるため、
/// 同一接続・同一トランザクションで各リポジトリの操作が実行される。
/// </para>
/// <para>
/// 既に外側でトランザクションが開始されている場合(テストで各ケースをトランザクションで囲み最後に
/// ロールバックする構成など)は、新たにトランザクションを開始せず、外側のトランザクションに参加する
/// (コミット/ロールバックは外側の所有者に委ねる)。EF Core / Npgsql は入れ子のトランザクション開始を
/// 許可しないため、この分岐が必要となる。
/// </para>
/// </remarks>
/// <param name="dbContext">データアクセスの窓口となる <see cref="AppDbContext"/>。</param>
public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    /// <inheritdoc />
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        // 既に外側のトランザクションがある場合は、それに参加する(新規開始しない)。
        if (dbContext.Database.CurrentTransaction is not null)
        {
            return await action(cancellationToken);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        => ExecuteAsync<object?>(async token =>
        {
            await action(token);
            return null;
        }, cancellationToken);
}

