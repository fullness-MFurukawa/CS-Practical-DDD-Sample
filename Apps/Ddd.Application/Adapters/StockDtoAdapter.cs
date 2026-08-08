// Apps/Ddd.Application/Adapters/StockDtoAdapter.cs
using Ddd.Application.Dtos;
using Ddd.Application.Exceptions;
using Ddd.Domain.Adapters;
using Ddd.Domain.Models.Stocks;

namespace Ddd.Application.Adapters;

/// <summary>
/// <see cref="Stock"/> エンティティと <see cref="StockDto"/> の相互変換を行う
/// アプリケーション層のアダプタ。
/// </summary>
/// <remarks>
/// ドメイン層の <see cref="IDomainBiAdapter{TDto, TDomain}"/> を実装する。
/// ID が未指定なら <see cref="Stock.CreateNew(StockQuantity)"/>(新規採番)、
/// 指定済みなら <see cref="Stock.Restore(StockId, StockQuantity)"/>(復元)を用いる。
/// 在庫数の欠落は <see cref="InvalidInputException"/> として弾き、範囲(0〜100)の検証は
/// <see cref="StockQuantity"/> が担う。
/// </remarks>
public sealed class StockDtoAdapter : IDomainBiAdapter<StockDto, Stock>
{
    /// <summary>
    /// <see cref="StockDto"/> から <see cref="Stock"/> エンティティを再構築する。
    /// </summary>
    /// <param name="input">在庫 DTO。</param>
    /// <returns>在庫エンティティ。</returns>
    /// <exception cref="InvalidInputException">DTO が null、または在庫数が未指定の場合。</exception>
    public Stock ToDomain(StockDto input)
    {
        if (input is null)
        {
            throw new InvalidInputException("StockDtoがnullです。");
        }
        if (input.Quantity is null)
        {
            throw new InvalidInputException("在庫数は必須です。");
        }
        if (string.IsNullOrWhiteSpace(input.Id))
        {
            return Stock.CreateNew(StockQuantity.Create(input.Quantity.Value));
        }
        return Stock.Restore(StockId.Parse(input.Id), StockQuantity.Create(input.Quantity.Value));
    }

    /// <summary>
    /// <see cref="Stock"/> エンティティを <see cref="StockDto"/> に変換する。
    /// </summary>
    /// <param name="domain">在庫エンティティ。</param>
    /// <returns>在庫 DTO。</returns>
    /// <exception cref="InvalidInputException">引数が null の場合。</exception>
    public StockDto FromDomain(Stock domain)
    {
        if (domain is null)
        {
            throw new InvalidInputException("Stockがnullです。");
        }
        return new StockDto(domain.StockId.ToString(), domain.Quantity.Value);
    }
}