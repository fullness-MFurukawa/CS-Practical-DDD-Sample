using Ddd.Domain.Exceptions;
using Ddd.Domain.Mappers;
using Ddd.Domain.Models.Categories;
using Ddd.Infrastructure.Entities;

namespace Ddd.Infrastructure.Categories;

/// <summary>
/// 永続化エンティティ <see cref="ProductCategoryEntity"/> からドメインエンティティ
/// <see cref="Category"/> を再構築する腐敗防止層(ACL)の Mapper。
/// </summary>
/// <remarks>
/// <para>
/// カテゴリに対する要件は問合せのみのため、変換は <c>ToDomain</c>(Entity → ドメイン)方向だけを持つ。
/// ドメインの VO はファクトリ＋自己検証で「正しい状態のみ」を生成するため、この方向は
/// ソースジェネレータでは生成できず手書きで実装する(値の妥当性検証を伴う ACL の責務)。
/// </para>
/// </remarks>
public sealed class ProductCategoryEntityMapper : IToDomainMapper<ProductCategoryEntity, Category>
{
    /// <summary>
    /// <see cref="ProductCategoryEntity"/> を検証し、<see cref="Category"/> に変換する。
    /// </summary>
    /// <param name="input">DBから取得した永続化エンティティ。</param>
    /// <returns>再構築された <see cref="Category"/>。</returns>
    /// <exception cref="DomainException">入力が null、または UUID・名称が不正な場合。</exception>
    public Category ToDomain(ProductCategoryEntity input)
    {
        if (input is null)
        {
            throw new DomainException("カテゴリ情報が取得できません。");
        }
        if (input.CategoryUuid == Guid.Empty)
        {
            throw new DomainException("カテゴリUUIDが不正です。");
        }
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new DomainException("カテゴリ名が未設定です。");
        }

        // VO のファクトリを通すことで、復元時にも不変条件を再検証する
        return Category.Restore(
            CategoryId.From(input.CategoryUuid),
            CategoryName.Create(input.Name));
    }
}