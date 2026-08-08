using Ddd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ddd.Infrastructure.Tests.Persistence;

/// <summary>
/// 実 PostgreSQL に接続する結合テストの共通基盤(<see cref="InfrastructureTestBase"/> を継承)。
/// </summary>
/// <remarks>
/// スコープ内の <see cref="AppDbContext"/> を DI から解決し、各テストの前後で
/// 「トランザクション開始 → テスト → ロールバック」を行う。Repository も同じスコープから解決されるため、
/// この <see cref="DbContext"/> と同一インスタンスを共有し、同一トランザクション内で動作する。
/// <c>GetRequiredService&lt;T&gt;()</c> は基底 <see cref="InfrastructureTestBase"/> から継承する。
/// </remarks>
public abstract class DatabaseTestBase : InfrastructureTestBase
{
    /// <summary>スコープから解決した DbContext(トランザクション内)。</summary>
    protected AppDbContext DbContext { get; private set; } = null!;

    private IDbContextTransaction _transaction = null!;

    /// <summary>各テスト開始時: スコープの DbContext を取得し、トランザクションを開始する。</summary>
    /// <remarks>基底の <see cref="InfrastructureTestBase.InitializeScope"/> の後に実行される。</remarks>
    [TestInitialize]
    public void BeginTransaction()
    {
        DbContext = GetRequiredService<AppDbContext>();
        _transaction = DbContext.Database.BeginTransaction();
    }

    /// <summary>各テスト終了時: ロールバックする(スコープ破棄より前に実行される)。</summary>
    [TestCleanup]
    public void RollbackTransaction()
    {
        _transaction.Rollback();
        _transaction.Dispose();
    }
}