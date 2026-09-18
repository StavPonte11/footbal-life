using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class PositionWeightMapTests
    {
        // ─── Coverage: all 9 positions return a valid dictionary ──────────────

        [Theory]
        [InlineData(Position.GK)]
        [InlineData(Position.CB)]
        [InlineData(Position.FB)]
        [InlineData(Position.DM)]
        [InlineData(Position.CM)]
        [InlineData(Position.AM)]
        [InlineData(Position.LW)]
        [InlineData(Position.RW)]
        [InlineData(Position.ST)]
        public void For_AllPositions_Returns15Entries(Position position)
        {
            var weights = PositionWeightMap.For(position);
            Assert.Equal(15, weights.Count);
        }

        [Theory]
        [InlineData(Position.GK)]
        [InlineData(Position.CB)]
        [InlineData(Position.FB)]
        [InlineData(Position.DM)]
        [InlineData(Position.CM)]
        [InlineData(Position.AM)]
        [InlineData(Position.LW)]
        [InlineData(Position.RW)]
        [InlineData(Position.ST)]
        public void For_AllPositions_AllWeightsInValidRange(Position position)
        {
            var weights = PositionWeightMap.For(position);

            foreach (var kvp in weights)
            {
                Assert.True(kvp.Value >= 0f && kvp.Value <= 1f,
                    $"Position {position}, attribute {kvp.Key}: weight {kvp.Value} out of [0,1].");
            }
        }

        [Theory]
        [InlineData(Position.GK)]
        [InlineData(Position.CB)]
        [InlineData(Position.FB)]
        [InlineData(Position.DM)]
        [InlineData(Position.CM)]
        [InlineData(Position.AM)]
        [InlineData(Position.LW)]
        [InlineData(Position.RW)]
        [InlineData(Position.ST)]
        public void For_AllPositions_ContainsAllAttributeNames(Position position)
        {
            var weights = PositionWeightMap.For(position);

            foreach (AttributeName attr in Enum.GetValues(typeof(AttributeName)))
            {
                Assert.True(weights.ContainsKey(attr),
                    $"Position {position} is missing attribute {attr}.");
            }
        }

        // ─── Acceptance Criteria from Issue #7 ───────────────────────────────

        [Fact]
        public void PositionWeightMap_Striker_HasHighShootingWeight()
        {
            float shooting = PositionWeightMap.GetWeight(Position.ST, AttributeName.Shooting);
            float pace = PositionWeightMap.GetWeight(Position.ST, AttributeName.Pace);
            float tackling = PositionWeightMap.GetWeight(Position.ST, AttributeName.Tackling);

            Assert.True(shooting >= 0.9f, $"Striker shooting weight should be >= 0.9, was {shooting}");
            Assert.True(pace >= 0.8f, $"Striker pace weight should be >= 0.8, was {pace}");
            Assert.True(tackling <= 0.2f, $"Striker tackling weight should be <= 0.2, was {tackling}");
        }

        [Fact]
        public void PositionWeightMap_Goalkeeper_HasHighPositioningWeight()
        {
            float positioning = PositionWeightMap.GetWeight(Position.GK, AttributeName.Positioning);
            float composure = PositionWeightMap.GetWeight(Position.GK, AttributeName.Composure);

            Assert.True(positioning >= 0.9f, $"GK positioning weight should be >= 0.9, was {positioning}");
            Assert.True(composure >= 0.85f, $"GK composure weight should be >= 0.85, was {composure}");
        }

        [Fact]
        public void PositionWeightMap_Defender_HasHighTacklingAndLowShooting()
        {
            float tackling = PositionWeightMap.GetWeight(Position.CB, AttributeName.Tackling);
            float positioning = PositionWeightMap.GetWeight(Position.CB, AttributeName.Positioning);
            float shooting = PositionWeightMap.GetWeight(Position.CB, AttributeName.Shooting);

            Assert.True(tackling >= 0.9f, $"Defender tackling weight should be >= 0.9, was {tackling}");
            Assert.True(positioning >= 0.85f, $"Defender positioning weight should be >= 0.85, was {positioning}");
            Assert.True(shooting <= 0.15f, $"Defender shooting weight should be <= 0.15, was {shooting}");
        }

        [Theory]
        [InlineData(Position.GK)]
        [InlineData(Position.CB)]
        [InlineData(Position.FB)]
        [InlineData(Position.DM)]
        [InlineData(Position.CM)]
        [InlineData(Position.AM)]
        [InlineData(Position.LW)]
        [InlineData(Position.RW)]
        [InlineData(Position.ST)]
        public void PositionWeightMap_AllPositions_WeightsSumToOne(Position position)
        {
            Assert.True(PositionWeightMap.WeightsSumToOne(position),
                $"Normalized weights for position {position} do not sum to 1.0.");
        }

        [Fact]
        public void TryLoadFromFile_WithValidJson_LoadsSuccessfully()
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "content", "data", "positions.json");
            if (File.Exists(jsonPath))
            {
                bool loaded = PositionWeightMap.TryLoadFromFile(jsonPath);
                Assert.True(loaded);
            }
        }

        // ─── Semantic spot-checks ─────────────────────────────────────────────

        [Fact]
        public void GK_Shooting_HasLowWeight()
        {
            var weights = PositionWeightMap.For(Position.GK);
            Assert.True(weights[AttributeName.Shooting] <= 0.10f);
        }

        [Fact]
        public void Winger_Pace_HasMaximumWeight()
        {
            var lwWeights = PositionWeightMap.For(Position.LW);
            var rwWeights = PositionWeightMap.For(Position.RW);

            Assert.Equal(1.00f, lwWeights[AttributeName.Pace]);
            Assert.Equal(1.00f, rwWeights[AttributeName.Pace]);
        }

        [Fact]
        public void LW_And_RW_HaveIdenticalWeights()
        {
            var lwWeights = PositionWeightMap.For(Position.LW);
            var rwWeights = PositionWeightMap.For(Position.RW);

            foreach (AttributeName attr in Enum.GetValues(typeof(AttributeName)))
            {
                Assert.Equal(lwWeights[attr], rwWeights[attr]);
            }
        }

        [Fact]
        public void For_InvalidPosition_ThrowsArgumentOutOfRangeException()
        {
            var invalidPosition = (Position)99;
            Assert.Throws<ArgumentOutOfRangeException>(() => PositionWeightMap.For(invalidPosition));
        }
    }

    public class AttributeRelevanceMatrixTests
    {
        private static PlayerAbilities MakeAbilities(int uniform) =>
            PlayerAbilities.CreateUniform(uniform);

        // ─── Acceptance Criteria from Issue #8 ───────────────────────────────

        [Fact]
        public void AttributeRelevance_Midfielder_PassingIsHighlyRelevant()
        {
            var relevance = AttributeRelevanceMatrix.Get(Position.CM);

            Assert.Equal(Position.CM, relevance.Position);
            Assert.Null(relevance.Situation);

            var passingEntry = relevance.WeightedAttributes.FirstOrDefault(x => x.Attribute == AttributeName.Passing);
            Assert.True(passingEntry.Weight >= 0.9f, $"CM passing relevance weight should be >= 0.9, was {passingEntry.Weight}");
        }

        [Fact]
        public void AttributeRelevance_Defender_TacklingIsHighlyRelevant()
        {
            var relevance = AttributeRelevanceMatrix.Get(Position.CB);

            Assert.Equal(Position.CB, relevance.Position);
            var tacklingEntry = relevance.WeightedAttributes.FirstOrDefault(x => x.Attribute == AttributeName.Tackling);
            Assert.True(tacklingEntry.Weight >= 0.9f, $"CB tackling relevance weight should be >= 0.9, was {tacklingEntry.Weight}");
        }

        [Fact]
        public void AttributeRelevance_SituationModifiers_ApplyCorrectly()
        {
            var openPlay = AttributeRelevanceMatrix.Get(Position.LW, SituationType.OpenPlay);
            var counter = AttributeRelevanceMatrix.Get(Position.LW, SituationType.CounterAttack);

            Assert.Equal(SituationType.CounterAttack, counter.Situation);
            Assert.NotNull(counter.WeightedAttributes);
        }

        // ─── Basic scoring properties ─────────────────────────────────────────

        [Theory]
        [InlineData(Position.GK)]
        [InlineData(Position.CB)]
        [InlineData(Position.FB)]
        [InlineData(Position.DM)]
        [InlineData(Position.CM)]
        [InlineData(Position.AM)]
        [InlineData(Position.LW)]
        [InlineData(Position.RW)]
        [InlineData(Position.ST)]
        public void ComputeScore_WithUniformAbilities_ReturnsScoreInValidRange(Position position)
        {
            var abilities = MakeAbilities(70);
            float score = AttributeRelevanceMatrix.ComputeScore(abilities, position);

            Assert.True(score >= 0f && score <= 1f,
                $"Score for {position} with uniform abilities is out of [0,1]: {score}");
        }

        [Fact]
        public void ComputeScore_MaxAbilities_ReturnsScoreNearOne()
        {
            var abilities = MakeAbilities(100);

            foreach (Position pos in Enum.GetValues(typeof(Position)))
            {
                float score = AttributeRelevanceMatrix.ComputeScore(abilities, pos);
                Assert.InRange(score, 0.99f, 1.0f);
            }
        }

        [Fact]
        public void ComputeScore_ZeroAbilities_ReturnsZero()
        {
            var abilities = MakeAbilities(0);

            foreach (Position pos in Enum.GetValues(typeof(Position)))
            {
                float score = AttributeRelevanceMatrix.ComputeScore(abilities, pos);
                Assert.Equal(0f, score);
            }
        }

        [Fact]
        public void ComputeScore_HigherAbilities_ProducesHigherScore()
        {
            var low  = MakeAbilities(20);
            var high = MakeAbilities(80);

            float lowScore  = AttributeRelevanceMatrix.ComputeScore(low,  Position.ST);
            float highScore = AttributeRelevanceMatrix.ComputeScore(high, Position.ST);

            Assert.True(highScore > lowScore);
        }

        // ─── Semantic positioning fit ─────────────────────────────────────────

        [Fact]
        public void ComputeAllPositions_Returns9Entries()
        {
            var abilities = MakeAbilities(60);
            var scores = AttributeRelevanceMatrix.ComputeAllPositions(abilities);

            Assert.Equal(9, scores.Count);
        }

        [Fact]
        public void BestFitPosition_PaceAndDribbling_ReturnWinger()
        {
            var abilities = new PlayerAbilities(
                pace: 95, acceleration: 95, stamina: 70, strength: 30, agility: 95,
                passing: 55, shooting: 65, dribbling: 95, crossing: 80, firstTouch: 80,
                tackling: 20, vision: 65, composure: 70, positioning: 60, decisionMaking: 70);

            Position best = AttributeRelevanceMatrix.BestFitPosition(abilities);

            Assert.True(best == Position.LW || best == Position.RW,
                $"Expected winger position, got {best}.");
        }

        [Fact]
        public void BestFitPosition_TacklingAndStrength_ReturnCentreBack()
        {
            var abilities = new PlayerAbilities(
                pace: 60, acceleration: 55, stamina: 70, strength: 95, agility: 50,
                passing: 60, shooting: 15, dribbling: 25, crossing: 15, firstTouch: 60,
                tackling: 95, vision: 65, composure: 80, positioning: 90, decisionMaking: 85);

            Position best = AttributeRelevanceMatrix.BestFitPosition(abilities);

            Assert.Equal(Position.CB, best);
        }

        [Fact]
        public void BestFitPosition_ShootingAndPositioning_ReturnStriker()
        {
            var abilities = new PlayerAbilities(
                pace: 85, acceleration: 90, stamina: 70, strength: 75, agility: 75,
                passing: 55, shooting: 95, dribbling: 65, crossing: 25, firstTouch: 85,
                tackling: 10, vision: 60, composure: 95, positioning: 95, decisionMaking: 85);

            Position best = AttributeRelevanceMatrix.BestFitPosition(abilities);

            Assert.Equal(Position.ST, best);
        }

        // ─── Null guards ──────────────────────────────────────────────────────

        [Fact]
        public void ComputeScore_NullAbilities_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                AttributeRelevanceMatrix.ComputeScore(null!, Position.CM));
        }

        [Fact]
        public void ComputeAllPositions_NullAbilities_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                AttributeRelevanceMatrix.ComputeAllPositions(null!));
        }

        [Fact]
        public void BestFitPosition_NullAbilities_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                AttributeRelevanceMatrix.BestFitPosition(null!));
        }

        // ─── Determinism ──────────────────────────────────────────────────────

        [Fact]
        public void ComputeScore_SameInput_ProducesSameOutput()
        {
            var abilities = MakeAbilities(55);
            float s1 = AttributeRelevanceMatrix.ComputeScore(abilities, Position.CM);
            float s2 = AttributeRelevanceMatrix.ComputeScore(abilities, Position.CM);

            Assert.Equal(s1, s2);
        }
    }
}
