

using Ddd.Application.Persistence;

namespace Ddd.Infrastructure.Tests.Persistence;

/// <summary>
/// <see cref="Ddd.Infrastructure.Persistence.UnitOfWork"/> のテスト。
/// </summary>
/// <remarks>
/// <para>
/// EF Core のトランザクション API(<c>BeginTransactionAsync</c> / <c>CurrentTransaction</c> /
/// <see cref="Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction"/>)は拡張メソッド・非仮想
/// メンバーのためモックできない。そこで <see cref="IUnitOfWork"/> は DI から実物(実 <c>AppDbContext</c>)で
/// 解決し、「トランザクション内で実行する処理」は素のラムダで渡して、実行回数・戻り値・例外伝播を検証する。
/// </para>
/// <para>
/// 本クラスは <see cref="InfrastructureTestBase"/> を継承する(外側トランザクションなし)。よって
/// <c>UnitOfWork</c> は自前でトランザクションを開始・コミット/ロールバックする経路を通る。渡す処理は
/// DB へ書き込まないため、コミットは空(無害)である。
/// </para>
/// </remarks>
[TestClass]
[TestCategory("Infrastructure.Persistence")]
public sealed class UnitOfWorkTests : InfrastructureTestBase
{
    private IUnitOfWork UnitOfWork => GetRequiredService<IUnitOfWork>();

    [TestMethod(DisplayName = "処理を1回実行し戻り値をそのまま返す")]
    public async Task ExecuteAsync_InvokesActionOnce_AndReturnsResult()
    {
        var calls = 0;

        var result = await UnitOfWork.ExecuteAsync(_ =>
        {
            calls++;
            return Task.FromResult(42);
        });

        Assert.AreEqual(42, result);
        Assert.AreEqual(1, calls);
    }

    [TestMethod(DisplayName = "処理が例外を投げるとロールバックして例外を再送出する")]
    public async Task ExecuteAsync_RethrowsException_WhenActionThrows()
    {
        var calls = 0;

        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => UnitOfWork.ExecuteAsync<int>(_ =>
            {
                calls++;
                throw new InvalidOperationException("boom");
            }));

        Assert.AreEqual("boom", ex.Message);
        Assert.AreEqual(1, calls);
    }

    [TestMethod(DisplayName = "戻り値なしのExecuteAsyncも処理を1回実行する")]
    public async Task ExecuteAsync_Void_InvokesActionOnce()
    {
        var calls = 0;

        await UnitOfWork.ExecuteAsync(_ =>
        {
            calls++;
            return Task.CompletedTask;
        });

        Assert.AreEqual(1, calls);
    }
}

/// <summary>
/// <see cref="Ddd.Infrastructure.Persistence.UnitOfWork"/> の「外側トランザクションへの参加」分岐のテスト。
/// </summary>
/// <remarks>
/// <see cref="DatabaseTestBase"/> は各テストをトランザクションで囲む。よって <c>UnitOfWork</c> は
/// 既存トランザクション(<c>CurrentTransaction</c> が非 null)を検出し、新規開始せずにそのまま処理を実行する。
/// </remarks>
[TestClass]
[TestCategory("Infrastructure.Persistence")]
public sealed class UnitOfWorkAmbientTransactionTests : DatabaseTestBase
{
    private IUnitOfWork UnitOfWork => GetRequiredService<IUnitOfWork>();

    [TestMethod(DisplayName = "外側にトランザクションがあるときは新規開始せず処理を実行し戻り値を返す")]
    public async Task ExecuteAsync_JoinsAmbientTransaction_AndReturnsResult()
    {
        var calls = 0;

        var result = await UnitOfWork.ExecuteAsync(_ =>
        {
            calls++;
            return Task.FromResult(7);
        });

        Assert.AreEqual(7, result);
        Assert.AreEqual(1, calls);
    }
}