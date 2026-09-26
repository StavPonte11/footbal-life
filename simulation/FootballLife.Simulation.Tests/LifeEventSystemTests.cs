using System;
using System.Collections.Generic;
using System.IO;
using FootballLife.Domain;
using FootballLife.Simulation;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class LifeEventSystemTests
    {
        private static readonly DateOnly TestDate = new DateOnly(2026, 9, 1);

        private static (Player Player, PlayerState State, PlayerCareerState Career, WorldState World) CreateTestContext(
            DateOnly? dob = null,
            float fatigue = 20f,
            decimal salary = 1000m,
            float managerTrust = 60f)
        {
            var playerId = Guid.NewGuid();
            var clubId = Guid.NewGuid();

            var player = new Player(
                playerId,
                "Marcus Sterling",
                "England",
                dob ?? new DateOnly(2005, 5, 15), // ~21 years old in Sep 2026
                Foot.Right,
                Position.ST,
                null);

            var abilities = new PlayerAbilities(60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60);
            var state = PlayerState.Default with { Fatigue = fatigue, Happiness = 70f, Confidence = 60f, Morale = 65f };

            var career = new PlayerCareerState(
                clubId: clubId,
                status: SquadStatus.Rotation,
                managerTrust: managerTrust,
                weeklySalary: salary,
                marketValue: 250_000m,
                reputation: 40f);

            var season = new Season(
                2026,
                TestDate,
                new DateOnly(2027, 5, 30),
                new LeagueTable(Guid.NewGuid(), Array.Empty<LeagueTableRow>()),
                Array.Empty<Matchday>());

            var world = WorldState.CreateEmpty(season)
                .WithPlayer(player, abilities, state, career, account: FinanceAccount.Create(5000m));

            return (player, state, career, world);
        }

        private static LifeEvent CreateSampleEvent(
            string id = "test_ev",
            EventCategory category = EventCategory.Lifestyle,
            EventPreconditions? cond = null,
            int weight = 50,
            int cooldownWeeks = 4)
        {
            var choice1 = new EventChoice("opt_a", "Option A", new[] {
                new EventEffect(EffectTarget.Happiness, 10f, "Boosts happiness"),
                new EventEffect(EffectTarget.Fatigue, -8f, "Rested legs"),
                new EventEffect(EffectTarget.Money, -200f, "Small expense")
            });

            var choice2 = new EventChoice("opt_b", "Option B", new[] {
                new EventEffect(EffectTarget.ManagerTrust, 5f, "Earned trust"),
                new EventEffect(EffectTarget.Confidence, 4f, "Good discipline")
            });

            return new LifeEvent(
                id,
                "Sample Dilemma",
                "Description of dilemma",
                category,
                new[] { choice1, choice2 },
                cond ?? EventPreconditions.Empty,
                weight,
                cooldownWeeks);
        }

        // ─── P1-050: Condition Evaluator Tests ─────────────────────────────────

        [Fact]
        public void LifeEventSystem_HighSalaryEvent_DoesNotTriggerForYouthPlayer()
        {
            var (player, state, career, world) = CreateTestContext(salary: 200m); // £200/wk youth wage
            var ev = CreateSampleEvent("high_salary_ev", cond: new EventPreconditions(minSalary: 2500m));

            bool eligible = LifeEventSystem.EvaluateConditions(ev, player, state, career, world.GetAccount(player.Id), world, TestDate);

            Assert.False(eligible);
        }

        [Fact]
        public void LifeEventSystem_HighSalaryEvent_TriggersWhenSalaryMeetsCondition()
        {
            var (player, state, career, world) = CreateTestContext(salary: 5000m);
            var ev = CreateSampleEvent("high_salary_ev", cond: new EventPreconditions(minSalary: 2500m));

            bool eligible = LifeEventSystem.EvaluateConditions(ev, player, state, career, world.GetAccount(player.Id), world, TestDate);

            Assert.True(eligible);
        }

        [Fact]
        public void LifeEventSystem_ExhaustedEvent_TriggersOnlyWhenFatigued()
        {
            var (player, stateFresh, career, worldFresh) = CreateTestContext(fatigue: 15f);
            var ev = CreateSampleEvent("exhausted_ev", cond: new EventPreconditions(minFatigue: 50f));

            bool eligibleFresh = LifeEventSystem.EvaluateConditions(ev, player, stateFresh, career, worldFresh.GetAccount(player.Id), worldFresh, TestDate);
            Assert.False(eligibleFresh);

            var stateTired = stateFresh with { Fatigue = 65f };
            var worldTired = worldFresh.WithPlayerState(player.Id, stateTired);

            bool eligibleTired = LifeEventSystem.EvaluateConditions(ev, player, stateTired, career, worldTired.GetAccount(player.Id), worldTired, TestDate);
            Assert.True(eligibleTired);
        }

        [Fact]
        public void LifeEventSystem_AgeConditions_EvaluatedAccurately()
        {
            // Player born May 2005 is 21 on Sept 1, 2026
            var (player, state, career, world) = CreateTestContext(dob: new DateOnly(2005, 5, 1));
            var veteranEvent = CreateSampleEvent("veteran_ev", cond: new EventPreconditions(minAge: 30));
            var youngEvent = CreateSampleEvent("young_ev", cond: new EventPreconditions(minAge: 18, maxAge: 23));

            Assert.False(LifeEventSystem.EvaluateConditions(veteranEvent, player, state, career, world.GetAccount(player.Id), world, TestDate));
            Assert.True(LifeEventSystem.EvaluateConditions(youngEvent, player, state, career, world.GetAccount(player.Id), world, TestDate));
        }

        [Fact]
        public void LifeEventSystem_ManagerTrustConditions_EvaluatedAccurately()
        {
            var (player, state, career, world) = CreateTestContext(managerTrust: 45f);
            var highTrustEvent = CreateSampleEvent("trust_ev", cond: new EventPreconditions(minManagerTrust: 70f));
            var lowTrustEvent = CreateSampleEvent("doghouse_ev", cond: new EventPreconditions(maxManagerTrust: 50f));

            Assert.False(LifeEventSystem.EvaluateConditions(highTrustEvent, player, state, career, world.GetAccount(player.Id), world, TestDate));
            Assert.True(LifeEventSystem.EvaluateConditions(lowTrustEvent, player, state, career, world.GetAccount(player.Id), world, TestDate));
        }

        // ─── P1-051: Weighted Selection & Determinism ─────────────────────────

        [Fact]
        public void LifeEventSystem_SelectWeeklyEvent_DeterministicWithSameSeed()
        {
            var (player, state, career, world) = CreateTestContext();
            var pool = new List<LifeEvent>
            {
                CreateSampleEvent("ev_1", weight: 20),
                CreateSampleEvent("ev_2", weight: 50),
                CreateSampleEvent("ev_3", weight: 80),
                CreateSampleEvent("ev_4", weight: 10)
            };

            var rngA = new SimulationRandom(4242);
            var rngB = new SimulationRandom(4242);

            var picksA = new List<string>();
            var picksB = new List<string>();

            for (int i = 0; i < 20; i++)
            {
                var evA = LifeEventSystem.SelectWeeklyEvent(pool, player, world, rngA, TestDate);
                var evB = LifeEventSystem.SelectWeeklyEvent(pool, player, world, rngB, TestDate);

                Assert.NotNull(evA);
                Assert.NotNull(evB);
                picksA.Add(evA!.Id);
                picksB.Add(evB!.Id);
            }

            Assert.Equal(picksA, picksB);
        }

        [Fact]
        public void LifeEventSystem_SelectWeeklyEvent_ReturnsNullWhenNoEligibleEvents()
        {
            var (player, state, career, world) = CreateTestContext(salary: 100m);
            var pool = new List<LifeEvent>
            {
                CreateSampleEvent("impossible_1", cond: new EventPreconditions(minSalary: 10000m)),
                CreateSampleEvent("impossible_2", cond: new EventPreconditions(minAge: 50))
            };

            var rng = new SimulationRandom(123);
            var selected = LifeEventSystem.SelectWeeklyEvent(pool, player, world, rng, TestDate);

            Assert.Null(selected);
        }

        // ─── P1-052: Effect Applicator Tests ──────────────────────────────────

        [Fact]
        public void LifeEventSystem_ChoiceReducesFatigue_AndIncreasesHappiness()
        {
            var (player, state, career, world) = CreateTestContext(fatigue: 40f);
            var ev = CreateSampleEvent();
            var choice = ev.Choices[0]; // happiness +10, fatigue -8, money -200

            var updatedWorld = LifeEventSystem.ApplyChoice(ev, choice, player, world, TestDate);

            var newState = updatedWorld.GetState(player.Id);
            var newAccount = updatedWorld.GetAccount(player.Id);

            Assert.Equal(80f, newState.Happiness); // was 70 + 10 = 80
            Assert.Equal(32f, newState.Fatigue);   // was 40 - 8 = 32
            Assert.Equal(4800m, newAccount.Balance); // was 5000 - 200 = 4800
        }

        [Fact]
        public void LifeEventSystem_FinancialEffect_CreditsOrDebitsFinanceAccount()
        {
            var (player, state, career, world) = CreateTestContext();
            var choice = new EventChoice("bonus", "Commercial bonus", new[] {
                new EventEffect(EffectTarget.Money, 3500f, "Brand bonus credited")
            });
            var ev = CreateSampleEvent();

            var updatedWorld = LifeEventSystem.ApplyChoice(ev, choice, player, world, TestDate);
            var account = updatedWorld.GetAccount(player.Id);

            Assert.Equal(8500m, account.Balance); // was 5000 + 3500 = 8500
            Assert.Equal(TransactionType.Investment, account.History[0].Type);
            Assert.Contains("Brand bonus credited", account.History[0].Description);
        }

        [Fact]
        public void LifeEventSystem_ManagerTrustEffect_MutatesAndClampsTrust()
        {
            var (player, state, career, world) = CreateTestContext(managerTrust: 95f);
            var choiceHugeBoost = new EventChoice("praise", "Praise", new[] {
                new EventEffect(EffectTarget.ManagerTrust, 20f, "Big trust gain")
            });
            var ev = CreateSampleEvent();

            var updatedWorld = LifeEventSystem.ApplyChoice(ev, choiceHugeBoost, player, world, TestDate);
            var updatedCareer = updatedWorld.GetCareerState(player.Id);

            Assert.Equal(100f, updatedCareer.ManagerTrust); // Clamped at 100 max
        }

        // ─── P1-054: Cooldown and Statistical Tests ───────────────────────────

        [Fact]
        public void LifeEventSystem_CooldownObserved_CannotFireSameEventConsecutively()
        {
            var (player, state, career, world) = CreateTestContext();
            var ev = CreateSampleEvent("unique_dilemma", cooldownWeeks: 4);

            // 1. Initially eligible
            Assert.True(LifeEventSystem.EvaluateConditions(ev, player, state, career, world.GetAccount(player.Id), world, TestDate));

            // 2. Apply choice -> sets cooldown for 4 weeks (28 days)
            world = LifeEventSystem.ApplyChoice(ev, ev.Choices[0], player, world, TestDate);

            // 3. 1 week later -> should NOT be eligible
            var oneWeekLater = TestDate.AddDays(7);
            Assert.False(LifeEventSystem.EvaluateConditions(ev, player, state, career, world.GetAccount(player.Id), world, oneWeekLater));

            // 4. 3 weeks later -> still NOT eligible
            var threeWeeksLater = TestDate.AddDays(21);
            Assert.False(LifeEventSystem.EvaluateConditions(ev, player, state, career, world.GetAccount(player.Id), world, threeWeeksLater));

            // 5. 5 weeks later (35 days) -> cooldown expired, eligible again!
            var fiveWeeksLater = TestDate.AddDays(35);
            Assert.True(LifeEventSystem.EvaluateConditions(ev, player, state, career, world.GetAccount(player.Id), world, fiveWeeksLater));
        }

        [Fact]
        public void LifeEventSystem_CleanExpiredCooldowns_PurgesStaleEntries()
        {
            var (player, state, career, world) = CreateTestContext();
            world = world.WithEventCooldown("ev_old", TestDate.AddDays(-7))
                         .WithEventCooldown("ev_future", TestDate.AddDays(14));

            Assert.Equal(2, world.EventCooldowns.Count);

            var cleaned = world.CleanExpiredCooldowns(TestDate);

            Assert.Single(cleaned.EventCooldowns);
            Assert.True(cleaned.EventCooldowns.ContainsKey("ev_future"));
            Assert.False(cleaned.EventCooldowns.ContainsKey("ev_old"));
        }

        [Fact]
        public void LifeEventSystem_StatisticalSpread_Over1000Weeks()
        {
            var (player, state, career, world) = CreateTestContext();
            var pool = new List<LifeEvent>
            {
                CreateSampleEvent("rare_ev", weight: 10),
                CreateSampleEvent("common_ev", weight: 90)
            };

            var rng = new SimulationRandom(777);
            int commonCount = 0;
            int rareCount = 0;

            for (int i = 0; i < 1000; i++)
            {
                var ev = LifeEventSystem.SelectWeeklyEvent(pool, player, world, rng, TestDate);
                if (ev?.Id == "common_ev") commonCount++;
                else if (ev?.Id == "rare_ev") rareCount++;
            }

            // In 1000 draws with weights 90 : 10, common should be approximately ~900 (e.g. 850-950)
            Assert.InRange(commonCount, 850, 950);
            Assert.InRange(rareCount, 50, 150);
        }

        // ─── P1-053: events.json Data File & Loader Tests ─────────────────────

        [Fact]
        public void LifeEventDataLoader_EventsJson_ValidatesAllExpandedEventsSuccessfully()
        {
            string eventsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "content", "data", "events.json");

            if (!File.Exists(eventsPath))
            {
                // Fallback direct path
                eventsPath = Path.GetFullPath(@"content/data/events.json");
            }

            Assert.True(File.Exists(eventsPath), $"events.json file should exist at: {eventsPath}");

            var result = LifeEventDataLoader.LoadFromFile(eventsPath);

            Assert.True(result.IsSuccess, $"Failed to load events.json: {string.Join("; ", result.Errors)}");
            Assert.True(result.Events.Count >= 100, $"Expected at least 100 events, found {result.Events.Count}");

            // Verify all categories represented
            var categories = new HashSet<EventCategory>();
            foreach (var ev in result.Events)
            {
                categories.Add(ev.Category);
                Assert.NotEmpty(ev.Id);
                Assert.NotEmpty(ev.Title);
                Assert.True(ev.Choices.Count >= 2, $"Event {ev.Id} must have at least 2 choices.");
                Assert.True(ev.Weight > 0, $"Event {ev.Id} must have positive weight.");
            }

            Assert.Contains(EventCategory.Lifestyle, categories);
            Assert.Contains(EventCategory.Media, categories);
            Assert.Contains(EventCategory.LockerRoom, categories);
            Assert.Contains(EventCategory.Family, categories);
            Assert.Contains(EventCategory.Commercial, categories);
            Assert.Contains(EventCategory.Training, categories);
        }

        [Fact]
        public void LifeEventDataLoader_InvalidJson_ReportsValidationErrors()
        {
            string invalidJson = @"{
                ""events"": [
                    {
                        ""id"": """",
                        ""title"": ""Missing Choice Event"",
                        ""category"": ""Media"",
                        ""choices"": []
                    }
                ]
            }";

            var result = LifeEventDataLoader.LoadFromJson(invalidJson);

            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
        }
    }
}
