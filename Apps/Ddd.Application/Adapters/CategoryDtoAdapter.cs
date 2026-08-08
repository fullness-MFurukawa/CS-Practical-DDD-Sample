// Apps/Ddd.Application/Adapters/CategoryDtoAdapter.cs
using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;
using Ddd.Domain.Adapters;
using Ddd.Domain.Models.Categories;

namespace Ddd.Application.Adapters;

/// <summary>
/// <see cref="Category"/> エンティティと <see cref="CategoryDto"/> の相互変換を行う
/// アプリケーション層のアダプタ。
/// </summary>
/// <remarks>
/// <para>
/// ドメイン層の <see cref="IDomainBiAdapter{TDto, TDomain}"/>(腐敗防止層のアダプタ契約)を
/// 実装する。外部形式(DTO)とドメインモデルの境界で、入力検証と生成/復元の振り分けを担う。
/// </para>
/// <para>
/// ID が未指定なら <see cref="Category.CreateNew(CategoryName)"/>(新規採番)、
/// 指定済みなら <see cref="Category.Restore(CategoryId, CategoryName)"/>(復元)を用いる。
/// 必須項目の欠落は <see cref="InvalidInputException"/> として弾く(値オブジェクトの
/// 不変条件違反は値オブジェクト側が <see cref="Ddd.Domain.Exceptions.DomainException"/> を投げる)。
/// </para>
/// </remarks>
public sealed class CategoryDtoAdapter : IDomainBiAdapter<CategoryDto, Category>
{
    /// <summary>
    /// <see cref="CategoryDto"/> から <see cref="Category"/> エンティティを再構築する。
    /// </summary>
    /// <param name="input">カテゴリ DTO。</param>
    /// <returns>カテゴリエンティティ。</returns>
    /// <exception cref="InvalidInputException">DTO が null、または必須項目が欠落している場合。</exception>
    public Category ToDomain(CategoryDto input)
    {
        if (input is null)
        {
            throw new InvalidInputException("CategoryDtoがnullです。");
        }
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new InvalidInputException("商品カテゴリ名は必須です。");
        }
        if (string.IsNullOrWhiteSpace(input.Id))
        {
            return Category.CreateNew(CategoryName.Create(input.Name));
        }
        return Category.Restore(CategoryId.Parse(input.Id), CategoryName.Create(input.Name));
    }

    /// <summary>
    /// <see cref="Category"/> エンティティを <see cref="CategoryDto"/> に変換する。
    /// </summary>
    /// <param name="domain">カテゴリエンティティ。</param>
    /// <returns>カテゴリ DTO。</returns>
    /// <exception cref="InvalidInputException">引数が null の場合。</exception>
    public CategoryDto FromDomain(Category domain)
    {
        if (domain is null)
        {
            throw new InvalidInputException("Categoryがnullです。");
        }
        return new CategoryDto(domain.CategoryId.ToString(), domain.Name.Value);
    }
}