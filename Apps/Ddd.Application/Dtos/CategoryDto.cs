// Apps/Ddd.Application/Dtos/CategoryDto.cs
namespace Ddd.Application.Dtos;

/// <summary>
/// 商品カテゴリ情報を表す DTO(Data Transfer Object)。
/// </summary>
/// <remarks>
/// <para>
/// ドメインの <see cref="Ddd.Domain.Models.Categories.Category"/> エンティティに対応し、
/// 商品カテゴリの識別子および名称を保持する。
/// </para>
/// <para>
/// 他の DTO(例: <see cref="ProductDto"/>)からネストして参照されることが多く、
/// 集約構造のミラーとして設計されている。DTO は可変(get/set)であり、
/// ユースケースでの上書きや、プレゼンテーション層での JSON バインドを想定する。
/// </para>
/// </remarks>
public class CategoryDto
{
    /// <summary>カテゴリID(UUID形式)。<c>CategoryId</c> 値オブジェクトに対応。</summary>
    public string? Id { get; set; }

    /// <summary>カテゴリ名。<c>CategoryName</c> 値オブジェクトに対応。</summary>
    public string? Name { get; set; }

    /// <summary>既定のコンストラクタ(JSON バインド等で使用)。</summary>
    public CategoryDto()
    {
    }

    /// <summary>全項目を指定して生成する。</summary>
    /// <param name="id">カテゴリID(UUID形式)。</param>
    /// <param name="name">カテゴリ名。</param>
    public CategoryDto(string? id, string? name)
    {
        Id = id;
        Name = name;
    }
}