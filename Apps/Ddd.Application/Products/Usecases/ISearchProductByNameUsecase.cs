using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;

namespace Ddd.Application.Products.Usecases;

/// <summary>
/// ユースケース「商品を名前で検索する」を実現するインターフェイス。
/// </summary>
public interface ISearchProductByNameUsecase
{
    /// <summary>
    /// 商品名を指定して商品情報を取得する。
    /// </summary>
    /// <param name="name">商品名。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>該当する商品の DTO。</returns>
    /// <exception cref="NotFoundException">指定された商品名の商品が存在しないとき。</exception>
    Task<ProductDto> SearchAsync(string name, CancellationToken cancellationToken = default);
}