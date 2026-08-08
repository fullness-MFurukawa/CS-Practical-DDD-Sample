using Ddd.Application.Exceptions;
using Ddd.Domain.Models.Categories;

namespace Ddd.Application.Categories.Services;

/// <summary>
/// <see cref="ICategoryService"/> の実装クラス。
/// </summary>
/// <remarks>
/// リポジトリを介して永続層からドメインエンティティを取得し、必要に応じてアプリケーション例外に変換する。
/// ドメインの整合性検証やトランザクションは行わない(ユースケース層で管理される)。
/// </remarks>
/// <param name="categoryRepository">カテゴリのリポジトリ(ドメインのポート)。</param>
public sealed class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    /// <inheritdoc />
    public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => categoryRepository.FindAllAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Category> GetCategoryByIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.FindByIdAsync(categoryId, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException($"商品カテゴリId:[{categoryId.Value}]の商品カテゴリは存在しません。");
        }
        return category;
    }
}