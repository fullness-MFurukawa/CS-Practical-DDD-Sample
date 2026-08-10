using System.ComponentModel.DataAnnotations;

namespace Ddd.Api.Products.Schemas;

/// <summary>
/// 商品登録リクエスト受信用スキーマ(入力DTO)。
/// </summary>
/// <remarks>
/// <para>
/// プレゼンテーション層で使用する入力専用のデータ転送オブジェクト。API クライアントから送信される
/// JSON を受け取り、DataAnnotations で入力値の妥当性を境界で検証する。ドメイン層の知識
/// (値オブジェクト等)は露出しない。
/// </para>
/// <para>
/// <c>[ApiController]</c> により自動検証され、検証エラーは 400(ProblemDetails)として返る。
/// 受け取った値は <see cref="ProductCreateSchemaAdapter"/> でアプリケーション層の
/// <see cref="Ddd.Application.Dtos.ProductDto"/> に変換される。
/// </para>
/// <para>
/// 検証属性は record のプライマリコンストラクタ<b>引数</b>に付与する(<c>[property:]</c> を付けて
/// プロパティに付与すると、MVC のモデル検証で無視され実行時例外となるため付けない)。
/// </para>
/// </remarks>
/// <param name="Name">商品名(必須・30文字以内)。</param>
/// <param name="Price">商品単価(円): 50〜10000(必須)。</param>
/// <param name="CategoryId">商品カテゴリのUUID(必須)。</param>
/// <param name="StockQuantity">初期在庫数: 0〜100(必須)。</param>
public record ProductCreateSchema(
    [Required(ErrorMessage = "商品名は必須です")]
    [StringLength(30, ErrorMessage = "商品名は30文字以内で指定してください")]
    string Name,

    [Required(ErrorMessage = "単価は必須です")]
    [Range(50, 10000, ErrorMessage = "単価は50以上10000以下で指定してください")]
    int? Price,

    [Required(ErrorMessage = "商品カテゴリIdは必須です")]
    string CategoryId,

    [Required(ErrorMessage = "在庫数は必須です")]
    [Range(0, 100, ErrorMessage = "在庫数は0以上100以下で指定してください")]
    int? StockQuantity);