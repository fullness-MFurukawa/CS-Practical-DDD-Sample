using Ddd.Domain.Exceptions;

namespace Ddd.Domain.Models.Products;

/// <summary>
/// 商品単価を表す値オブジェクト。不変・自己検証・値で等価。
/// </summary>
/// <remarks>
/// 仕様: 50以上10000以下を有効値とする。内部型が <see cref="int"/> のため必須性は型で保証される。
/// </remarks>
public sealed record ProductPrice
{
    /// <summary>最小値。</summary>
    public const int MinPrice = 50;

    /// <summary>最大値。</summary>
    public const int MaxPrice = 10000;

    /// <summary>単価(不変)。</summary>
    public int Value { get; }

    private ProductPrice(int value) => Value = value;

    /// <summary>入力を検証して <see cref="ProductPrice"/> を生成する。</summary>
    /// <exception cref="DomainException">範囲外(50〜10000)の場合。</exception>
    public static ProductPrice Create(int raw)
    {
        if (raw < MinPrice || raw > MaxPrice)
        {
            throw new DomainException($"商品単価は {MinPrice} 以上 {MaxPrice} 以下で指定してください。: {raw}");
        }
        return new ProductPrice(raw);
    }

    /// <summary>保持している値を文字列として返す。</summary>
    public override string ToString() => Value.ToString();
}