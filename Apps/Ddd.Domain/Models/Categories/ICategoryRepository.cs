namespace Ddd.Domain.Models.Categories;

/// <summary>
/// ドメインリポジトリ: カテゴリ <see cref="Category"/> の取得を担う契約(ポート)。
/// </summary>
/// <remarks>カテゴリに対する要件は問合せのみ。インフラ層でこの契約を実装する。</remarks>
public interface ICategoryRepository
{
    /// <summary>カテゴリIDを指定してカテゴリを取得する。存在しない場合は null。</summary>
    /// <param name="categoryId">取得対象のカテゴリID。</param>
    /// <param name="cancellationToken">キャンセル用トークン。</param>
    /// <returns>該当する <see cref="Category"/>。存在しない場合は <c>null</c>。</returns>
    Task<Category?> FindByIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default);

    /// <summary>すべてのカテゴリを取得する。</summary>
    /// <param name="cancellationToken">キャンセル用トークン。</param>
    /// <returns>カテゴリの読み取り専用リスト(該当なしの場合は空)。</returns>
    Task<IReadOnlyList<Category>> FindAllAsync(CancellationToken cancellationToken = default);
}