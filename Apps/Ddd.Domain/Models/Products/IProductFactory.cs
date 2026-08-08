using Ddd.Domain.Models.Products;

namespace Ddd.Domain.Factories;

/// <summary>
/// Product 集約の「合成(受け皿 → 集約)」と「分解(集約 → 受け皿)」を担うファクトリの抽象(ポート)。
/// </summary>
/// <remarks>
/// <para>
/// DDD における Factory パターン。複雑な集約の生成・再構築(reconstitution)と分解の責務を表す。
/// インターフェイスはドメイン層に置き、実装は外側の層(インフラストラクチャ層)が担う。
/// </para>
/// <para>
/// 永続化の「受け皿」型(EF Core のエンティティなど)には依存しないよう、受け皿型をジェネリックパラメータ
/// (<typeparamref name="TProduct"/> / <typeparamref name="TCategory"/> / <typeparamref name="TStock"/>)で
/// 表す。個々の受け皿 ⇔ ドメインの変換は腐敗防止層(Adapter)に委譲する。
/// </para>
/// </remarks>
/// <typeparam name="TProduct">商品の受け皿型(例: EF Core の商品エンティティ)。</typeparam>
/// <typeparam name="TCategory">カテゴリの受け皿型。</typeparam>
/// <typeparam name="TStock">在庫の受け皿型。</typeparam>
public interface IProductFactory<TProduct, TCategory, TStock>
{
    /// <summary>
    /// 3種の受け皿から完全な <see cref="Product"/> 集約を合成(再構築)する。
    /// </summary>
    /// <param name="product">商品の受け皿。</param>
    /// <param name="category">カテゴリの受け皿。</param>
    /// <param name="stock">在庫の受け皿。</param>
    /// <returns>合成済みの <see cref="Product"/> 集約。</returns>
    Product Assemble(TProduct product, TCategory category, TStock stock);

    /// <summary>
    /// 集約から商品の受け皿を作る(INSERT/UPDATE 用)。外部キーは補完しない(Repository が補完する)。
    /// </summary>
    TProduct ToProduct(Product product);

    /// <summary>
    /// 集約から在庫の受け皿を作る(INSERT/UPDATE 用)。外部キーは補完しない(Repository が補完する)。
    /// </summary>
    TStock ToStock(Product product);

    /// <summary>
    /// 集約からカテゴリの UUID を取り出す。Repository で外部キー(category_id)を解決するために利用する。
    /// </summary>
    Guid ExtractCategoryUuid(Product product);
}