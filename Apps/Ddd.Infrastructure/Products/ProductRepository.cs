using System.Data.Common;
using Ddd.Domain.Exceptions;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Products;
using Ddd.Infrastructure.Entities;
using Ddd.Infrastructure.Exceptions;
using Ddd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ddd.Infrastructure.Products;

/// <summary>
/// <see cref="IProductRepository"/> の EF Core による実装。
/// </summary>
/// <remarks>
/// <para>
/// Product 集約(商品・カテゴリ・在庫)の永続化を担う。受け皿(EF エンティティ) ↔ 集約 の合成・分解は
/// 汎用ファクトリ <see cref="IFactory{TAggregate, TExternal}"/>(実装は <c>ProductFactory</c>)に委譲する。
/// 読み取りは <c>Include</c> でカテゴリ・在庫を一括ロードし、書き込みは EF Core の
/// リレーションシップ修復に任せる(在庫は所有ナビゲーションとして商品と同一 <c>SaveChanges</c> で永続化)。
/// </para>
/// <para>
/// ドメイン例外(<see cref="DomainException"/>)はそのまま伝播、キャンセルは伝播、
/// DB由来の技術的例外(<see cref="DbUpdateException"/> / <see cref="DbException"/>)と予期しない例外は
/// <see cref="InternalException"/> にラップする。トランザクション境界はユースケース層が管理する。
/// </para>
/// </remarks>
public sealed class ProductRepository(
    AppDbContext dbContext,
    IFactory<Product, ProductEntity> factory) : IProductRepository
{
    /// <inheritdoc />
    public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (product is null)
        {
            throw new DomainException("商品は必須です。");
        }
        if (product.Category is null)
        {
            throw new DomainException("商品にカテゴリが設定されていません。");
        }
        try
        {
            // カテゴリUUID → カテゴリの内部PK(int)を解決する(カテゴリは別集約のマスタ)。
            var categoryUuid = product.Category.CategoryId.Value;
            var categoryPk = await dbContext.ProductCategories
                .Where(c => c.CategoryUuid == categoryUuid)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (categoryPk is null)
            {
                throw new DomainException("指定された商品カテゴリが存在しません。");
            }

            // 集約 → 受け皿(在庫は所有ナビゲーションとしてネストされる)。
            var productEntity = factory.Disassemble(product);
            productEntity.CategoryId = categoryPk.Value; // 参照する既存カテゴリの外部キーを補完

            // 商品と在庫を同一トランザクションで INSERT。
            // 在庫の product_id は EF Core のリレーションシップ修復で自動補完される。
            dbContext.Products.Add(productEntity);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DomainException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            throw new InternalException("商品登録中にデータベースエラーが発生しました。", ex);
        }
        catch (DbException ex)
        {
            throw new InternalException("商品登録中にデータベースエラーが発生しました。", ex);
        }
        catch (Exception ex)
        {
            throw new InternalException("商品登録処理中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (product is null)
        {
            throw new DomainException("商品は必須です。");
        }
        try
        {
            // 集約 → 受け皿(UUIDで対象を特定、変更後の名称・単価・在庫数を保持)。
            var productEntity = factory.Disassemble(product);
            var stockEntity = productEntity.Stock!; // Disassemble が在庫の設定を保証する

            // 商品を product_uuid で特定し、名称・単価を UPDATE(カテゴリは変更対象外)。
            var updated = await dbContext.Products
                .Where(p => p.ProductUuid == productEntity.ProductUuid)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Name, productEntity.Name)
                    .SetProperty(p => p.Price, productEntity.Price),
                    cancellationToken);
            if (updated == 0)
            {
                // 事前に FindById で存在確認済みのため、到達するのは想定外(並行削除など)。
                throw new InternalException("更新対象の商品が見つかりませんでした。");
            }

            // 在庫を stock_uuid で特定し、在庫数を UPDATE。
            await dbContext.ProductStocks
                .Where(s => s.StockUuid == stockEntity.StockUuid)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.Quantity, stockEntity.Quantity),
                    cancellationToken);
        }
        catch (DomainException)
        {
            throw;
        }
        catch (InternalException)
        {
            throw; // 自前で投げた InternalException を二重ラップしない
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            throw new InternalException("商品変更中にデータベースエラーが発生しました。", ex);
        }
        catch (DbException ex)
        {
            throw new InternalException("商品変更中にデータベースエラーが発生しました。", ex);
        }
        catch (Exception ex)
        {
            throw new InternalException("商品変更処理中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(ProductName productName, CancellationToken cancellationToken = default)
    {
        if (productName is null)
        {
            throw new DomainException("商品名は必須です。");
        }
        try
        {
            return await dbContext.Products
                .AsNoTracking()
                .AnyAsync(p => p.Name == productName.Value, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbException ex)
        {
            throw new InternalException("商品名の存在確認中にデータベースエラーが発生しました。", ex);
        }
        catch (Exception ex)
        {
            throw new InternalException("商品名の存在確認処理中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Product?> FindByIdAsync(ProductId productId, CancellationToken cancellationToken = default)
    {
        if (productId is null)
        {
            throw new DomainException("商品Idは必須です。");
        }
        try
        {
            // カテゴリ・在庫を Include して集約ルート1件を取得する。
            var entity = await dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .FirstOrDefaultAsync(p => p.ProductUuid == productId.Value, cancellationToken);

            return entity is null ? null : factory.Assemble(entity);
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
            throw new InternalException("商品情報の取得中にデータベースエラーが発生しました。", ex);
        }
        catch (Exception ex)
        {
            throw new InternalException("商品情報の取得処理中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Product?> FindByNameAsync(ProductName productName, CancellationToken cancellationToken = default)
    {
        if (productName is null)
        {
            throw new DomainException("商品名は必須です。");
        }
        try
        {
            var entity = await dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .FirstOrDefaultAsync(p => p.Name == productName.Value, cancellationToken);

            return entity is null ? null : factory.Assemble(entity);
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
            throw new InternalException("商品名による検索中にデータベースエラーが発生しました。", ex);
        }
        catch (Exception ex)
        {
            throw new InternalException("商品名による検索処理中に予期しないエラーが発生しました。", ex);
        }
    }
}