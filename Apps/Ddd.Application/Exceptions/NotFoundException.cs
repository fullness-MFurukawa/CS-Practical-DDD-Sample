// Apps/Ddd.Application/Exceptions/NotFoundException.cs
namespace Ddd.Application.Exceptions;

/// <summary>指定されたデータが存在しないことを表すアプリケーション層の例外。</summary>
/// <remarks>
/// リポジトリ検索で該当なしの場合にスロー。ビジネス上の「リソース不在」を表現する。
/// 発生層=アプリケーション層 / 捕捉層=プレゼンテーション層(HTTP 404)。
/// </remarks>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}