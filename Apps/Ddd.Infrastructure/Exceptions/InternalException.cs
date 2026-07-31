namespace Ddd.Infrastructure.Exceptions;

/// <summary>
/// インフラストラクチャ層で発生する「技術的な異常状態」を表す例外。
/// </summary>
/// <remarks>
/// <para>
/// データベースの接続障害・タイムアウト、外部API通信の失敗、ファイルI/Oエラーなど、
/// アプリケーション外部の要因で発生し、業務ルールの違反ではない技術的エラーを通知する。
/// </para>
/// <para>
/// ドメイン層の <c>DomainException</c>(業務ルール違反)と本クラス(技術的エラー)を
/// 使い分けることで、「どの層で起きた問題か」を層の責務として明確に区別できる。
/// 通常はプレゼンテーション層で「ユーザー向けエラーレスポンス」に変換し、原因例外は
/// ログ・スタックトレースとして記録する。
/// </para>
/// </remarks>
public class InternalException : Exception
{
    /// <summary>指定したメッセージで技術的例外を生成する。</summary>
    /// <param name="message">エラーの内容を示すメッセージ。</param>
    public InternalException(string message) : base(message)
    {
    }

    /// <summary>指定したメッセージと原因例外で技術的例外を生成する。</summary>
    /// <param name="message">エラーの内容を示すメッセージ。</param>
    /// <param name="cause">原因となった例外(例: DbUpdateException, DbException, IOException など)。</param>
    public InternalException(string message, Exception cause) : base(message, cause)
    {
    }
}