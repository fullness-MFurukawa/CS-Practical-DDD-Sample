namespace Ddd.Domain.Mappers;

/// <summary>
/// 外部形式とドメインモデルの相互変換を提供する腐敗防止層(ACL)のポート。
/// </summary>
/// <remarks>
/// インターフェイスはドメイン層に配置し、実装は外側の層(インフラストラクチャ層/アプリケーション層/プレゼンテーション層)に置く。
/// 問合せ(外部→ドメイン)と更新(ドメイン→外部)の双方向変換が必要な箇所で用いる。
/// </remarks>
/// <typeparam name="TDto">外部形式の型。</typeparam>
/// <typeparam name="TDomain">ドメイン型。</typeparam>
public interface IDomainBiMapper<TDto, TDomain>
{
    /// <summary>外部形式をドメインモデルへ変換する。</summary>
    /// <param name="input">変換元の外部形式。</param>
    /// <returns>変換後のドメインモデル。</returns>
    TDomain ToDomain(TDto input);

    /// <summary>ドメインモデルを外部形式へ変換する。</summary>
    /// <param name="domain">変換元のドメインモデル。</param>
    /// <returns>変換後の外部形式。</returns>
    TDto FromDomain(TDomain domain);
}