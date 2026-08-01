using Ddd.Domain.Exceptions;
using Ddd.Domain.Mappers;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Entities;

namespace Ddd.Infrastructure.Products;

/// <summary>
/// Product 集約の「合成(Entity → 集約)」と「分解(集約 → Entity)」を担うアセンブラ(Factory)。
/// </summary>
/// <remarks>
/// 責務は<b>型変換と合成/分解のみ</b>で、永続化(SQL 実行)は Repository が担う。
/// Entity ↔ ドメイン の個別変換は各 Mapper(インターフェイス経由)に委譲する。
/// </remarks>
public sealed class ProductAssembler(
    IDomainBiMapper<ProductEntity, Product> productMapper,
    IToDomainMapper<ProductCategoryEntity, Category> categoryMapper,
    IDomainBiMapper<ProductStockEntity, Stock> stockMapper)
{
    // ----------------------------------------------------------------------
    // 合成(Entity → 集約)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 3種の永続化エンティティから完全な <see cref="Product"/> 集約を合成(再構築)する。
    /// </summary>
    /// <param name="product">商品行(product_uuid / name / price)。</param>
    /// <param name="category">カテゴリ行(category_uuid / name)。</param>
    /// <param name="stock">在庫行(stock_uuid / stock)。</param>
    /// <returns>合成済みの <see cref="Product"/> 集約。</returns>
    /// <exception cref="DomainException">いずれかが null、または必須項目が不正な場合。</exception>
    public Product Assemble(ProductEntity product, ProductCategoryEntity category, ProductStockEntity stock)
    {
        if (product is null)
        {
            throw new DomainException("ProductEntity が null です。");
        }
        if (category is null)
        {
            throw new DomainException("ProductCategoryEntity が null です。");
        }
        if (stock is null)
        {
            throw new DomainException("ProductStockEntity が null です。");
        }

        // 骨格を復元し、カテゴリ・在庫を後から合成する
        var aggregate = productMapper.ToDomain(product); // skeleton
        aggregate.AttachCategory(categoryMapper.ToDomain(category));
        aggregate.AttachStock(stockMapper.ToDomain(stock));
        return aggregate;
    }

    // ----------------------------------------------------------------------
    // 分解(集約 → Entity)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 集約から <see cref="ProductEntity"/> を作る(INSERT/UPDATE 用)。
    /// </summary>
    /// <remarks>外部キー <c>category_id</c> はここでは設定しない。Repository で補完する。</remarks>
    public ProductEntity ToProductEntity(Product product)
    {
        if (product is null)
        {
            throw new DomainException("Product が null です。");
        }
        return productMapper.FromDomain(product);
    }

    /// <summary>
    /// 集約から <see cref="ProductStockEntity"/> を作る(INSERT/UPDATE 用)。
    /// </summary>
    /// <remarks>外部キー <c>product_id</c> はここでは設定しない。Repository で補完する。</remarks>
    public ProductStockEntity ToStockEntity(Product product)
    {
        if (product is null)
        {
            throw new DomainException("Product が null です。");
        }
        var stock = product.Stock;
        if (stock is null)
        {
            throw new DomainException("Product に Stock が設定されていません。");
        }
        return stockMapper.FromDomain(stock);
    }

    /// <summary>
    /// 集約からカテゴリの UUID を取り出すユーティリティ。
    /// Repository で外部キー <c>category_id</c> を解決するために利用する。
    /// </summary>
    public Guid ExtractCategoryUuid(Product product)
    {
        if (product is null)
        {
            throw new DomainException("Product が null です。");
        }
        var category = product.Category;
        if (category is null)
        {
            throw new DomainException("Product に Category が設定されていません。");
        }
        return category.CategoryId.Value;
    }
}