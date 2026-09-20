using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents a discrete state modification resulting from a life event choice.
    /// </summary>
    public sealed record EventEffect
    {
        public EffectTarget Target { get; init; }
        public float Delta { get; init; }
        public string Description { get; init; }

        public EventEffect(EffectTarget target, float delta, string description = "")
        {
            Target = target;
            Delta = delta;
            Description = description ?? string.Empty;
        }
    }
}
