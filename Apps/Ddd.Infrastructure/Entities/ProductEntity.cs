using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ddd.Infrastructure.Entities;

/// <summary>
/// <c>product</c> テーブルに対応する永続化エンティティ(EF Core の受け皿)。
/// </summary>
/// <remarks>
/// <para>
/// ドメインの集約ルート <c>Product</c> とは分離した POCO。相互変換は腐敗防止層
/// (<c>ProductEntityMapper</c>)が担い、カテゴリ・在庫の合成は <c>ProductAssembler</c> が行う。
/// </para>
/// <para>
/// 外部キー <c>category_id</c> はスカラー列として保持し、リレーションシップ(FK制約)は
/// <c>AppDbContext.OnModelCreating</c> で構成する(ナビゲーションプロパティは持たせない)。
/// </para>
/// </remarks>
[Table("product")]
public class ProductEntity
{
    /// <summary>内部PK(自動採番)。DB上の識別子。</summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>ドメイン識別子(商品UUID)。コード上の識別に用いる。</summary>
    [Column("product_uuid")]
    public Guid ProductUuid { get; set; }

    /// <summary>商品名(最大30文字)。</summary>
    [Column("name")]
    [MaxLength(30)]
    public string Name { get; set; } = string.Empty;

    /// <summary>単価。</summary>
    [Column("price")]
    public int Price { get; set; }

    /// <summary>所属するカテゴリの内部PK(外部キー <c>category_id</c>)。</summary>
    [Column("category_id")]
    public int CategoryId { get; set; }
}