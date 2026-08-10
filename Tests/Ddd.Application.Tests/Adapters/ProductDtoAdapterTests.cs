using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;
using Ddd.Domain.Adapters;
using Ddd.Domain.Models.Products;

namespace Ddd.Application.Tests.Adapters;

/// <summary>
/// <see cref="Ddd.Application.Adapters.ProductDtoAdapter"/>(<see cref="ProductDto"/> ⇔ <see cref="Product"/>)の
/// テスト。骨格(id/name/price)のみを扱い、カテゴリ・在庫は含めない。テスト対象は DI から解決する。
/// </summary>
[TestClass]
[TestCategory("Application.Adapters")]
public sealed class ProductDtoAdapterTests : ApplicationTestBase
{
    private IDomainBiAdapter<ProductDto, Product> Adapter
        => GetRequiredService<IDomainBiAdapter<ProductDto, Product>>();

    private const string Uuid = "0f8fad5b-d9cb-469f-a165-70867728950e";

    [TestMethod(DisplayName = "Id未指定なら新規採番の骨格を生成する_カテゴリと在庫はnull")]
    public void ToDomain_CreatesSkeleton_WhenIdMissing()
    {
        var product = Adapter.ToDomain(new ProductDto(null, "万年筆", 3000, null, null));

        Assert.AreEqual("万年筆", product.Name.Value);
        Assert.AreEqual(3000, product.Price.Value);
        Assert.AreNotEqual(Guid.Empty, product.ProductId.Value);
        Assert.IsNull(product.Category);
        Assert.IsNull(product.Stock);
    }

    [TestMethod(DisplayName = "Id指定なら骨格を復元する")]
    public void ToDomain_RestoresSkeleton_WhenIdGiven()
    {
        var product = Adapter.ToDomain(new ProductDto(Uuid, "万年筆", 3000, null, null));

        Assert.AreEqual(Uuid, product.ProductId.ToString());
        Assert.AreEqual("万年筆", product.Name.Value);
        Assert.AreEqual(3000, product.Price.Value);
    }

    [TestMethod(DisplayName = "DTOがnullなら例外")]
    public void ToDomain_ThrowsWhenNull()
        => Assert.ThrowsExactly<InvalidInputException>(() => Adapter.ToDomain(null!));

    [TestMethod(DisplayName = "商品名が空白なら例外")]
    public void ToDomain_ThrowsWhenNameBlank()
    {
        var ex = Assert.ThrowsExactly<InvalidInputException>(
            () => Adapter.ToDomain(new ProductDto(Uuid, "  ", 3000, null, null)));
        Assert.AreEqual("商品名は必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "単価が未指定なら例外")]
    public void ToDomain_ThrowsWhenPriceNull()
    {
        var ex = Assert.ThrowsExactly<InvalidInputException>(
            () => Adapter.ToDomain(new ProductDto(Uuid, "万年筆", null, null, null)));
        Assert.AreEqual("商品単価は必須です。", ex.Message);
    }

    [TestMethod(DisplayName = "Productを骨格DTOに変換する_カテゴリと在庫はnull")]
    public void FromDomain_ConvertsSkeletonDto()
    {
        var product = Product.RestoreSkeleton(
            ProductId.Parse(Uuid), ProductName.Create("万年筆"), ProductPrice.Create(3000));

        var dto = Adapter.FromDomain(product);

        Assert.AreEqual(Uuid, dto.Id);
        Assert.AreEqual("万年筆", dto.Name);
        Assert.AreEqual(3000, dto.Price);
        Assert.IsNull(dto.Category);
        Assert.IsNull(dto.Stock);
    }

    [TestMethod(DisplayName = "Productがnullなら例外")]
    public void FromDomain_ThrowsWhenNull()
        => Assert.ThrowsExactly<InvalidInputException>(() => Adapter.FromDomain(null!));
}