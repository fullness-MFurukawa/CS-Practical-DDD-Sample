using Ddd.Application.Persistence;

namespace Ddd.Application.Tests.Fakes;

/// <summary>
/// テスト用の <see cref="IUnitOfWork"/>。トランザクションは張らず、渡された処理をそのまま実行する。
/// </summary>
public sealed class FakeUnitOfWork : IUnitOfWork
{
    public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        => action(cancellationToken);

    public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        => action(cancellationToken);
}