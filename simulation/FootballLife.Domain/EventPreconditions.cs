namespace FootballLife.Domain
{
    /// <summary>
    /// Preconditions that must all be satisfied for a life event to be eligible to trigger.
    /// Null fields indicate no constraint.
    /// </summary>
    public sealed record EventPreconditions
    {
        public int? MinAge { get; init; }
        public int? MaxAge { get; init; }
        public float? MinFatigue { get; init; }
        public float? MaxFatigue { get; init; }
        public decimal? MinSalary { get; init; }
        public decimal? MaxSalary { get; init; }
        public float? MinManagerTrust { get; init; }
        public float? MaxManagerTrust { get; init; }
        public LifestyleTier? MinLifestyleTier { get; init; }

        public static EventPreconditions Empty { get; } = new();

        public EventPreconditions(
            int? minAge = null,
            int? maxAge = null,
            float? minFatigue = null,
            float? maxFatigue = null,
            decimal? minSalary = null,
            decimal? maxSalary = null,
            float? minManagerTrust = null,
            float? maxManagerTrust = null,
            LifestyleTier? minLifestyleTier = null)
        {
            MinAge = minAge;
            MaxAge = maxAge;
            MinFatigue = minFatigue;
            MaxFatigue = maxFatigue;
            MinSalary = minSalary;
            MaxSalary = maxSalary;
            MinManagerTrust = minManagerTrust;
            MaxManagerTrust = maxManagerTrust;
            MinLifestyleTier = minLifestyleTier;
        }
    }
}
