using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ddd.Infrastructure.Entities;

/// <summary>
/// <c>product_stock</c> テーブルに対応する永続化エンティティ(EF Core の受け皿)。
/// </summary>
/// <remarks>
/// <para>
/// ドメインの <c>Stock</c> とは分離した POCO。相互変換は腐敗防止層
/// (<c>ProductStockEntityMapper</c>)が担う。在庫は Product 集約の一部として
/// <c>ProductRepository</c> の中で永続化される。
/// </para>
/// <para>
/// 外部キー <c>product_id</c> はスカラー列として保持し、リレーションシップ(FK制約)は
/// <c>AppDbContext.OnModelCreating</c> で構成する(ナビゲーションプロパティは持たせない)。
/// </para>
/// </remarks>
[Table("product_stock")]
public class ProductStockEntity
{
    /// <summary>内部PK(自動採番)。DB上の識別子。</summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>ドメイン識別子(在庫UUID)。コード上の識別に用いる。</summary>
    [Column("stock_uuid")]
    public Guid StockUuid { get; set; }

    /// <summary>在庫数(物理列名は <c>stock</c>)。</summary>
    [Column("stock")]
    public int Quantity { get; set; }

    /// <summary>所属する商品の内部PK(外部キー <c>product_id</c>)。</summary>
    [Column("product_id")]
    public int ProductId { get; set; }
}