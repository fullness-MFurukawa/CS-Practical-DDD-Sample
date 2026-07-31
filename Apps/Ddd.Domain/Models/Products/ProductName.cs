using Ddd.Domain.Exceptions;

namespace Ddd.Domain.Models.Products;

/// <summary>
/// 商品名を表す値オブジェクト。不変・自己検証・値で等価。
/// </summary>
/// <remarks>
/// 仕様: 必須(null/空/空白のみ不可)、最大30文字、前後の空白はトリムされる。
/// </remarks>
public sealed record ProductName
{
    /// <summary>商品名の最大長。</summary>
    public const int MaxLength = 30;

    /// <summary>トリム済みの値(不変)。</summary>
    public string Value { get; }

    private ProductName(string value) => Value = value;

    /// <summary>入力を検証して <see cref="ProductName"/> を生成する。</summary>
    /// <exception cref="DomainException">必須違反・空・最大長超過の場合。</exception>
    public static ProductName Create(string raw)
    {
        if (raw is null)
        {
            throw new DomainException("商品名は必須です。");
        }
        var trimmed = raw.Trim();
        if (trimmed.Length == 0)
        {
            throw new DomainException("商品名は空にできません。");
        }
        if (trimmed.Length > MaxLength)
        {
            throw new DomainException($"商品名は{MaxLength}文字以内で指定してください。: {trimmed}");
        }
        return new ProductName(trimmed);
    }

    /// <summary>保持している値をそのまま返す。</summary>
    public override string ToString() => Value;
}