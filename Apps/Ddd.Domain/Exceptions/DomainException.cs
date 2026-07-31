namespace Ddd.Domain.Exceptions;

/// <summary>
/// ドメイン層における「不変条件(Invariant)」や「ビジネスルール」違反を表現するための例外。
/// </summary>
/// <remarks>
/// <para>
/// この例外は、システム障害や技術的異常ではなく「ドメイン上の意味的な不整合(ルール違反)」を
/// 明示的に表すために使用する。値オブジェクトの生成時の不正値、エンティティの不正な状態遷移、
/// ドメインサービスでの検証失敗などが対象となる。
/// </para>
/// <para>
/// これにより、ドメイン層の「検証エラー」と技術的例外(例: DB接続失敗を表す InternalException)を
/// 明確に区別でき、アプリケーション層/プレゼンテーション層で適切にハンドリングできる。
/// </para>
/// </remarks>
public class DomainException : Exception
{
    /// <summary>ドメイン例外をメッセージ付きで生成する。</summary>
    /// <param name="message">ドメインルール違反の説明メッセージ。</param>
    public DomainException(string message) : base(message)
    {
    }

    /// <summary>ドメイン例外をメッセージと原因例外付きで生成する。</summary>
    /// <param name="message">ドメインルール違反の説明メッセージ。</param>
    /// <param name="cause">原因となった例外。</param>
    public DomainException(string message, Exception cause) : base(message, cause)
    {
    }
}