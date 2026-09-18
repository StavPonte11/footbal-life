namespace FootballLife.Simulation
{
    /// <summary>
    /// Details a specific validation violation encountered when reading game content datasets.
    /// </summary>
    public sealed record ValidationError
    {
        public string Entity { get; init; }
        public string Field { get; init; }
        public string Message { get; init; }
        public string? RawValue { get; init; }

        public ValidationError(string entity, string field, string message, string? rawValue = null)
        {
            Entity = entity;
            Field = field;
            Message = message;
            RawValue = rawValue;
        }

        public override string ToString() => $"[{Entity}.{Field}] {Message} (Value: {RawValue ?? "null"})";
    }
}
