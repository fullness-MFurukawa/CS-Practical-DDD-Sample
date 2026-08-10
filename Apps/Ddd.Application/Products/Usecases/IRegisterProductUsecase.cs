using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;

namespace Ddd.Application.Products.Usecases;

/// <summary>
/// ユースケース「商品を登録する」を実現するアプリケーション層のインターフェイス。
/// </summary>
/// <remarks>
/// <para>役割:</para>
/// <list type="bullet">
///   <item><description>プレゼンテーション層からの要求に応じて、商品登録に必要なアプリケーション処理を統括する。</description></item>
///   <item><description>カテゴリの参照、存在確認、登録実行、登録結果の返却までを一貫した操作として提供する。</description></item>
/// </list>
/// <para>非責務:</para>
/// <list type="bullet">
///   <item><description>ドメインルールの実装(エンティティ/値オブジェクトに委譲)。</description></item>
///   <item><description>永続化の詳細(リポジトリ/インフラストラクチャ層に委譲)。</description></item>
/// </list>
/// </remarks>
public interface IRegisterProductUsecase
{
    /// <summary>
    /// すべての商品カテゴリを取得する。
    /// </summary>
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定されたカテゴリIdでカテゴリを取得する。
    /// </summary>
    /// <exception cref="NotFoundException">
    /// 指定Idのカテゴリが存在しないとき。
    /// </exception>
    Task<CategoryDto> GetCategoryByIdAsync(string categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定された商品名が既に存在するかを検査する(存在すれば例外で通知する)。
    /// </summary>
    /// <exception cref="ExistsException">
    /// 同名の商品が既に存在するとき。
    /// </exception>
    Task ExistsProductAsync(string productName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 商品を登録する。登録後は、登録結果(Id等を含む最新状態)の DTO を返す。
    /// </summary>
    /// <exception cref="InvalidInputException">
    /// DTOの必須項目不足や変換不能など入力が不正なとき。
    /// </exception>
    /// <exception cref="NotFoundException">
    /// 指定されたカテゴリが存在しないとき。
    /// </exception>
    /// <exception cref="ExistsException">
    /// 同名の商品が既に存在するとき。
    /// </exception>
    Task<ProductDto> AddProductAsync(ProductDto product, CancellationToken cancellationToken = default);
}