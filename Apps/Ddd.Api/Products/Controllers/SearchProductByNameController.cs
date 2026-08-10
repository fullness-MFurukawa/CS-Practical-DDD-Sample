using System.ComponentModel.DataAnnotations;
using Ddd.Application.Dtos;
using Ddd.Application.Products.Usecases;
using Microsoft.AspNetCore.Mvc;

namespace Ddd.Api.Products.Controllers;

/// <summary>
/// ユースケース「商品名で検索する」を実現するエンドポイントを提供するコントローラ。
/// </summary>
/// <remarks>
/// <para>
/// クライアントから商品名を受け取り、アプリケーション層のユースケースに委譲する。Controller は
/// ビジネスロジックを持たない「薄い層」であり、トランザクション境界はユースケース側にある。
/// <see cref="ProductDto"/> を返し、ドメイン内部構造(エンティティ・値オブジェクト)は秘匿する。
/// </para>
/// <para>
/// 例外は <c>ApiExceptionHandler</c> で統一処理する(NotFoundException→404、
/// InvalidInputException/DomainException→400)。
/// </para>
/// </remarks>
/// <param name="usecase">ユースケース「商品を名前で検索する」。</param>
[ApiController]
[Route("api/products")]
[Tags("SearchProducts")]
public sealed class SearchProductByNameController(ISearchProductByNameUsecase usecase) : ControllerBase
{
    /// <summary>
    /// 商品名を指定して商品情報を取得する。
    /// </summary>
    /// <remarks>例: <c>GET /api/products/search?name=蛍光ペン(赤)</c></remarks>
    /// <param name="name">商品名(必須・空白のみ不可)。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>該当する商品の DTO。</returns>
    [HttpGet("search")]
    [EndpointSummary("商品名で検索")]
    [EndpointDescription("商品名を指定して商品情報(ProductDto)を取得します。")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> SearchByNameAsync(
        [FromQuery, Required(ErrorMessage = "商品名は必須です")] string name,
        CancellationToken cancellationToken)
    {
        return await usecase.SearchAsync(name, cancellationToken);
    }
}