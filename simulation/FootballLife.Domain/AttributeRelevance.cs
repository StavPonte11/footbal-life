using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents an evaluated attribute relevance mapping for a position and optional context.
    /// </summary>
    public sealed record AttributeRelevance
    {
        public Position Position { get; init; }
        public SituationType? Situation { get; init; }
        public IReadOnlyList<(AttributeName Attribute, float Weight)> WeightedAttributes { get; init; }

        public AttributeRelevance(
            Position position,
            SituationType? situation,
            IReadOnlyList<(AttributeName Attribute, float Weight)> weightedAttributes)
        {
            Position = position;
            Situation = situation;
            WeightedAttributes = weightedAttributes;
        }
    }
}
