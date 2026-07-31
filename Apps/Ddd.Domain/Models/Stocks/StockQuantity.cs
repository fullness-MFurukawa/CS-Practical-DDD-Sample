using Ddd.Domain.Exceptions;

namespace Ddd.Domain.Models.Stocks;

/// <summary>
/// 在庫数を表す値オブジェクト。不変・自己検証・値で等価。
/// </summary>
/// <remarks>
/// 仕様: 0以上100以下を有効値とする。内部型が <see cref="int"/> のため必須性は型で保証される。
/// </remarks>
public sealed record StockQuantity
{
    /// <summary>最小値。</summary>
    public const int Min = 0;

    /// <summary>最大値。</summary>
    public const int Max = 100;

    /// <summary>在庫数(不変)。</summary>
    public int Value { get; }

    private StockQuantity(int value) => Value = value;

    /// <summary>入力を検証して <see cref="StockQuantity"/> を生成する。</summary>
    /// <exception cref="DomainException">範囲外(0〜100)の場合。</exception>
    public static StockQuantity Create(int raw)
    {
        if (raw < Min || raw > Max)
        {
            throw new DomainException($"在庫数は {Min} 以上 {Max} 以下で指定してください。: {raw}");
        }
        return new StockQuantity(raw);
    }

    /// <summary>保持している値を文字列として返す。</summary>
    public override string ToString() => Value.ToString();
}