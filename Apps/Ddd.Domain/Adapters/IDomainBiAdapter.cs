namespace Ddd.Domain.Adapters;

/// <summary>
/// 外部形式とドメインモデルの相互変換を提供する腐敗防止層(ACL)のアダプタ。
/// GoF の Adapter パターンに相当する。
/// </summary>
/// <remarks>
/// インターフェイスはドメイン層に配置し、実装は外側の層に置く。
/// 問合せ(外部→ドメイン)と更新(ドメイン→外部)の双方向変換が必要な箇所で用いる。
/// </remarks>
/// <typeparam name="TDto">外部形式の型。</typeparam>
/// <typeparam name="TDomain">ドメイン型。</typeparam>
public interface IDomainBiAdapter<TDto, TDomain>
{
    /// <summary>外部形式をドメインモデルへ変換する。</summary>
    TDomain ToDomain(TDto input);

    /// <summary>ドメインモデルを外部形式へ変換する。</summary>
    TDto FromDomain(TDomain domain);
}