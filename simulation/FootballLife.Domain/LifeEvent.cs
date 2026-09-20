using System;
using System.Collections.Generic;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents a systemic narrative dilemma with preconditions, options, and consequences.
    /// </summary>
    public sealed record LifeEvent
    {
        public string Id { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public EventCategory Category { get; init; }
        public IReadOnlyList<EventChoice> Choices { get; init; }
        public EventPreconditions Conditions { get; init; }
        public int Weight { get; init; }
        public int CooldownWeeks { get; init; }

        public LifeEvent(
            string id,
            string title,
            string description,
            EventCategory category,
            IReadOnlyList<EventChoice> choices,
            EventPreconditions? conditions = null,
            int weight = 50,
            int cooldownWeeks = 4)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Event Id cannot be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Event Title cannot be empty.", nameof(title));
            if (choices == null || choices.Count == 0) throw new ArgumentException("Event must have at least one choice.", nameof(choices));
            if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be positive.");
            if (cooldownWeeks < 0) throw new ArgumentOutOfRangeException(nameof(cooldownWeeks), "CooldownWeeks cannot be negative.");

            Id = id;
            Title = title;
            Description = description ?? string.Empty;
            Category = category;
            Choices = choices;
            Conditions = conditions ?? EventPreconditions.Empty;
            Weight = weight;
            CooldownWeeks = cooldownWeeks;
        }
    }
}
