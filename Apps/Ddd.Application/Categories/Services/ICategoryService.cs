// Apps/Ddd.Application/Categories/ICategoryService.cs
using Ddd.Application.Exceptions;
using Ddd.Domain.Models.Categories;

namespace Ddd.Application.Categories.Services;

/// <summary>
/// 商品カテゴリに関するアプリケーションサービスのインターフェイス。
/// </summary>
/// <remarks>
/// <para>
/// Service 層は、ユースケース(UseCase)実現のためのドメイン操作をカプセル化し、
/// 複数のユースケースから共通利用される。
/// </para>
/// <para>
/// この層ではトランザクションは管理しない(境界はユースケース側)。また、ドメインロジックは持たず、
/// リポジトリを介してデータ取得を行う。取得できない場合はアプリケーション例外に翻訳する。
/// </para>
/// </remarks>
public interface ICategoryService
{
    /// <summary>
    /// すべての商品カテゴリを取得する。
    /// </summary>
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 商品カテゴリIdで商品カテゴリを取得する。
    /// </summary>
    /// <exception cref="NotFoundException">
    /// 該当する商品カテゴリが存在しない場合。
    /// </exception>
    Task<Category> GetCategoryByIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default);
}