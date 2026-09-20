namespace FootballLife.Domain
{
    /// <summary>
    /// Living standards available to a footballer, determining weekly lifestyle expenses and social prestige.
    /// </summary>
    public enum LifestyleTier
    {
        /// <summary>Shared flat, budget living, basic transport (£150/wk).</summary>
        Modest = 0,

        /// <summary>Quality private apartment, reliable new car, regular dining (£500/wk).</summary>
        Comfortable = 1,

        /// <summary>Luxury city centre penthouse, sports coupe, designer clothing (£2,000/wk).</summary>
        Luxurious = 2,

        /// <summary>Gated mansion, supercar collection, luxury travel, VIP services (£8,000/wk).</summary>
        Extravagant = 3,

        /// <summary>Country estate, private jet charters, high-end security, full personal entourage (£25,000/wk).</summary>
        Superstar = 4
    }
}
