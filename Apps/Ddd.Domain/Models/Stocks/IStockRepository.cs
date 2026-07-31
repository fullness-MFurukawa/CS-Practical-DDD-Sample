namespace Ddd.Domain.Models.Stocks;

/// <summary>
/// ドメインリポジトリ: 在庫 <see cref="Stock"/> の永続化を担う契約(ポート)。
/// </summary>
public interface IStockRepository
{
    /// <summary>新しい在庫を永続化する。</summary>
    /// <param name="stock">永続化対象の在庫。</param>
    /// <param name="cancellationToken">キャンセル用トークン。</param>
    Task CreateAsync(Stock stock, CancellationToken cancellationToken = default);
}