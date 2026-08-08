// Apps/Ddd.Application/Dtos/ProductDto.cs
namespace Ddd.Application.Dtos;

/// <summary>
/// 商品情報を表す DTO(Data Transfer Object)。
/// </summary>
/// <remarks>
/// <para>
/// ドメインの <see cref="Ddd.Domain.Models.Products.Product"/> エンティティに対応する
/// データ構造であり、アプリケーション層とプレゼンテーション層の間でデータを受け渡すために使用する。
/// </para>
/// <para>
/// DTO はドメイン層の内部構造(エンティティ・値オブジェクト)を隠蔽し、
/// API 仕様や UI の要求に合わせた構造を提供する。カテゴリ・在庫は
/// <see cref="CategoryDto"/> / <see cref="StockDto"/> をネストして表現する
/// (集約のミラー)。
/// </para>
/// <para>
/// OpenAPI(Swagger)のスキーマ属性・例値は、DTO 自体には付与せず、
/// プレゼンテーション層の OpenAPI 設定で表現する方針とする。
/// </para>
/// </remarks>
public class ProductDto
{
    /// <summary>商品ID(UUID形式)。エンティティの識別子に対応。</summary>
    public string? Id { get; set; }

    /// <summary>商品名。<c>ProductName</c> 値オブジェクトに対応。</summary>
    public string? Name { get; set; }

    /// <summary>商品単価(円)。<c>ProductPrice</c> 値オブジェクトに対応。</summary>
    public int? Price { get; set; }

    /// <summary>商品カテゴリ。<see cref="CategoryDto"/> をネストして表現。</summary>
    public CategoryDto? Category { get; set; }

    /// <summary>商品在庫情報。<see cref="StockDto"/> をネストして表現。</summary>
    public StockDto? Stock { get; set; }

    /// <summary>既定のコンストラクタ(JSON バインド等で使用)。</summary>
    public ProductDto()
    {
    }

    /// <summary>全項目を指定して生成する。</summary>
    /// <param name="id">商品ID(UUID形式)。</param>
    /// <param name="name">商品名。</param>
    /// <param name="price">商品単価(円)。</param>
    /// <param name="category">商品カテゴリ。</param>
    /// <param name="stock">商品在庫情報。</param>
    public ProductDto(string? id, string? name, int? price, CategoryDto? category, StockDto? stock)
    {
        Id = id;
        Name = name;
        Price = price;
        Category = category;
        Stock = stock;
    }
}