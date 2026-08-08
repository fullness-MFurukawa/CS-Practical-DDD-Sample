using System.Data.Common;
using Ddd.Domain.Adapters;
using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Categories;
using Ddd.Infrastructure.Entities;
using Ddd.Infrastructure.Exceptions;
using Ddd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ddd.Infrastructure.Categories;

/// <summary>
/// <see cref="ICategoryRepository"/> の EF Core による実装。
/// </summary>
/// <remarks>
/// <para>
/// カテゴリの取得(FindById / FindAll)を担う。Entity → <see cref="Category"/> の変換は
/// <see cref="IToDomainAdapter{TDto,TDomain}"/>(<c>ProductCategoryEntityAdapter</c>)に委譲する。
/// 読み取りのみのため合成用の Factory は不要。
/// </para>
/// <para>
/// ドメイン例外(<see cref="DomainException"/>)はそのまま伝播させ、データベース由来の技術的例外は
/// <see cref="InternalException"/> にラップして上位へスローする。キャンセルは伝播させる。
/// </para>
/// </remarks>
public sealed class CategoryRepository(
    AppDbContext dbContext,
    IToDomainAdapter<ProductCategoryEntity, Category> adapter) : ICategoryRepository
{
    /// <inheritdoc />
    public async Task<Category?> FindByIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        if (categoryId is null)
        {
            throw new DomainException("商品カテゴリIdは必須です。");
        }
        try
        {
            // category_uuid を Guid のまま比較する。読み取りのため追跡は不要。
            var entity = await dbContext.ProductCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryUuid == categoryId.Value, cancellationToken);

            return entity is null ? null : adapter.ToDomain(entity);
        }
        catch (DomainException)
        {
            throw; // ドメイン例外はそのまま伝播させる
        }
        catch (OperationCanceledException)
        {
            throw; // キャンセルはラップせず伝播させる
        }
        catch (DbException ex)
        {
            throw new InternalException("カテゴリ情報の取得中にデータベースエラーが発生しました。", ex);
        }
        catch (Exception ex)
        {
            throw new InternalException("カテゴリ情報の取得処理中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Category>> FindAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await dbContext.ProductCategories
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync(cancellationToken);

            return entities.Select(adapter.ToDomain).ToList();
        }
        catch (DomainException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbException ex)
        {
            throw new InternalException("カテゴリ一覧の取得中にデータベースエラーが発生しました。", ex);
        }
        catch (Exception ex)
        {
            throw new InternalException("カテゴリ一覧の取得処理中に予期しないエラーが発生しました。", ex);
        }
    }
}