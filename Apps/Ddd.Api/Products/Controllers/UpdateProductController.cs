using Ddd.Api.Products.Schemas;
using Ddd.Application.Dtos;
using Ddd.Application.Products.Usecases;
using Microsoft.AspNetCore.Mvc;

namespace Ddd.Api.Products.Controllers;

/// <summary>
/// ユースケース「商品を変更する」を実現するエンドポイント群を提供するコントローラ。
/// </summary>
/// <remarks>
/// <para>
/// 商品IDは URI のパス <c>{id}</c> で受け取り、ボディには含めない(URI がリソースを一意に指す)。
/// 変更対象は名称・単価・在庫数のみ。カテゴリは変更対象外。Controller は「変換と委譲」に徹し、
/// トランザクション境界はユースケース側にある。
/// </para>
/// <para>
/// 例外は <c>ApiExceptionHandler</c> で統一処理する(NotFoundException→404、
/// ExistsException→409(他商品が同名を使用中)、InvalidInputException/DomainException→400)。
/// </para>
/// </remarks>
/// <param name="usecase">ユースケース「商品を変更する」。</param>
/// <param name="adapter">商品変更スキーマ → <see cref="ProductDto"/> の変換アダプタ。</param>
[ApiController]
[Route("api/products")]
[Tags("UpdateProducts")]
public sealed class UpdateProductController(
    IUpdateProductUsecase usecase,
    ProductUpdateSchemaAdapter adapter) : ControllerBase
{
    /// <summary>変更対象の商品を取得する(編集画面の初期表示用)。</summary>
    [HttpGet("{id}")]
    [EndpointSummary("商品取得(変更用)")]
    [EndpointDescription("商品Id(UUID)を指定して、変更対象の商品情報を取得します。")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductAsync(string id, CancellationToken cancellationToken)
        => await usecase.GetProductAsync(id, cancellationToken);

    /// <summary>商品を変更する。成功時は 200 OK。</summary>
    /// <remarks>
    /// 取得・変更適用・重複チェック(自分自身を除く)・更新・再取得・ドメインイベントの配送は、
    /// ユースケース(<c>UpdateProductAsync</c>)内で1トランザクションとして完結する。
    /// </remarks>
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [EndpointSummary("商品変更")]
    [EndpointDescription("商品の名称・単価・在庫数を変更します。成功時は200を返します。")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> UpdateAsync(
        string id,
        [FromBody] ProductUpdateSchema request,
        CancellationToken cancellationToken)
    {
        // ProductUpdateSchema → ProductDto(id はパスから補完する)
        var dto = adapter.ToDto(request);
        dto.Id = id;

        var updated = await usecase.UpdateProductAsync(dto, cancellationToken);
        return Ok(updated);
    }
}