using Ddd.Application.Dtos;
using Ddd.Application.Events;
using Ddd.Application.Exceptions;
using Ddd.Application.Persistence;
using Ddd.Application.Products.Services;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Application.Products.Usecases.Interactors;

/// <summary>
/// ユースケース「商品を変更する」の実装(Interactor)。
/// </summary>
/// <remarks>
/// <para>
/// 読み取り(<see cref="GetProductAsync"/>)はトランザクションを張らず、書き込み
/// (<see cref="UpdateProductAsync"/>)は <see cref="IUnitOfWork"/> でトランザクション境界を明確化する。
/// </para>
/// <para>
/// 変更適用(<c>Rename</c>/<c>Reprice</c>/<c>ChangeStock</c>)で集約に蓄積されたドメインイベントを、
/// 永続化のあとに <see cref="IDomainEventDispatcher"/> で配送する。配送はトランザクション内で行うため、
/// ハンドラが失敗すれば変更ごとロールバックされる(all-or-nothing)。
/// </para>
/// </remarks>
/// <param name="productService">商品のアプリケーションサービス。</param>
/// <param name="factory">商品集約 ⇔ 商品 DTO のファクトリ。</param>
/// <param name="unitOfWork">トランザクション境界(Unit of Work)。</param>
/// <param name="dispatcher">ドメインイベントのディスパッチャ。</param>
public sealed class UpdateProductInteractor(
    IProductService productService,
    IFactory<Product, ProductDto> factory,
    IUnitOfWork unitOfWork,
    IDomainEventDispatcher dispatcher) : IUpdateProductUsecase
{
    /// <inheritdoc />
    public async Task<ProductDto> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new InvalidInputException("商品IDは必須です。");
        }

        // Id で取得(存在必須)し、DTO へ変換して返す。
        var product = await productService.GetProductByIdAsync(ProductId.Parse(productId), cancellationToken);
        return factory.Disassemble(product);
    }

    /// <inheritdoc />
    public async Task<ProductDto> UpdateProductAsync(ProductDto product, CancellationToken cancellationToken = default)
    {
        // 1. 入力ガード。
        if (product is null)
        {
            throw new InvalidInputException("ProductDtoがnullです。");
        }
        if (string.IsNullOrWhiteSpace(product.Id))
        {
            throw new InvalidInputException("商品IDは必須です。");
        }
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new InvalidInputException("商品名は必須です。");
        }
        if (product.Price is null)
        {
            throw new InvalidInputException("商品単価は必須です。");
        }
        if (product.Stock is null || product.Stock.Quantity is null)
        {
            throw new InvalidInputException("在庫数は必須です。");
        }

        // 2. 変更後の値オブジェクトを生成(ここで名称・単価・在庫数が検証される)。
        var productId = ProductId.Parse(product.Id);
        var newName = ProductName.Create(product.Name);
        var newPrice = ProductPrice.Create(product.Price.Value);
        var newQuantity = StockQuantity.Create(product.Stock.Quantity.Value);

        // 3. 変更処理一式をひとつのトランザクション境界で実行する。
        return await unitOfWork.ExecuteAsync(async token =>
        {
            // 変更対象の集約を取得(存在必須。カテゴリ・在庫を識別子ごと保持している)。
            var current = await productService.GetProductByIdAsync(productId, token);

            // 同名重複チェック(自分自身を除く。他商品が使用中なら ExistsException)。
            await productService.ExistsProductExceptAsync(newName, current.ProductId, token);

            // 取得した集約に変更を適用(実際に変わった項目についてドメインイベントが積まれる)。
            current.Rename(newName);
            current.Reprice(newPrice);
            current.ChangeStock(newQuantity);

            // 永続化する。
            await productService.UpdateProductAsync(current, token);

            // 蓄積されたドメインイベントを取り出して配送する
            // (トランザクション内なので、ハンドラが失敗すれば変更ごと巻き戻る)。
            await dispatcher.DispatchAsync(current.PullDomainEvents(), token);

            // 最新状態を再取得して DTO に変換して返す。
            var updated = await productService.GetProductByIdAsync(current.ProductId, token);
            return factory.Disassemble(updated);
        }, cancellationToken);
    }
}