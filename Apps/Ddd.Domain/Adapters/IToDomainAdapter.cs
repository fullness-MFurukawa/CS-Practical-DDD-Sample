namespace Ddd.Domain.Adapters;

/// <summary>
/// 外部形式(DTO / 永続化エンティティなど)からドメインモデルへの一方向変換を提供する
/// 腐敗防止層(ACL)のアダプタ。GoF の Adapter パターンに相当する。
/// </summary>
/// <remarks>
/// インターフェイスはドメイン層に配置し、実装は外側の層(インフラストラクチャ層/アプリケーション層/プレゼンテーション層)に置く。
/// これによりドメイン層は外部形式に依存しない。
/// </remarks>
/// <typeparam name="TDto">変換元の外部形式の型。</typeparam>
/// <typeparam name="TDomain">変換先のドメイン型。</typeparam>
public interface IToDomainAdapter<in TDto, out TDomain>
{
    /// <summary>外部形式をドメインモデルへ変換する。</summary>
    /// <param name="input">変換元の外部形式。</param>
    /// <returns>変換後のドメインモデル。</returns>
    TDomain ToDomain(TDto input);
}