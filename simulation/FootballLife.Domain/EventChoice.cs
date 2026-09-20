using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents an actionable choice presented to the player during a life event.
    /// </summary>
    public sealed record EventChoice
    {
        public string Id { get; init; }
        public string Text { get; init; }
        public IReadOnlyList<EventEffect> Effects { get; init; }

        public EventChoice(string id, string text, IReadOnlyList<EventEffect> effects)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Choice Id cannot be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Choice Text cannot be empty.", nameof(text));
            Id = id;
            Text = text;
            Effects = effects ?? Array.Empty<EventEffect>();
        }
    }
}
