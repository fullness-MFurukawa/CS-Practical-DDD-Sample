// Apps/Ddd.Application/Dtos/StockDto.cs
namespace Ddd.Application.Dtos;

/// <summary>
/// 商品在庫情報を表す DTO。
/// </summary>
/// <remarks>
/// ドメインの <see cref="Ddd.Domain.Models.Stocks.Stock"/> エンティティに対応し、
/// 在庫の識別子と数量を保持する。<see cref="ProductDto"/> からネストして参照される。
/// 数量は未指定(null)を検出できるよう <see cref="int"/>? とする。
/// </remarks>
public class StockDto
{
    /// <summary>在庫ID(UUID形式)。<c>StockId</c> 値オブジェクトに対応。</summary>
    public string? Id { get; set; }

    /// <summary>在庫数量。<c>StockQuantity</c> 値オブジェクトに対応。</summary>
    public int? Quantity { get; set; }

    /// <summary>既定のコンストラクタ(JSON バインド等で使用)。</summary>
    public StockDto()
    {
    }

    /// <summary>全項目を指定して生成する。</summary>
    /// <param name="id">在庫ID(UUID形式)。</param>
    /// <param name="quantity">在庫数量。</param>
    public StockDto(string? id, int? quantity)
    {
        Id = id;
        Quantity = quantity;
    }
}