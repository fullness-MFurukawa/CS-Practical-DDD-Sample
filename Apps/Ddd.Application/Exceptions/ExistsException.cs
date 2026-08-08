// Apps/Ddd.Application/Exceptions/ExistsException.cs
namespace Ddd.Application.Exceptions;

/// <summary>「指定されたデータが既に存在する」ことを表すアプリケーション層の例外。</summary>
/// <remarks>
/// 登録処理で一意制約違反を検出した場合にスロー。ビジネス上の重複状態を表す。
/// 発生層=アプリケーション層 / 捕捉層=プレゼンテーション層(HTTP 409)。
/// </remarks>
public class ExistsException : Exception
{
    public ExistsException(string message) : base(message) { }
    public ExistsException(string message, Exception innerException) : base(message, innerException) { }
}