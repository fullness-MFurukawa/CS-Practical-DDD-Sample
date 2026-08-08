using Ddd.Domain.Adapters;
using Ddd.Domain.Exceptions;
using Ddd.Domain.Models.Products;
using Ddd.Infrastructure.Entities;
using Riok.Mapperly.Abstractions;

namespace Ddd.Infrastructure.Products;

/// <summary>
/// 永続化エンティティ <see cref="ProductEntity"/> とドメインの集約ルート <see cref="Product"/> を
/// 相互変換する腐敗防止層(ACL)のアダプタ。
/// </summary>
/// <remarks>
/// <para>
/// <b>ToDomain(Entity → ドメイン)</b>: 商品テーブル単体の行から、カテゴリ・在庫を伴わない
/// 「骨格」の <see cref="Product"/> を復元する(<c>RestoreSkeleton</c>)。カテゴリ・在庫は別アダプタで
/// 変換し、<c>ProductFactory</c> で合成する。VO のファクトリ＋検証が必要なため手書きで実装する。
/// </para>
/// <para>
/// <b>FromDomain(ドメイン → Entity)</b>: <c>product_uuid</c> / <c>name</c> / <c>price</c> のみを設定する。
/// 単純な値の詰め替えのため Mapperly に生成させる。主キー <c>Id</c> と外部キー <c>CategoryId</c> は
/// ここでは設定せず、<c>CategoryId</c> は Repository で解決・補完する(未補完で INSERT すると NOT NULL 違反)。
/// </para>
/// </remarks>
[Mapper]
public sealed partial class ProductEntityAdapter : IDomainBiAdapter<ProductEntity, Product>
{
    /// <summary>
    /// <see cref="ProductEntity"/> を検証し、カテゴリ・在庫を伴わない骨格の <see cref="Product"/> を復元する。
    /// </summary>
    /// <param name="input">DBから取得した商品行。</param>
    /// <returns>骨格の <see cref="Product"/>(カテゴリ・在庫は未設定)。</returns>
    /// <exception cref="DomainException">入力が null、または UUID・名称が不正な場合。</exception>
    public Product ToDomain(ProductEntity input)
    {
        if (input is null)
        {
            throw new DomainException("商品情報が取得できません。");
        }
        if (input.ProductUuid == Guid.Empty)
        {
            throw new DomainException("商品UUIDが不正です。");
        }
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new DomainException("商品名が未設定です。");
        }

        // カテゴリ・在庫は別アダプタで変換し、後段(ProductFactory)で合成する。
        // 単価(price)は int のため null チェックは不要。値の妥当性は ProductPrice.Create が検証する。
        return Product.RestoreSkeleton(
            ProductId.From(input.ProductUuid),
            ProductName.Create(input.Name),
            ProductPrice.Create(input.Price));
    }

    /// <summary>
    /// <see cref="Product"/> を保存用の <see cref="ProductEntity"/> に変換する(Mapperly 生成)。
    /// </summary>
    /// <remarks>主キー <c>Id</c> と外部キー <c>CategoryId</c> は設定しない(Repository で補完する)。</remarks>
    /// <param name="domain">ドメインの集約ルート。</param>
    /// <returns><c>product_uuid</c> / <c>name</c> / <c>price</c> を設定した永続化エンティティ。</returns>
    [MapperIgnoreSource(nameof(Product.Category))]
    [MapperIgnoreSource(nameof(Product.Stock))]
    [MapperIgnoreTarget(nameof(ProductEntity.Id))]
    [MapperIgnoreTarget(nameof(ProductEntity.CategoryId))]
    [MapProperty(nameof(Product.ProductId), nameof(ProductEntity.ProductUuid))]
    [MapperIgnoreTarget(nameof(ProductEntity.Category))]   
    [MapperIgnoreTarget(nameof(ProductEntity.Stock))]      
    public partial ProductEntity FromDomain(Product domain);

    // ---- Mapperly が利用する VO → プリミティブ の変換子 ----

    /// <summary>
    /// 商品識別子(VO)から uuid 列用の <see cref="Guid"/> を取り出す。
    /// </summary>
    private static Guid MapProductId(ProductId id) => id.Value;

    /// <summary>商品名(VO)から文字列を取り出す。</summary>
    private static string MapProductName(ProductName name) => name.Value;

    /// <summary>単価(VO)から整数を取り出す。</summary>
    private static int MapProductPrice(ProductPrice price) => price.Value;
}