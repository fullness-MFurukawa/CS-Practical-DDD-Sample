using Ddd.Application.Exceptions;
using Ddd.Domain.Models.Products;

namespace Ddd.Application.Products.Services;

/// <summary>
/// 商品に関するアプリケーションサービスのインターフェイス。
/// </summary>
/// <remarks>
/// ユースケースから呼び出されるドメイン操作の窓口を定義する。Service は単一のエンティティ
/// (ここでは <see cref="Product"/>)に対して作成し、複数のユースケースから共通利用される。
/// トランザクションはユースケース層で管理する。
/// </remarks>
public interface IProductService
{
    /// <summary>
    /// 指定された商品名が未登録であることを確認する(登録済みなら例外)。
    /// </summary>
    /// <exception cref="ExistsException">
    /// 指定された商品名の商品が既に存在する場合。
    /// </exception>
    Task ExistsProductAsync(ProductName productName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定された商品名が、更新対象の商品自身を除いて未使用であることを確認する。
    /// </summary>
    /// <exception cref="ExistsException">
    /// 指定された商品名の商品が、更新対象以外に既に存在する場合。
    /// </exception>
    Task ExistsProductExceptAsync(ProductName productName, ProductId productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 商品Idで商品を取得する。
    /// </summary>
    /// <exception cref="NotFoundException">
    /// 該当する商品が存在しない場合。
    /// </exception>
    Task<Product> GetProductByIdAsync(ProductId productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 商品名で商品を取得する。
    /// </summary>
    /// <exception cref="NotFoundException">
    /// 該当する商品が存在しない場合。
    /// </exception>
    Task<Product> GetProductByNameAsync(ProductName productName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 商品を登録する。
    /// </summary>
    Task AddProductAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// 商品を変更(更新)する。
    /// </summary>
    Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default);
}