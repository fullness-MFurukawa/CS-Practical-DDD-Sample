using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;
using Ddd.Domain.Adapters;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Application.Factories;

/// <summary>
/// 汎用ファクトリ契約 <see cref="IFactory{TAggregate, TExternal}"/> を、
/// <see cref="Product"/> 集約と <see cref="ProductDto"/> の組み合わせで実装したファクトリ。
/// </summary>
/// <remarks>
/// <para>
/// Java 版の <c>ProductDTOAssembler</c> に相当する。3 つの
/// <see cref="IDomainBiAdapter{TDto, TDomain}"/>(商品・カテゴリ・在庫)を統括し、
/// DTO 群とドメイン集約 <see cref="Product"/> の合成/分解を行う。個々の値の変換・検証は
/// 各 Adapter に委譲し、本クラスは集約としての組み立て順序(骨格生成 → カテゴリ/在庫の attach)や
/// null 構造の整合性チェックに責務を絞る。
/// </para>
/// <para>
/// 契約はドメイン層に置き、実装はアプリケーション層に置く。依存はコンストラクタで注入する。同じ
/// <see cref="IDomainBiAdapter{TDto, TDomain}"/> 型でも型引数が異なるため、DI コンテナは
/// 商品用・カテゴリ用・在庫用をそれぞれ解決する。
/// </para>
/// <para>
/// 単独の <see cref="Category"/> → <see cref="CategoryDto"/> 変換(カテゴリ一覧など)は本ファクトリの
/// 責務ではなく、呼び出し側が <see cref="IDomainBiAdapter{TDto, TDomain}"/>(カテゴリ用)を直接用いる。
/// </para>
/// </remarks>
/// <param name="productAdapter">商品(骨格)用のアダプタ。</param>
/// <param name="categoryAdapter">カテゴリ用のアダプタ。</param>
/// <param name="stockAdapter">在庫用のアダプタ。</param>
public sealed class ProductDtoFactory(
    IDomainBiAdapter<ProductDto, Product> productAdapter,
    IDomainBiAdapter<CategoryDto, Category> categoryAdapter,
    IDomainBiAdapter<StockDto, Stock> stockAdapter) : IFactory<Product, ProductDto>
{
    /// <inheritdoc />
    public Product Assemble(ProductDto external)
    {
        if (external is null)
        {
            throw new InvalidInputException("ProductDtoがnullです。");
        }
        if (external.Category is null)
        {
            throw new InvalidInputException("CategoryDtoがnullです。");
        }
        if (external.Stock is null)
        {
            throw new InvalidInputException("StockDtoがnullです。");
        }

        // 骨格を作り、カテゴリ・在庫を attach して集約として合成する。
        var skeleton = productAdapter.ToDomain(external);
        skeleton.AttachCategory(categoryAdapter.ToDomain(external.Category));
        skeleton.AttachStock(stockAdapter.ToDomain(external.Stock));
        return skeleton;
    }

    /// <inheritdoc />
    public ProductDto Disassemble(Product domain)
    {
        if (domain is null)
        {
            throw new InvalidInputException("Productがnullです。");
        }

        var dto = productAdapter.FromDomain(domain);
        if (domain.Category is not null)
        {
            dto.Category = categoryAdapter.FromDomain(domain.Category);
        }
        if (domain.Stock is not null)
        {
            dto.Stock = stockAdapter.FromDomain(domain.Stock);
        }
        return dto;
    }
}