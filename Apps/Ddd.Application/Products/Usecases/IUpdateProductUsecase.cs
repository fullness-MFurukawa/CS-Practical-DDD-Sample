using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;

namespace Ddd.Application.Products.Usecases;

/// <summary>
/// ユースケース「商品を変更する」を実現するアプリケーション層のインターフェイス。
/// </summary>
/// <remarks>
/// <para>役割:</para>
/// <list type="bullet">
///   <item><description>変更対象の取得(編集用)、変更の適用、同名重複チェック、変更実行、変更結果の返却までを一貫した操作として提供する。</description></item>
/// </list>
/// <para>変更対象は商品の名称・単価・在庫数であり、カテゴリは変更対象外とする。</para>
/// </remarks>
public interface IUpdateProductUsecase
{
    /// <summary>変更対象の商品を取得する(編集画面の初期表示などで利用)。</summary>
    /// <exception cref="InvalidInputException">Id形式が不正な場合など入力が不正なとき。</exception>
    /// <exception cref="NotFoundException">指定Idの商品が存在しないとき。</exception>
    Task<ProductDto> GetProductAsync(string productId, CancellationToken cancellationToken = default);

    /// <summary>商品を変更する。変更後は、変更結果(最新状態)の DTO を返す。</summary>
    /// <exception cref="InvalidInputException">DTOの必須項目不足や変換不能など入力が不正なとき。</exception>
    /// <exception cref="NotFoundException">指定Idの商品が存在しないとき。</exception>
    /// <exception cref="ExistsException">指定された商品名が、変更対象以外の商品で既に使用されているとき。</exception>
    Task<ProductDto> UpdateProductAsync(ProductDto product, CancellationToken cancellationToken = default);
}