using System.Data.Common;
using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Products;
using Ddd.Infrastructure.Exceptions;
using Ddd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ddd.Infrastructure.Products;

/// <summary>
/// <see cref="IProductRepository"/> の EF Core による実装。
/// </summary>
/// <remarks>
/// <para>
/// Product 集約(商品・カテゴリ・在庫)の永続化を担う。Entity ↔ 集約 の合成・分解は
/// <see cref="ProductAssembler"/> に委譲する。
/// </para>
/// <para>
/// ドメイン例外(<see cref="DomainException"/>)はそのまま伝播、キャンセルは伝播、
/// DB由来の技術的例外(<see cref="DbUpdateException"/> / <see cref="DbException"/>)と予期しない例外は
/// <see cref="InternalException"/> にラップする。トランザクション境界はユースケース層が管理する。
/// </para>
/// </remarks>
public sealed class ProductRepository(
    AppDbContext dbContext,
    ProductAssembler assembler) : IProductRepository
{
    /// <inheritdoc />
    public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (product is null)
        {
            throw new DomainException("商品は必須です。");
        }
        try
        {
            // カテゴリUUID → カテゴリの内部PK(int)を解決する
            var categoryUuid = assembler.ExtractCategoryUuid(product);
            var categoryPk = await dbContext.ProductCategories
                .Where(c => c.CategoryUuid == categoryUuid)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (categoryPk is null)
            {
                throw new DomainException("指定された商品カテゴリが存在しません。");
            }

            // 集約 → Entity(外部キーは未設定)
            var productEntity = assembler.ToProductEntity(product);
            var stockEntity = assembler.ToStockEntity(product);

            // product に category_id を補完して INSERT(採番されたPKを取得)
            productEntity.CategoryId = categoryPk.Value;
            dbContext.Products.Add(productEntity);
            await dbContext.SaveChangesAsync(cancellationToken);

            // stock に product_id を補完して INSERT
            stockEntity.ProductId = productEntity.Id;
            dbContext.ProductStocks.Add(stockEntity);
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
            // 集約 → Entity(UUIDで対象を特定、変更後の名称・単価・在庫数を保持)
            var productEntity = assembler.ToProductEntity(product);
            var stockEntity = assembler.ToStockEntity(product);

            // 商品を product_uuid で特定し、名称・単価を UPDATE(カテゴリは変更対象外)
            var updated = await dbContext.Products
                .Where(p => p.ProductUuid == productEntity.ProductUuid)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Name, productEntity.Name)
                    .SetProperty(p => p.Price, productEntity.Price),
                    cancellationToken);
            if (updated == 0)
            {
                // 事前に FindById で存在確認済みのため、到達するのは想定外(並行削除など)
                throw new InternalException("更新対象の商品が見つかりませんでした。");
            }

            // 在庫を stock_uuid で特定し、在庫数を UPDATE
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
            // product・product_stock・product_category を結合して1行取得する
            var row = await (
                from p in dbContext.Products.AsNoTracking()
                join s in dbContext.ProductStocks.AsNoTracking() on p.Id equals s.ProductId
                join c in dbContext.ProductCategories.AsNoTracking() on p.CategoryId equals c.Id
                where p.ProductUuid == productId.Value
                select new { Product = p, Stock = s, Category = c })
                .FirstOrDefaultAsync(cancellationToken);

            return row is null ? null : assembler.Assemble(row.Product, row.Category, row.Stock);
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
            var row = await (
                from p in dbContext.Products.AsNoTracking()
                join s in dbContext.ProductStocks.AsNoTracking() on p.Id equals s.ProductId
                join c in dbContext.ProductCategories.AsNoTracking() on p.CategoryId equals c.Id
                where p.Name == productName.Value
                select new { Product = p, Stock = s, Category = c })
                .FirstOrDefaultAsync(cancellationToken);

            return row is null ? null : assembler.Assemble(row.Product, row.Category, row.Stock);
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