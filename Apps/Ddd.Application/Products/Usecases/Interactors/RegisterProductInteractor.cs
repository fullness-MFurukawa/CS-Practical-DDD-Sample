using Ddd.Application.Categories.Services;
using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;
using Ddd.Application.Persistence;
using Ddd.Application.Products.Services;
using Ddd.Domain.Adapters;
using Ddd.Domain.Factories;
using Ddd.Domain.Models.Categories;
using Ddd.Domain.Models.Products;

namespace Ddd.Application.Products.Usecases.Interactors;

/// <summary>
/// ユースケース「商品を登録する」の実装(Interactor)。
/// </summary>
/// <remarks>
/// <para>
/// Service と Factory / Adapter を組み合わせ、DTO とドメインの橋渡しを行う。読み取り系
/// (<see cref="GetCategoriesAsync"/> など)はトランザクションを張らず、書き込み系
/// (<see cref="AddProductAsync"/>)は <see cref="IUnitOfWork"/> でトランザクション境界を明確化する。
/// </para>
/// <para>
/// 単独の <see cref="Category"/> → <see cref="CategoryDto"/> 変換は、商品集約のファクトリではなく
/// カテゴリ用の <see cref="IDomainBiAdapter{TDto, TDomain}"/> を直接用いる(葉の1:1変換は Adapter の責務)。
/// </para>
/// </remarks>
/// <param name="productService">商品のアプリケーションサービス。</param>
/// <param name="categoryService">商品カテゴリのアプリケーションサービス。</param>
/// <param name="factory">商品集約 ⇔ 商品 DTO のファクトリ。</param>
/// <param name="categoryAdapter">カテゴリ ⇔ カテゴリ DTO のアダプタ。</param>
/// <param name="unitOfWork">トランザクション境界(Unit of Work)。</param>
public sealed class RegisterProductInteractor(
    IProductService productService,
    ICategoryService categoryService,
    IFactory<Product, ProductDto> factory,
    IDomainBiAdapter<CategoryDto, Category> categoryAdapter,
    IUnitOfWork unitOfWork) : IRegisterProductUsecase
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await categoryService.GetCategoriesAsync(cancellationToken);
        return categories.Select(categoryAdapter.FromDomain).ToList();
    }

    /// <inheritdoc />
    public async Task<CategoryDto> GetCategoryByIdAsync(string categoryId, CancellationToken cancellationToken = default)
    {
        var category = await categoryService.GetCategoryByIdAsync(CategoryId.Parse(categoryId), cancellationToken);
        return categoryAdapter.FromDomain(category);
    }

    /// <inheritdoc />
    public Task ExistsProductAsync(string productName, CancellationToken cancellationToken = default)
        => productService.ExistsProductAsync(ProductName.Create(productName), cancellationToken);

    /// <inheritdoc />
    public async Task<ProductDto> AddProductAsync(ProductDto product, CancellationToken cancellationToken = default)
    {
        if (product is null)
        {
            throw new InvalidInputException("ProductDtoがnullです。");
        }
        if (product.Category is null || string.IsNullOrWhiteSpace(product.Category.Id))
        {
            throw new InvalidInputException("商品カテゴリIDは必須です。");
        }

        var categoryId = CategoryId.Parse(product.Category.Id);

        // 登録処理一式をひとつのトランザクション境界で実行する。
        return await unitOfWork.ExecuteAsync(async token =>
        {
            // 商品カテゴリを取得し、DTOのカテゴリを DB 由来の正しい内容で上書きする。
            var category = await categoryService.GetCategoryByIdAsync(categoryId, token);
            product.Category = categoryAdapter.FromDomain(category);

            // DTO → ドメイン集約 Product を合成(ここで name/price 等が検証される)。
            var toRegister = factory.Assemble(product);

            // 同名商品の重複チェック(存在すれば ExistsException)。
            await productService.ExistsProductAsync(toRegister.Name, token);

            // 登録する。
            await productService.AddProductAsync(toRegister, token);

            // 登録結果(DB 上の最新状態)を取得して DTO に変換して返す。
            var registered = await productService.GetProductByNameAsync(toRegister.Name, token);
            return factory.Disassemble(registered);
        }, cancellationToken);
    }
}