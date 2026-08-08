// Apps/Ddd.Application/Exceptions/InvalidInputException.cs
namespace Ddd.Application.Exceptions;

/// <summary>アプリケーション層で受け取った入力データが不正であることを表す例外。</summary>
/// <remarks>
/// 入力変換(Adapter)やユースケースで、ドメイン検証に到達する前に弾く入力不備(必須欠落等)を表す。
/// UUID形式不正・価格負数など値オブジェクトの不変条件は DomainException が担うため対象外。
/// 発生層=アプリケーション層 / 捕捉層=プレゼンテーション層(HTTP 400)。
/// </remarks>
public class InvalidInputException : Exception
{
    public InvalidInputException(string message) : base(message) { }
    public InvalidInputException(string message, Exception innerException) : base(message, innerException) { }
}