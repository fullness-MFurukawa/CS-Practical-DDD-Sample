using Ddd.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ddd.Infrastructure.Persistence;

/// <summary>
/// 商品管理のデータアクセス手段となる EF Core の <see cref="DbContext"/>。
/// </summary>
/// <remarks>
/// <para>
/// jOOQ の <c>DSLContext</c>、MyBatis の <c>SqlMapper</c>、Spring Data JPA の <c>JpaRepository</c> に
/// 相当する「データアクセスの窓口」。各 Repository はこの <see cref="AppDbContext"/> を注入して
/// 永続化を実装する。
/// </para>
/// <para>
/// テーブル名・主キー・列名・長さといった基本マッピングは各エンティティの属性で表し、
/// 一意インデックスとリレーションシップ(外部キー制約)は本クラスの
/// <see cref="OnModelCreating(ModelBuilder)"/> で構成する。エンティティにはナビゲーション
/// プロパティを持たせず、外部キーはスカラー列として扱う(結合は Repository で明示的に行う)。
/// </para>
/// </remarks>
public class AppDbContext : DbContext
{
    /// <summary>DI から <see cref="DbContextOptions{TContext}"/> を受け取って生成する。</summary>
    /// <param name="options">接続文字列やプロバイダ(Npgsql)を含むオプション。</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary><c>product</c> テーブル。</summary>
    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    /// <summary><c>product_category</c> テーブル。</summary>
    public DbSet<ProductCategoryEntity> ProductCategories => Set<ProductCategoryEntity>();

    /// <summary><c>product_stock</c> テーブル。</summary>
    public DbSet<ProductStockEntity> ProductStocks => Set<ProductStockEntity>();

    /// <summary>
    /// モデルの追加構成。属性で表現しない「一意制約」と「外部キー(結合)」を定義する。
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー。</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- 一意インデックス: ドメイン識別子(UUID列)は一意であること ---
        modelBuilder.Entity<ProductEntity>()
            .HasIndex(p => p.ProductUuid)
            .IsUnique();

        modelBuilder.Entity<ProductCategoryEntity>()
            .HasIndex(c => c.CategoryUuid)
            .IsUnique();

        modelBuilder.Entity<ProductStockEntity>()
            .HasIndex(s => s.StockUuid)
            .IsUnique();

        // --- 外部キー: product.category_id → product_category.id ---
        // ナビゲーションプロパティを持たないため、型引数と HasForeignKey で関係を明示する。
        // カテゴリは参照(マスタ)のため、削除は制限(Restrict)する。
        modelBuilder.Entity<ProductEntity>()
            .HasOne<ProductCategoryEntity>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- 外部キー: product_stock.product_id → product.id ---
        // 在庫は Product 集約の一部のため、商品削除時は在庫も削除(Cascade)する。
        modelBuilder.Entity<ProductStockEntity>()
            .HasOne<ProductEntity>()
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}