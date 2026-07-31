using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Domain.Models.Products;

/// <summary>
/// 商品を表すエンティティ(集約ルート)。
/// </summary>
/// <remarks>
/// <para>同一性は <see cref="ProductId"/>。属性 <see cref="Name"/> / <see cref="Price"/> は不変・自己検証のVO。</para>
/// <para>集約として <see cref="Category"/>(参照)と <see cref="Stock"/>(内包)を保持する。</para>
/// </remarks>
public sealed class Product : IEquatable<Product>
{
    /// <summary>商品の同一性(不変)。</summary>
    public ProductId ProductId { get; }

    /// <summary>商品名(VO)。</summary>
    public ProductName Name { get; private set; }

    /// <summary>商品単価(VO)。</summary>
    public ProductPrice Price { get; private set; }

    /// <summary>商品カテゴリ(Entity)。骨格再構築時は一時的に null。</summary>
    public Category? Category { get; private set; }

    /// <summary>商品在庫(Entity)。骨格再構築時は一時的に null。</summary>
    public Stock? Stock { get; private set; }

    private Product(ProductId id, ProductName name, ProductPrice price, Category? category, Stock? stock)
    {
        if (id is null)
        {
            throw new DomainException("商品IDは必須です。");
        }
        if (name is null)
        {
            throw new DomainException("商品名は必須です。");
        }
        if (price is null)
        {
            throw new DomainException("商品単価は必須です。");
        }

        // 完全性チェック: Category と Stock は「両方null」か「両方非null」だけを許可する。
        var onlyOneProvided = (category is null) ^ (stock is null);
        if (onlyOneProvided)
        {
            throw new DomainException(
                "Productの再構築に失敗：CategoryとStockは両方指定するか、両方nullにしてください。");
        }

        ProductId = id;
        Name = name;
        Price = price;
        Category = category;
        Stock = stock;
    }

    /// <summary>新規作成(在庫は初期数量で新規採番)。</summary>
    public static Product CreateNew(ProductName name, ProductPrice price, Category category, StockQuantity quantity)
        => new(ProductId.New(), name, price, category, Stock.CreateNew(quantity));

    /// <summary>識別子を指定して完全な集約を再構築(リストア)する。</summary>
    public static Product Restore(ProductId id, ProductName name, ProductPrice price, Category category, Stock stock)
        => new(id, name, price, category, stock);

    /// <summary>骨格(Category/Stockなし)だけで再構築する。Assemblerで後から合成する用途。</summary>
    public static Product RestoreSkeleton(ProductId id, ProductName name, ProductPrice price)
        => new(id, name, price, null, null);

    /// <summary>カテゴリを設定する。</summary>
    /// <exception cref="DomainException"><paramref name="category"/> が null の場合。</exception>
    public void AttachCategory(Category category)
        => Category = category ?? throw new DomainException("商品カテゴリは必須です。");

    /// <summary>在庫を設定する。</summary>
    /// <exception cref="DomainException"><paramref name="stock"/> が null の場合。</exception>
    public void AttachStock(Stock stock)
        => Stock = stock ?? throw new DomainException("在庫は必須です。");

    /// <summary>商品名を変更する(妥当性はVOが自己検証済み。ここでは非nullのみ保証)。</summary>
    /// <exception cref="DomainException"><paramref name="newName"/> が null の場合。</exception>
    public void Rename(ProductName newName)
        => Name = newName ?? throw new DomainException("商品名は必須です。");

    /// <summary>単価を変更する(妥当性はVOが自己検証済み。ここでは非nullのみ保証)。</summary>
    /// <exception cref="DomainException"><paramref name="newPrice"/> が null の場合。</exception>
    public void Reprice(ProductPrice newPrice)
        => Price = newPrice ?? throw new DomainException("商品単価は必須です。");

    /// <summary>在庫数を変更する(在庫の同一性 StockId は保持したまま数量のみ書き換える)。</summary>
    /// <exception cref="DomainException">在庫が未設定(未 AttachStock)の場合。</exception>
    public void ChangeStock(StockQuantity newQty)
    {
        EnsureStockAttached();
        Stock!.ChangeQuantity(newQty);
    }

    /// <summary>現在の在庫数を返す。</summary>
    /// <exception cref="DomainException">在庫が未設定の場合。</exception>
    public StockQuantity CurrentStock()
    {
        EnsureStockAttached();
        return Stock!.Quantity;
    }

    private void EnsureStockAttached()
    {
        if (Stock is null)
        {
            throw new DomainException("在庫が未設定です。先に AttachStock(...) を呼び出してください。");
        }
    }

    /// <summary>同一性(<see cref="ProductId"/>)による等価判定。属性値ではなくIDが一致すれば等価とみなす。</summary>
    /// <param name="other">比較対象の商品。</param>
    /// <returns>IDが一致すれば <c>true</c>。</returns>
    public bool Equals(Product? other) => other is not null && ProductId.Equals(other.ProductId);

    /// <summary><see cref="object"/> 経由の等価判定。<see cref="ProductId"/> を基準に比較する。</summary>
    /// <param name="obj">比較対象のオブジェクト。</param>
    /// <returns><see cref="Product"/> であり、かつIDが一致すれば <c>true</c>。</returns>
    public override bool Equals(object? obj) => Equals(obj as Product);

    /// <summary><see cref="ProductId"/> に基づくハッシュ値を返す(等価性と整合させる)。</summary>
    /// <returns>IDのハッシュ値。</returns>
    public override int GetHashCode() => ProductId.GetHashCode();

    /// <summary>デバッグ用の文字列表現(ID・名称・単価・カテゴリ・在庫)。</summary>
    /// <returns>商品の内容を表す文字列。</returns>
    public override string ToString()
        => $"Product{{id={ProductId}, name={Name}, price={Price}, category={Category}, stock={Stock}}}";
}