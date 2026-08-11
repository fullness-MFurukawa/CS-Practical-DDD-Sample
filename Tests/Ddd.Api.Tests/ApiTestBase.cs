namespace Ddd.Api.Tests;

/// <summary>
/// コントローラテストの基盤。テストごとに <see cref="ApiTestFactory"/> と <see cref="HttpClient"/> を生成する。
/// </summary>
/// <remarks>
/// テストごとにファクトリを作り直すため、Fake のシードデータはテスト間で共有されない(状態が独立する)。
/// </remarks>
public abstract class ApiTestBase
{
    /// <summary>テストサーバのファクトリ(Fake リポジトリへのシード投入に使う)。</summary>
    protected ApiTestFactory Factory { get; private set; } = null!;

    /// <summary>テストサーバへ HTTP リクエストするクライアント。</summary>
    protected HttpClient Client { get; private set; } = null!;

    /// <summary>各テスト開始時にファクトリとクライアントを生成する。</summary>
    [TestInitialize]
    public void SetUp()
    {
        Factory = new ApiTestFactory();
        Client = Factory.CreateClient();
    }

    /// <summary>各テスト終了時に破棄する。</summary>
    [TestCleanup]
    public void TearDown()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }
}