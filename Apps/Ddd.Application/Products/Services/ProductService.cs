// Apps/Ddd.Application/Products/ProductService.cs
using Ddd.Application.Exceptions;
using Ddd.Domain.Models.Products;

namespace Ddd.Application.Products.Services;

/// <summary>
/// <see cref="IProductService"/> の実装クラス。
/// </summary>
/// <remarks>
/// リポジトリを介してドメインモデルを操作し、アプリケーション層の例外をスローする。
/// ユースケースから呼び出され、ドメイン層を抽象化したファサードとして振る舞う。
/// トランザクションはユースケース層で管理する。
/// </remarks>
/// <param name="repository">商品のリポジトリ(ドメインのポート)。</param>
public sealed class ProductService(IProductRepository repository) : IProductService
{
    /// <inheritdoc />
    public async Task ExistsProductAsync(ProductName productName, CancellationToken cancellationToken = default)
    {
        if (await repository.ExistsByNameAsync(productName, cancellationToken))
        {
            throw new ExistsException($"商品名:[{productName.Value}]は既に登録済みです。");
        }
    }

    /// <inheritdoc />
    public async Task ExistsProductExceptAsync(
        ProductName productName, ProductId productId, CancellationToken cancellationToken = default)
    {
        // 同名商品を検索する。存在しなければ重複なし(何もしない)。
        var existing = await repository.FindByNameAsync(productName, cancellationToken);

        // 同名商品が存在しても、それが更新対象自身なら重複ではない。
        // 「同名商品が存在し、かつその商品Idが更新対象と異なる場合のみ」例外とする。
        if (existing is not null && !existing.ProductId.Equals(productId))
        {
            throw new ExistsException($"商品名:[{productName.Value}]は既に登録済みです。");
        }
    }

    /// <inheritdoc />
    public async Task<Product> GetProductByIdAsync(ProductId productId, CancellationToken cancellationToken = default)
    {
        var product = await repository.FindByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException($"商品Id:[{productId.Value}]の商品は存在しません。");
        }
        return product;
    }

    /// <inheritdoc />
    public async Task<Product> GetProductByNameAsync(ProductName productName, CancellationToken cancellationToken = default)
    {
        var product = await repository.FindByNameAsync(productName, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException($"商品名:[{productName.Value}]の商品は存在しません。");
        }
        return product;
    }

    /// <inheritdoc />
    public Task AddProductAsync(Product product, CancellationToken cancellationToken = default)
        => repository.CreateAsync(product, cancellationToken);

    /// <inheritdoc />
    public Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
        => repository.UpdateAsync(product, cancellationToken);
}