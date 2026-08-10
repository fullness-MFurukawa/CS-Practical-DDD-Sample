using Ddd.Application.Dtos;

namespace Ddd.Api.Products.Schemas;

/// <summary>
/// プレゼンテーション層の <see cref="ProductUpdateSchema"/> を、アプリケーション層の
/// <see cref="ProductDto"/> へ変換する腐敗防止層(ACL)アダプタ。
/// </summary>
/// <remarks>
/// 商品IDは URI のパスで受け取るためここでは設定せず(<c>null</c>)、コントローラでパスの <c>{id}</c> を補完する。
/// カテゴリは変更対象外のため設定しない(<c>null</c>)。在庫はネストした DTO として在庫数のみを組み立てる
/// (在庫IDは更新時に不要)。
/// </remarks>
public sealed class ProductUpdateSchemaAdapter
{
    /// <summary>
    /// 商品変更スキーマを <see cref="ProductDto"/> に変換する。
    /// </summary>
    /// <param name="schema">商品変更リクエストスキーマ。</param>
    /// <returns>アプリケーション層の商品 DTO(id は未設定=コントローラで補完、category は null、stock はネスト)。</returns>
    public ProductDto ToDto(ProductUpdateSchema schema)
        => new(
            id: null,
            name: schema.Name,
            price: schema.Price,
            category: null,
            stock: new StockDto(null, schema.StockQuantity));
}