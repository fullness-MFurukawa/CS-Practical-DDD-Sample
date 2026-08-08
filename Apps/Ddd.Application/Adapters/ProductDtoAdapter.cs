// Apps/Ddd.Application/Adapters/ProductDtoAdapter.cs
using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;
using Ddd.Domain.Adapters;
using Ddd.Domain.Models.Products;

namespace Ddd.Application.Adapters;

/// <summary>
/// <see cref="Product"/> エンティティと <see cref="ProductDto"/> の相互変換を行う
/// アプリケーション層のアダプタ。
/// </summary>
/// <remarks>
/// <para>
/// ドメイン層の <see cref="IDomainBiAdapter{TDto, TDomain}"/> を実装する。
/// 本アダプタは商品の骨格(id・name・price)のみを扱い、カテゴリ・在庫の合成/分解は
/// <see cref="Ddd.Application.Factories.ProductDtoFactory"/> が担当する。
/// </para>
/// <para>
/// ID が未指定なら <see cref="ProductId.New"/>(新規採番)、指定済みなら
/// <see cref="ProductId.Parse(string)"/>(復元)を用い、
/// <see cref="Product.RestoreSkeleton(ProductId, ProductName, ProductPrice)"/> で骨格を生成する。
/// </para>
/// </remarks>
public sealed class ProductDtoAdapter : IDomainBiAdapter<ProductDto, Product>
{
    /// <summary>
    /// <see cref="ProductDto"/> から <see cref="Product"/> エンティティを骨格として再構築する。
    /// カテゴリ・在庫は含まない。
    /// </summary>
    /// <param name="input">商品 DTO。</param>
    /// <returns>骨格状態の <see cref="Product"/>(カテゴリ・在庫は未設定)。</returns>
    /// <exception cref="InvalidInputException">DTO が null、または必須項目(名前・単価)が欠落している場合。</exception>
    public Product ToDomain(ProductDto input)
    {
        if (input is null)
        {
            throw new InvalidInputException("ProductDtoがnullです。");
        }
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new InvalidInputException("商品名は必須です。");
        }
        if (input.Price is null)
        {
            throw new InvalidInputException("商品単価は必須です。");
        }
        return Product.RestoreSkeleton(
            string.IsNullOrWhiteSpace(input.Id) ? ProductId.New() : ProductId.Parse(input.Id),
            ProductName.Create(input.Name),
            ProductPrice.Create(input.Price.Value));
    }

    /// <summary>
    /// <see cref="Product"/> エンティティを <see cref="ProductDto"/>(骨格)に変換する。
    /// カテゴリ・在庫は含めない(<c>null</c>)。
    /// </summary>
    /// <param name="domain">商品エンティティ。</param>
    /// <returns>骨格状態の商品 DTO。</returns>
    /// <exception cref="InvalidInputException">引数が null の場合。</exception>
    public ProductDto FromDomain(Product domain)
    {
        if (domain is null)
        {
            throw new InvalidInputException("Productがnullです。");
        }
        return new ProductDto(
            domain.ProductId.ToString(),
            domain.Name.Value,
            domain.Price.Value,
            null,
            null);
    }
}