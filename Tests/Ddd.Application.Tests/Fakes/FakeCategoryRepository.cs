using Ddd.Domain.Models.Categories;

namespace Ddd.Application.Tests.Fakes;

/// <summary>
/// テスト用のインメモリ <see cref="ICategoryRepository"/>。<see cref="Seed"/> で事前データを投入する。
/// </summary>
public sealed class FakeCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories = new();

    /// <summary>
    /// テストデータを投入する。
    /// </summary>
    public void Seed(params Category[] categories) => _categories.AddRange(categories);

    public Task<Category?> FindByIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
        => Task.FromResult(_categories.FirstOrDefault(c => c.CategoryId.Equals(categoryId)));

    public Task<IReadOnlyList<Category>> FindAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Category>>(_categories.ToList());
}