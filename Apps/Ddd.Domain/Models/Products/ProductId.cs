using Ddd.Domain.Exceptions;

namespace Ddd.Domain.Models.Products;

/// <summary>
/// 商品を一意に識別する値オブジェクト。
/// </summary>
/// <remarks>
/// <para>内部表現は <see cref="Guid"/>。外部から直接生成させず、生成・検証の責務を型に閉じ込める。</para>
/// <para>等価性は値(<see cref="Value"/>)で判定される。</para>
/// </remarks>
public sealed record ProductId
{
    /// <summary>識別子の値(不変)。</summary>
    public Guid Value { get; }

    private ProductId(Guid value) => Value = value;

    /// <summary>新しい識別子を発行する。</summary>
    public static ProductId New() => new(Guid.NewGuid());

    /// <summary>既存の <see cref="Guid"/> から復元する。</summary>
    /// <exception cref="DomainException"><paramref name="value"/> が空(Guid.Empty)の場合。</exception>
    public static ProductId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("商品IDは必須です。");
        }
        return new ProductId(value);
    }

    /// <summary>UUID文字列(8-4-4-4-12形式)から復元する。</summary>
    /// <exception cref="DomainException">null/空白、またはUUID形式でない場合。</exception>
    public static ProductId Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new DomainException("商品IDは必須です。");
        }
        if (!Guid.TryParseExact(raw.Trim(), "D", out var value))
        {
            throw new DomainException($"商品IDはUUID形式で指定してください。: {raw}");
        }
        return new ProductId(value);
    }

    /// <summary>正規化済みのUUID文字列(小文字・ハイフン付き36文字)。</summary>
    public override string ToString() => Value.ToString();
}