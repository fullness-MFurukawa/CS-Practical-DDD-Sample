// Apps/Ddd.Domain/Factories/IFactory.cs  ★層に依存しない汎用ファクトリ契約
namespace Ddd.Domain.Factories;

/// <summary>
/// 集約と「外部表現」の合成(外部 → 集約)・分解(集約 → 外部)を担う汎用ファクトリの抽象(ポート)。
/// </summary>
/// <remarks>
/// <para>
/// DDD における Factory パターン。複雑な集約の生成・再構築(reconstitution)と分解の責務を、
/// 特定の集約・外部表現に依存しない形で表す。契約(インターフェイス)はドメイン層に置き、
/// 実装は外側の層(アプリケーション層・インフラストラクチャ層など)が担う。
/// </para>
/// <para>
/// 集約型・外部表現型をともにジェネリックパラメータで表すため、どの層でも、どの
/// 「集約 ⇔ 外部表現」の組み合わせにも再利用できる。<typeparamref name="TExternal"/> は
/// アプリケーション層では DTO、インフラストラクチャ層では永続化の受け皿など、層に応じた表現を取る。
/// ドメイン層は具体的な外部表現の型に依存しない。
/// </para>
/// <para>
/// 単独エンティティの 1:1 変換は腐敗防止層(<see cref="Ddd.Domain.Adapters.IDomainBiAdapter{TDto, TDomain}"/>)
/// が担い、本ファクトリは複数の Adapter を統括して「集約(ネストした複数パーツ)としての再構築・分解」に
/// 責務を絞る。この点が Adapter(葉の 1:1 変換)との役割の違いである。
/// </para>
/// </remarks>
/// <typeparam name="TAggregate">集約(ルートエンティティ)の型。</typeparam>
/// <typeparam name="TExternal">集約に対応する外部表現の型(DTO・永続化の受け皿など)。ネストした複数パーツを含みうる。</typeparam>
public interface IFactory<TAggregate, TExternal>
{
    /// 
    /// <summary>外部表現からドメイン集約を合成(再構築)する。
    /// </summary>
    TAggregate Assemble(TExternal external);

    /// <summary>
    /// ドメイン集約を外部表現へ分解する。
    /// </summary>
    TExternal Disassemble(TAggregate domain);
}