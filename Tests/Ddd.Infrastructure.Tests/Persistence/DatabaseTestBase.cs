using Ddd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace Ddd.Infrastructure.Tests.Persistence;

/// <summary>
/// 実 PostgreSQL に接続する結合テストの共通基盤。
/// </summary>
/// <remarks>
/// <para>
/// <c>appsettings.Test.json</c> の接続文字列 <c>Postgres</c> を読み、<see cref="AppDbContext"/> を生成する。
/// 各テストの前後で「トランザクション開始 → テスト → ロールバック」を行い、テストデータで実 DB を汚さない。
/// </para>
/// <para>
/// 派生テストは、この基盤が提供する <see cref="DbContext"/> を Repository 等に渡すことで、
/// 同一トランザクション内で操作させ、確実にロールバックさせる。前提として、対象 DB に
/// スキーマ(テーブル)とサンプルデータが作成済みであること。
/// </para>
/// </remarks>
public abstract class DatabaseTestBase
{
    /// <summary>appsettings.Test.json から解決した接続文字列(全テスト共通)。</summary>
    protected static string ConnectionString { get; } = LoadConnectionString();

    /// <summary>各テストで利用する DbContext(トランザクション内)。</summary>
    protected AppDbContext DbContext { get; private set; } = null!;

    private IDbContextTransaction _transaction = null!;

    private static string LoadConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: false)
            .Build();

        return configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "接続文字列 'Postgres' が appsettings.Test.json に設定されていません。");
    }

    /// <summary>接続文字列から <see cref="AppDbContext"/> を生成する。</summary>
    protected static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new AppDbContext(options);
    }

    /// <summary>各テスト開始時: DbContext を生成し、トランザクションを開始する。</summary>
    [TestInitialize]
    public void InitializeDatabase()
    {
        DbContext = CreateDbContext();
        _transaction = DbContext.Database.BeginTransaction();
    }

    /// <summary>各テスト終了時: ロールバックして後始末する(テストデータを残さない)。</summary>
    [TestCleanup]
    public void CleanupDatabase()
    {
        _transaction.Rollback();
        _transaction.Dispose();
        DbContext.Dispose();
    }
}