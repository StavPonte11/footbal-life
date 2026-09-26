using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Category classification for purchasable monetization offerings.
    /// </summary>
    public enum MonetizationCategory
    {
        Cosmetic = 0,
        ConvenienceToken = 1,
        CareerReplay = 2
    }

    /// <summary>
    /// Definition of an in-app product or cosmetic item.
    /// </summary>
    public sealed record MonetizationProduct(
        string ProductId,
        string Title,
        string Description,
        MonetizationCategory Category,
        decimal PriceUsd,
        int Quantity = 1
    );
}
