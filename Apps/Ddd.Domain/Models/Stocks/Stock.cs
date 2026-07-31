using Ddd.Domain.Exceptions;

namespace Ddd.Domain.Models.Stocks;

/// <summary>
/// 商品在庫を表すエンティティ。同一性は <see cref="StockId"/> で判定する。
/// </summary>
public sealed class Stock : IEquatable<Stock>
{
    /// <summary>在庫の同一性(不変)。</summary>
    public StockId StockId { get; }

    /// <summary>在庫数(VO)。</summary>
    public StockQuantity Quantity { get; private set; }

    private Stock(StockId id, StockQuantity quantity)
    {
        StockId = id ?? throw new DomainException("在庫IDは必須です。");
        Quantity = quantity ?? throw new DomainException("在庫数は必須です。");
    }

    /// <summary>新規作成。</summary>
    public static Stock CreateNew(StockQuantity initialQuantity)
        => new(StockId.New(), initialQuantity ?? throw new DomainException("在庫数は必須です。"));

    /// <summary>識別子を指定して再構築(リストア)する。</summary>
    public static Stock Restore(StockId id, StockQuantity quantity) => new(id, quantity);

    /// <summary>在庫数を加算する。</summary>
    /// <exception cref="DomainException">増分が負、または加算後が有効範囲外の場合。</exception>
    public void Increase(int amount)
    {
        if (amount < 0)
        {
            throw new DomainException($"在庫の増分は0以上で指定してください。: {amount}");
        }
        Quantity = StockQuantity.Create(Quantity.Value + amount);
    }

    /// <summary>在庫数を減算する。</summary>
    /// <exception cref="DomainException">減分が負、または減算後が有効範囲外の場合。</exception>
    public void Decrease(int amount)
    {
        if (amount < 0)
        {
            throw new DomainException($"在庫の減分は0以上で指定してください。: {amount}");
        }
        Quantity = StockQuantity.Create(Quantity.Value - amount);
    }

    /// <summary>在庫数を変更する。</summary>
    /// <exception cref="DomainException"><paramref name="newQuantity"/> が null の場合。</exception>
    public void ChangeQuantity(StockQuantity newQuantity)
        => Quantity = newQuantity ?? throw new DomainException("在庫数は必須です。");

    /// <summary>在庫切れ(下限)かどうか。</summary>
    public bool IsOutOfStock => Quantity.Value == StockQuantity.Min;

    /// <summary>在庫が上限に達しているかどうか。</summary>
    public bool IsFullCapacity => Quantity.Value == StockQuantity.Max;

    /// <summary>同一性(<see cref="StockId"/>)による等価判定。属性値ではなくIDが一致すれば等価とみなす。</summary>
    /// <param name="other">比較対象の在庫。</param>
    /// <returns>IDが一致すれば <c>true</c>。</returns>
    public bool Equals(Stock? other) => other is not null && StockId.Equals(other.StockId);

    /// <summary><see cref="object"/> 経由の等価判定。<see cref="StockId"/> を基準に比較する。</summary>
    /// <param name="obj">比較対象のオブジェクト。</param>
    /// <returns><see cref="Stock"/> であり、かつIDが一致すれば <c>true</c>。</returns>
    public override bool Equals(object? obj) => Equals(obj as Stock);

    /// <summary><see cref="StockId"/> に基づくハッシュ値を返す(等価性と整合させる)。</summary>
    /// <returns>IDのハッシュ値。</returns>
    public override int GetHashCode() => StockId.GetHashCode();

    /// <summary>デバッグ用の文字列表現(ID と在庫数)。</summary>
    /// <returns>在庫の内容を表す文字列。</returns>
    public override string ToString() => $"Stock{{id={StockId}, quantity={Quantity}}}";
}