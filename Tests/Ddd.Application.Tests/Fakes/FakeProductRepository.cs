using Ddd.Domain.Models.Products;

namespace Ddd.Application.Tests.Fakes;

/// <summary>
/// テスト用のインメモリ <see cref="IProductRepository"/>。<see cref="Seed"/> で事前データを投入する。
/// </summary>
public sealed class FakeProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();

    /// <summary>テストデータを投入する。</summary>
    public void Seed(params Product[] products) => _products.AddRange(products);

    public Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products.Add(product);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var index = _products.FindIndex(p => p.ProductId.Equals(product.ProductId));
        if (index >= 0)
        {
            _products[index] = product;
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByNameAsync(ProductName productName, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.Any(p => p.Name.Equals(productName)));

    public Task<Product?> FindByIdAsync(ProductId productId, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.FirstOrDefault(p => p.ProductId.Equals(productId)));

    public Task<Product?> FindByNameAsync(ProductName productName, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.FirstOrDefault(p => p.Name.Equals(productName)));
}