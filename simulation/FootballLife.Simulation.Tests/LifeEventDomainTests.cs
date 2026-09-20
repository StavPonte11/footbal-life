using System;
using System.Collections.Generic;
using System.Text.Json;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class LifeEventDomainTests
    {
        [Fact]
        public void LifeEvent_ConstructsWithValidArguments()
        {
            var choice1 = new EventChoice("c1", "Choice 1", new[] { new EventEffect(EffectTarget.Happiness, 10f, "Happy") });
            var choice2 = new EventChoice("c2", "Choice 2", new[] { new EventEffect(EffectTarget.Fatigue, -5f, "Rest") });
            var conditions = new EventPreconditions(minAge: 18, maxAge: 35);

            var ev = new LifeEvent(
                id: "test_event",
                title: "Test Dilemma",
                description: "Description of dilemma",
                category: EventCategory.Media,
                choices: new[] { choice1, choice2 },
                conditions: conditions,
                weight: 80,
                cooldownWeeks: 4);

            Assert.Equal("test_event", ev.Id);
            Assert.Equal("Test Dilemma", ev.Title);
            Assert.Equal("Description of dilemma", ev.Description);
            Assert.Equal(EventCategory.Media, ev.Category);
            Assert.Equal(2, ev.Choices.Count);
            Assert.Equal(18, ev.Conditions.MinAge);
            Assert.Equal(35, ev.Conditions.MaxAge);
            Assert.Equal(80, ev.Weight);
            Assert.Equal(4, ev.CooldownWeeks);
        }

        [Fact]
        public void LifeEvent_ThrowsOnInvalidArguments()
        {
            var choice = new EventChoice("c1", "Text", Array.Empty<EventEffect>());

            Assert.Throws<ArgumentException>(() =>
                new LifeEvent("", "Title", "Desc", EventCategory.Lifestyle, new[] { choice }));

            Assert.Throws<ArgumentException>(() =>
                new LifeEvent("id", "", "Desc", EventCategory.Lifestyle, new[] { choice }));

            Assert.Throws<ArgumentException>(() =>
                new LifeEvent("id", "Title", "Desc", EventCategory.Lifestyle, Array.Empty<EventChoice>()));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new LifeEvent("id", "Title", "Desc", EventCategory.Lifestyle, new[] { choice }, weight: 0));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new LifeEvent("id", "Title", "Desc", EventCategory.Lifestyle, new[] { choice }, cooldownWeeks: -1));
        }

        [Fact]
        public void EventChoice_ThrowsOnEmptyIdOrText()
        {
            Assert.Throws<ArgumentException>(() => new EventChoice("", "Text", Array.Empty<EventEffect>()));
            Assert.Throws<ArgumentException>(() => new EventChoice("id", "", Array.Empty<EventEffect>()));
        }

        [Fact]
        public void LifeEvent_Serialization_RoundTripsFromJson()
        {
            var choice1 = new EventChoice("c1", "Option A", new[] {
                new EventEffect(EffectTarget.Happiness, 15f, "Boosts happiness"),
                new EventEffect(EffectTarget.Money, -500f, "Costs £500")
            });
            var choice2 = new EventChoice("c2", "Option B", new[] {
                new EventEffect(EffectTarget.Confidence, -5f, "Doubt creeps in")
            });

            var original = new LifeEvent(
                id: "roundtrip_event",
                title: "Roundtrip Test",
                description: "Testing roundtrip JSON",
                category: EventCategory.Commercial,
                choices: new[] { choice1, choice2 },
                conditions: new EventPreconditions(minSalary: 1000m, minManagerTrust: 50f),
                weight: 60,
                cooldownWeeks: 6);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(original, options);

            var deserialized = JsonSerializer.Deserialize<LifeEvent>(json, options);

            Assert.NotNull(deserialized);
            Assert.Equal(original.Id, deserialized!.Id);
            Assert.Equal(original.Title, deserialized.Title);
            Assert.Equal(original.Category, deserialized.Category);
            Assert.Equal(original.Weight, deserialized.Weight);
            Assert.Equal(original.CooldownWeeks, deserialized.CooldownWeeks);
            Assert.Equal(original.Conditions.MinSalary, deserialized.Conditions.MinSalary);
            Assert.Equal(original.Conditions.MinManagerTrust, deserialized.Conditions.MinManagerTrust);
            Assert.Equal(original.Choices.Count, deserialized.Choices.Count);
            Assert.Equal(original.Choices[0].Effects.Count, deserialized.Choices[0].Effects.Count);
            Assert.Equal(original.Choices[0].Effects[0].Target, deserialized.Choices[0].Effects[0].Target);
            Assert.Equal(original.Choices[0].Effects[0].Delta, deserialized.Choices[0].Effects[0].Delta);
        }
    }
}
