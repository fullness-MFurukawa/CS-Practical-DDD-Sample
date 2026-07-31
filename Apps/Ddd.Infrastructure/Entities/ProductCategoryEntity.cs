using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ddd.Infrastructure.Entities;

/// <summary>
/// <c>product_category</c> テーブルに対応する永続化エンティティ(EF Core の受け皿)。
/// </summary>
/// <remarks>
/// <para>
/// ドメインの <c>Category</c> とは分離し、DB構造をそのまま表す POCO。ドメインとの相互変換は
/// 腐敗防止層(<c>ProductCategoryEntityMapper</c>)が担う。
/// </para>
/// <para>
/// テーブル名・主キー・列名・型・長さといった基本的なマッピングは本クラスの属性で表し、
/// 一意制約やリレーションシップ(結合)は <c>AppDbContext.OnModelCreating</c> で構成する。
/// </para>
/// </remarks>
[Table("product_category")]
public class ProductCategoryEntity
{
    /// <summary>内部PK(自動採番)。DB上の識別子。整数主キーは規約により自動でストア生成される。</summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>ドメイン識別子(カテゴリUUID)。コード上の識別に用いる。</summary>
    [Column("category_uuid")]
    public Guid CategoryUuid { get; set; }

    /// <summary>カテゴリ名(最大20文字)。</summary>
    [Column("name")]
    [MaxLength(20)]
    public string Name { get; set; } = string.Empty;
}