using System.ComponentModel.DataAnnotations;
using Ddd.Api.Products.Schemas;
using Ddd.Application.Dtos;
using Ddd.Application.Products.Usecases;
using Microsoft.AspNetCore.Mvc;

namespace Ddd.Api.Products.Controllers;

/// <summary>
/// ユースケース「商品を登録する」を実現するエンドポイント群を提供するコントローラ。
/// </summary>
/// <remarks>
/// <para>
/// クライアントからの HTTP リクエストを受け取り、アプリケーション層のユースケースに委譲する。
/// Controller は「変換と委譲」に徹し、ビジネスロジックは持たない。トランザクション境界は
/// ユースケース側にある。ドメイン内部構造は晒さず、DTO / Schema で API 境界を保つ。
/// </para>
/// <para>
/// 例外は <c>ApiExceptionHandler</c> で統一処理する(NotFoundException→404、
/// ExistsException→409、InvalidInputException/DomainException→400)。
/// </para>
/// </remarks>
/// <param name="usecase">ユースケース「商品を登録する」。</param>
/// <param name="adapter">商品登録スキーマ → <see cref="ProductDto"/> の変換アダプタ。</param>
[ApiController]
[Route("api/products")]
[Tags("RegisterProducts")]
public sealed class RegisterProductController(
    IRegisterProductUsecase usecase,
    ProductCreateSchemaAdapter adapter) : ControllerBase
{
    /// <summary>商品カテゴリ一覧を取得する(登録時のプルダウンなどに使用)。</summary>
    [HttpGet("categories")]
    [EndpointSummary("カテゴリ一覧取得")]
    [EndpointDescription("登録時のプルダウンなどに使用するカテゴリ一覧を返します。")]
    [ProducesResponseType<IReadOnlyList<CategoryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken)
        => Ok(await usecase.GetCategoriesAsync(cancellationToken));

    /// <summary>指定された商品カテゴリIdのカテゴリを取得する。</summary>
    [HttpGet("categories/{id}")]
    [EndpointSummary("商品カテゴリ取得")]
    [EndpointDescription("カテゴリId(UUID)を指定してカテゴリ情報を取得します。")]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetCategoryByIdAsync(string id, CancellationToken cancellationToken)
        => await usecase.GetCategoryByIdAsync(id, cancellationToken);

    /// <summary>指定された商品名が既に存在するかを確認する。存在すれば 409、存在しなければ 204。</summary>
    [HttpGet("exists")]
    [EndpointSummary("商品名の存在チェック")]
    [EndpointDescription("指定した商品名が既に存在するか判定します。存在する場合は409を返します。")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckExistsAsync(
        [FromQuery, Required(ErrorMessage = "商品名は必須です")] string name,
        CancellationToken cancellationToken)
    {
        await usecase.ExistsProductAsync(name, cancellationToken);
        return NoContent();
    }

    /// <summary>商品を登録する。成功時は 201 Created(Location 付き)。</summary>
    /// <remarks>
    /// 重複チェック・カテゴリ解決・登録・登録結果の再取得は、ユースケース(<c>AddProductAsync</c>)内で
    /// 1トランザクションとして完結する。
    /// </remarks>
    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    [EndpointSummary("商品登録")]
    [EndpointDescription("商品を新規登録します。成功時は201 Createdを返します。")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> RegisterAsync(
        [FromBody] ProductCreateSchema request,
        CancellationToken cancellationToken)
    {
        // ProductCreateSchema → ProductDto
        var dto = adapter.ToDto(request);

        // 登録(重複チェック・カテゴリ解決・登録・再取得は AddProductAsync 内で完結する)。
        var created = await usecase.AddProductAsync(dto, cancellationToken);

        var location = $"/api/products/{created.Id}";
        return Created(location, created);
    }
}