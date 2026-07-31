using Ddd.Domain.Exceptions;

namespace Ddd.Domain.Models.Stocks;

/// <summary>
/// 商品在庫を一意に識別する値オブジェクト。内部表現は <see cref="Guid"/>。
/// </summary>
public sealed record StockId
{
    /// <summary>識別子の値(不変)。</summary>
    public Guid Value { get; }

    private StockId(Guid value) => Value = value;

    /// <summary>新しい識別子を発行する。</summary>
    public static StockId New() => new(Guid.NewGuid());

    /// <summary>既存の <see cref="Guid"/> から復元する。</summary>
    /// <exception cref="DomainException"><paramref name="value"/> が空(Guid.Empty)の場合。</exception>
    public static StockId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("在庫IDは必須です。");
        }
        return new StockId(value);
    }

    /// <summary>UUID文字列(8-4-4-4-12形式)から復元する。</summary>
    /// <exception cref="DomainException">null/空白、またはUUID形式でない場合。</exception>
    public static StockId Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new DomainException("在庫IDは必須です。");
        }
        if (!Guid.TryParseExact(raw.Trim(), "D", out var value))
        {
            throw new DomainException($"在庫IDはUUID形式で指定してください。: {raw}");
        }
        return new StockId(value);
    }

    /// <summary>正規化済みのUUID文字列(小文字・ハイフン付き36文字)。</summary>
    public override string ToString() => Value.ToString();
}