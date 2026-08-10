using System.ComponentModel.DataAnnotations;

namespace Ddd.Api.Products.Schemas;

/// <summary>
/// 商品変更リクエスト受信用スキーマ(入力DTO)。
/// </summary>
/// <remarks>
/// <para>
/// 「商品を変更する」ユースケースで、変更対象の名称・単価・在庫数を受け取り、DataAnnotations で
/// 入力値の妥当性を境界で検証する。ドメイン層の知識(値オブジェクト等)は露出しない。
/// </para>
/// <para>
/// 商品IDは URI のパス <c>/api/products/{id}</c> で受け取るため本スキーマには含めない。
/// カテゴリは変更対象外のため受け取らない。検証属性は record のコンストラクタ引数に付与する。
/// </para>
/// </remarks>
/// <param name="Name">商品名(必須・30文字以内)。</param>
/// <param name="Price">商品単価(円): 50〜10000(必須)。</param>
/// <param name="StockQuantity">在庫数: 0〜100(必須)。</param>
public record ProductUpdateSchema(
    [Required(ErrorMessage = "商品名は必須です")]
    [StringLength(30, ErrorMessage = "商品名は30文字以内で指定してください")]
    string Name,

    [Required(ErrorMessage = "単価は必須です")]
    [Range(50, 10000, ErrorMessage = "単価は50以上10000以下で指定してください")]
    int? Price,

    [Required(ErrorMessage = "在庫数は必須です")]
    [Range(0, 100, ErrorMessage = "在庫数は0以上100以下で指定してください")]
    int? StockQuantity);