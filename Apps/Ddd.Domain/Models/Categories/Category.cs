using Ddd.Domain.Exceptions;

namespace Ddd.Domain.Models.Categories;

/// <summary>
/// 商品カテゴリを表すエンティティ。
/// </summary>
/// <remarks>
/// 同一性は <see cref="CategoryId"/> で判定する(属性が変わっても同じ存在とみなす)。
/// </remarks>
public sealed class Category : IEquatable<Category>
{
    /// <summary>カテゴリの同一性(不変)。</summary>
    public CategoryId CategoryId { get; }

    /// <summary>カテゴリ名(VO)。</summary>
    public CategoryName Name { get; private set; }

    private Category(CategoryId id, CategoryName name)
    {
        CategoryId = id ?? throw new DomainException("カテゴリIDは必須です。");
        Name = name ?? throw new DomainException("カテゴリ名は必須です。");
    }

    /// <summary>新規作成。</summary>
    public static Category CreateNew(CategoryName name) => new(CategoryId.New(), name);

    /// <summary>識別子を指定して再構築(リストア)する。</summary>
    public static Category Restore(CategoryId id, CategoryName name) => new(id, name);

    /// <summary>カテゴリ名を変更する。</summary>
    /// <exception cref="DomainException"><paramref name="newName"/> が null の場合。</exception>
    public void Rename(CategoryName newName)
        => Name = newName ?? throw new DomainException("カテゴリ名は必須です。");

    /// <summary>同一性(<see cref="CategoryId"/>)による等価判定。属性値ではなくIDが一致すれば等価とみなす。</summary>
    /// <param name="other">比較対象のカテゴリ。</param>
    /// <returns>IDが一致すれば <c>true</c>。</returns>
    public bool Equals(Category? other) => other is not null && CategoryId.Equals(other.CategoryId);

    /// <summary><see cref="object"/> 経由の等価判定。<see cref="CategoryId"/> を基準に比較する。</summary>
    /// <param name="obj">比較対象のオブジェクト。</param>
    /// <returns><see cref="Category"/> であり、かつIDが一致すれば <c>true</c>。</returns>
    public override bool Equals(object? obj) => Equals(obj as Category);

    /// <summary><see cref="CategoryId"/> に基づくハッシュ値を返す(等価性と整合させる)。</summary>
    /// <returns>IDのハッシュ値。</returns>
    public override int GetHashCode() => CategoryId.GetHashCode();

    /// <summary>デバッグ用の文字列表現(ID と名称)。</summary>
    /// <returns>カテゴリの内容を表す文字列。</returns>
    public override string ToString() => $"Category{{id={CategoryId}, name={Name}}}";
}