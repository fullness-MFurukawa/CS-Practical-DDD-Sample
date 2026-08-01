using Ddd.Domain.Exceptions;
using Ddd.Domain.Mappers;
using Ddd.Domain.Models.Stocks;
using Ddd.Infrastructure.Entities;
using Riok.Mapperly.Abstractions;

namespace Ddd.Infrastructure.Products;

/// <summary>
/// 永続化エンティティ <see cref="ProductStockEntity"/> とドメインエンティティ <see cref="Stock"/> を
/// 相互変換する腐敗防止層(ACL)の Mapper。
/// </summary>
/// <remarks>
/// <para>
/// <b>ToDomain(Entity → ドメイン)</b>: 在庫行から <see cref="Stock"/> を復元する。VO のファクトリ＋検証が
/// 必要なため手書きで実装する。
/// </para>
/// <para>
/// <b>FromDomain(ドメイン → Entity)</b>: <c>stock_uuid</c> / <c>stock</c> のみを設定する(Mapperly 生成)。
/// 主キー <c>Id</c> と外部キー <c>ProductId</c> はここでは設定せず、<c>ProductId</c> は Repository で
/// 補完する。在庫は Product 集約の一部として <c>ProductRepository</c> の中で永続化される。
/// </para>
/// </remarks>
[Mapper]
public sealed partial class ProductStockEntityMapper : IDomainBiMapper<ProductStockEntity, Stock>
{
    /// <summary>
    /// <see cref="ProductStockEntity"/> を検証し、<see cref="Stock"/> を復元する。
    /// </summary>
    /// <param name="input">DBから取得した在庫行。</param>
    /// <returns>再構築された <see cref="Stock"/>。</returns>
    /// <exception cref="DomainException">入力が null、または UUID が不正な場合。</exception>
    public Stock ToDomain(ProductStockEntity input)
    {
        if (input is null)
        {
            throw new DomainException("在庫情報が取得できません。");
        }
        if (input.StockUuid == Guid.Empty)
        {
            throw new DomainException("在庫UUIDが不正です。");
        }

        // 在庫数(Quantity)は int のため null チェックは不要。値の妥当性は StockQuantity.Create が検証する。
        return Stock.Restore(
            StockId.From(input.StockUuid),
            StockQuantity.Create(input.Quantity));
    }

    /// <summary>
    /// <see cref="Stock"/> を保存用の <see cref="ProductStockEntity"/> に変換する(Mapperly 生成)。
    /// </summary>
    /// <remarks>主キー <c>Id</c> と外部キー <c>ProductId</c> は設定しない(Repository で補完する)。</remarks>
    /// <param name="domain">ドメインの在庫エンティティ。</param>
    /// <returns><c>stock_uuid</c> / <c>stock</c> を設定した永続化エンティティ。</returns>
    [MapperIgnoreSource(nameof(Stock.IsOutOfStock))]
    [MapperIgnoreSource(nameof(Stock.IsFullCapacity))]
    [MapperIgnoreTarget(nameof(ProductStockEntity.Id))]
    [MapperIgnoreTarget(nameof(ProductStockEntity.ProductId))]
    [MapProperty(nameof(Stock.StockId), nameof(ProductStockEntity.StockUuid))]
    public partial ProductStockEntity FromDomain(Stock domain);

    // ---- Mapperly が利用する VO → プリミティブ の変換子 ----

    /// <summary>在庫識別子(VO)から uuid 列用の <see cref="Guid"/> を取り出す。</summary>
    private static Guid MapStockId(StockId id) => id.Value;

    /// <summary>在庫数(VO)から整数を取り出す。</summary>
    private static int MapStockQuantity(StockQuantity quantity) => quantity.Value;
}