using Ddd.Application.Dtos;

namespace Ddd.Api.Products.Schemas;

/// <summary>
/// プレゼンテーション層の <see cref="ProductCreateSchema"/> を、アプリケーション層の
/// <see cref="ProductDto"/> へ変換する腐敗防止層(ACL)アダプタ。
/// </summary>
/// <remarks>
/// 新規登録なので商品Idは設定しない。カテゴリ・在庫はネストした DTO として組み立てる。
/// カテゴリ名は登録処理(ユースケースの <c>AddProductAsync</c>)側で DB の正しい値に解決・上書きされるため、
/// ここでは <c>null</c> とする。
/// </remarks>
public sealed class ProductCreateSchemaAdapter
{
    /// <summary>
    /// 商品登録スキーマを <see cref="ProductDto"/> に変換する。
    /// </summary>
    /// <param name="schema">商品登録リクエストスキーマ。</param>
    /// <returns>アプリケーション層の商品 DTO(id は未設定、category/stock はネスト)。</returns>
    public ProductDto ToDto(ProductCreateSchema schema)
        => new(
            id: null,
            name: schema.Name,
            price: schema.Price,
            category: new CategoryDto(schema.CategoryId, null),
            stock: new StockDto(null, schema.StockQuantity));
}