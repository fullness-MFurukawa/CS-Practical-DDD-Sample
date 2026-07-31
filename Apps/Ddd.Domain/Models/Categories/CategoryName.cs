using Ddd.Domain.Exceptions;

namespace Ddd.Domain.Models.Categories;

/// <summary>
/// カテゴリ名を表す値オブジェクト。不変・自己検証・値で等価。
/// </summary>
/// <remarks>
/// 仕様: 必須(null/空/空白のみ不可)、最大20文字、前後の空白はトリムされる。
/// </remarks>
public sealed record CategoryName
{
    /// <summary>カテゴリ名の最大長。</summary>
    public const int MaxLength = 20;

    /// <summary>トリム済みの値(不変)。</summary>
    public string Value { get; }

    private CategoryName(string value) => Value = value;

    /// <summary>入力を検証して <see cref="CategoryName"/> を生成する。</summary>
    /// <exception cref="DomainException">必須違反・空・最大長超過の場合。</exception>
    public static CategoryName Create(string raw)
    {
        if (raw is null)
        {
            throw new DomainException("カテゴリ名は必須です。");
        }
        var trimmed = raw.Trim();
        if (trimmed.Length == 0)
        {
            throw new DomainException("カテゴリ名は空にできません。");
        }
        if (trimmed.Length > MaxLength)
        {
            throw new DomainException($"カテゴリ名は{MaxLength}文字以内で指定してください。: {trimmed}");
        }
        return new CategoryName(trimmed);
    }

    /// <summary>保持している値をそのまま返す。</summary>
    public override string ToString() => Value;
}