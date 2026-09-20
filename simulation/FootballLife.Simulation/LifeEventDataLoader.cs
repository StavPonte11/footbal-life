using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Result container for life event data loading and schema validation.
    /// </summary>
    public sealed class LifeEventLoadResult
    {
        public bool IsSuccess { get; }
        public IReadOnlyList<LifeEvent> Events { get; }
        public IReadOnlyList<ValidationError> Errors { get; }

        private LifeEventLoadResult(bool isSuccess, IReadOnlyList<LifeEvent> events, IReadOnlyList<ValidationError> errors)
        {
            IsSuccess = isSuccess;
            Events = events;
            Errors = errors;
        }

        public static LifeEventLoadResult Success(IReadOnlyList<LifeEvent> events) =>
            new LifeEventLoadResult(true, events, Array.Empty<ValidationError>());

        public static LifeEventLoadResult Failure(IReadOnlyList<ValidationError> errors) =>
            new LifeEventLoadResult(false, Array.Empty<LifeEvent>(), errors);
    }

    /// <summary>
    /// Loads and validates narrative life events from JSON.
    /// </summary>
    public static class LifeEventDataLoader
    {
        public static LifeEventLoadResult LoadFromJson(string json)
        {
            var errors = new List<ValidationError>();
            var events = new List<LifeEvent>();

            if (string.IsNullOrWhiteSpace(json))
            {
                errors.Add(new ValidationError("Events", "json", "Event JSON string is null or empty."));
                return LifeEventLoadResult.Failure(errors);
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("events", out var eventsArr) || eventsArr.ValueKind != JsonValueKind.Array)
                {
                    errors.Add(new ValidationError("Events", "root", "Missing 'events' array in JSON root."));
                    return LifeEventLoadResult.Failure(errors);
                }

                foreach (var evElem in eventsArr.EnumerateArray())
                {
                    ParseEvent(evElem, events, errors);
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ValidationError("Events", "json", $"JSON parsing exception: {ex.Message}"));
            }

            if (errors.Count > 0)
            {
                return LifeEventLoadResult.Failure(errors.AsReadOnly());
            }

            return LifeEventLoadResult.Success(events.AsReadOnly());
        }

        public static LifeEventLoadResult LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return LifeEventLoadResult.Failure(new[] {
                    new ValidationError("Events", "file", $"File not found: {filePath}")
                });
            }

            string json = File.ReadAllText(filePath);
            return LoadFromJson(json);
        }

        private static void ParseEvent(JsonElement elem, List<LifeEvent> events, List<ValidationError> errors)
        {
            string id = elem.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";
            if (string.IsNullOrWhiteSpace(id))
            {
                errors.Add(new ValidationError("Events", "id", "Event id is missing or empty."));
                return;
            }

            string title = elem.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "" : "";
            if (string.IsNullOrWhiteSpace(title))
            {
                errors.Add(new ValidationError("Events", $"{id}.title", "Event title is missing or empty."));
            }

            string description = elem.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? "" : "";

            string catStr = elem.TryGetProperty("category", out var catProp) ? catProp.GetString() ?? "" : "";
            if (!Enum.TryParse<EventCategory>(catStr, true, out var category))
            {
                errors.Add(new ValidationError("Events", $"{id}.category", $"Invalid EventCategory: '{catStr}'."));
            }

            int weight = elem.TryGetProperty("weight", out var weightProp) ? weightProp.GetInt32() : 50;
            if (weight <= 0)
            {
                errors.Add(new ValidationError("Events", $"{id}.weight", "Weight must be positive."));
            }

            int cooldownWeeks = elem.TryGetProperty("cooldownWeeks", out var coolProp) ? coolProp.GetInt32() : 4;
            if (cooldownWeeks < 0)
            {
                errors.Add(new ValidationError("Events", $"{id}.cooldownWeeks", "CooldownWeeks cannot be negative."));
            }

            // Parse Preconditions
            EventPreconditions conditions = EventPreconditions.Empty;
            if (elem.TryGetProperty("conditions", out var condElem) && condElem.ValueKind == JsonValueKind.Object)
            {
                int? minAge = condElem.TryGetProperty("minAge", out var p) ? p.GetInt32() : null;
                int? maxAge = condElem.TryGetProperty("maxAge", out p) ? p.GetInt32() : null;
                float? minFatigue = condElem.TryGetProperty("minFatigue", out p) ? (float)p.GetDouble() : null;
                float? maxFatigue = condElem.TryGetProperty("maxFatigue", out p) ? (float)p.GetDouble() : null;
                decimal? minSalary = condElem.TryGetProperty("minSalary", out p) ? p.GetDecimal() : null;
                decimal? maxSalary = condElem.TryGetProperty("maxSalary", out p) ? p.GetDecimal() : null;
                float? minTrust = condElem.TryGetProperty("minManagerTrust", out p) ? (float)p.GetDouble() : null;
                float? maxTrust = condElem.TryGetProperty("maxManagerTrust", out p) ? (float)p.GetDouble() : null;

                conditions = new EventPreconditions(
                    minAge: minAge,
                    maxAge: maxAge,
                    minFatigue: minFatigue,
                    maxFatigue: maxFatigue,
                    minSalary: minSalary,
                    maxSalary: maxSalary,
                    minManagerTrust: minTrust,
                    maxManagerTrust: maxTrust);
            }

            // Parse Choices
            var choices = new List<EventChoice>();
            if (!elem.TryGetProperty("choices", out var choicesArr) || choicesArr.ValueKind != JsonValueKind.Array)
            {
                errors.Add(new ValidationError("Events", $"{id}.choices", "Missing 'choices' array."));
            }
            else
            {
                foreach (var choiceElem in choicesArr.EnumerateArray())
                {
                    string choiceId = choiceElem.TryGetProperty("id", out var cid) ? cid.GetString() ?? "" : "";
                    string choiceText = choiceElem.TryGetProperty("text", out var ctext) ? ctext.GetString() ?? "" : "";

                    var effects = new List<EventEffect>();
                    if (choiceElem.TryGetProperty("effects", out var effArr) && effArr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var effElem in effArr.EnumerateArray())
                        {
                            string targetStr = effElem.TryGetProperty("target", out var tgtProp) ? tgtProp.GetString() ?? "" : "";
                            if (!Enum.TryParse<EffectTarget>(targetStr, true, out var target))
                            {
                                errors.Add(new ValidationError("Events", $"{id}.{choiceId}.target", $"Invalid EffectTarget: '{targetStr}'."));
                                continue;
                            }

                            float delta = effElem.TryGetProperty("delta", out var deltaProp) ? (float)deltaProp.GetDouble() : 0f;
                            string effDesc = effElem.TryGetProperty("description", out var edesc) ? edesc.GetString() ?? "" : "";

                            effects.Add(new EventEffect(target, delta, effDesc));
                        }
                    }

                    if (string.IsNullOrWhiteSpace(choiceId))
                        errors.Add(new ValidationError("Events", $"{id}.choice.id", "Choice ID is empty."));

                    if (string.IsNullOrWhiteSpace(choiceText))
                        errors.Add(new ValidationError("Events", $"{id}.choice.text", "Choice text is empty."));

                    choices.Add(new EventChoice(choiceId, choiceText, effects.AsReadOnly()));
                }
            }

            if (choices.Count < 2)
            {
                errors.Add(new ValidationError("Events", $"{id}.choices", "Event must have at least two choices for a valid dilemma."));
            }

            if (errors.Count == 0)
            {
                events.Add(new LifeEvent(id, title, description, category, choices.AsReadOnly(), conditions, weight, cooldownWeeks));
            }
        }
    }
}
