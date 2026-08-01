namespace Ddd.Domain.Models.Products;

/// <summary>
/// ドメインリポジトリ: 商品集約 <see cref="Product"/> の永続化と再構築を担う契約(ポート)。
/// </summary>
/// <remarks>
/// <para>ドメイン層は永続化の仕組み(EF Coreなど)に依存しない。インフラ層でこの契約を実装する。</para>
/// <para>商品名の一意性(同名不可)は、更新対象自身を除外して判定する必要があるため、
/// ユースケース層で <see cref="FindByNameAsync"/> を用いて検証することを想定する。</para>
/// </remarks>
public interface IProductRepository
{
    /// <summary>新しい商品を永続化する。</summary>
    /// <param name="product">永続化対象の商品集約。</param>
    /// <param name="cancellationToken">キャンセル用トークン。</param>
    Task CreateAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>既存の商品を変更(更新)する。商品IDで対象を特定し、内包する在庫も含めて反映する。</summary>
    /// <param name="product">変更内容を反映済みの商品集約(商品IDで対象を特定する)。</param>
    /// <param name="cancellationToken">キャンセル用トークン。</param>
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>指定された商品名が既に存在するかを確認する。</summary>
    /// <param name="productName">確認対象の商品名。</param>
    /// <param name="cancellationToken">キャンセル用トークン。</param>
    /// <returns>存在すれば <c>true</c>、存在しなければ <c>false</c>。</returns>
    Task<bool> ExistsByNameAsync(ProductName productName, CancellationToken cancellationToken = default);

    /// <summary>商品IDを指定して商品を取得する。存在しない場合は null。</summary>
    /// <param name="productId">取得対象の商品ID。</param>
    /// <param name="cancellationToken">キャンセル用トークン。</param>
    /// <returns>該当する <see cref="Product"/>。存在しない場合は <c>null</c>。</returns>
    Task<Product?> FindByIdAsync(ProductId productId, CancellationToken cancellationToken = default);

    /// <summary>商品名を指定して商品を取得する。存在しない場合は null。</summary>
    /// <param name="productName">取得対象の商品名。</param>
    /// <param name="cancellationToken">キャンセル用トークン。</param>
    /// <returns>該当する <see cref="Product"/>。存在しない場合は <c>null</c>。</returns>
    Task<Product?> FindByNameAsync(ProductName productName, CancellationToken cancellationToken = default);
}