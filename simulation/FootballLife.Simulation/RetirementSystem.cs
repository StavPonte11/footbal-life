using System;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# simulation system managing veteran player aging, physical attribute decline,
    /// contract wind-downs, and voluntary or forced retirement choices.
    /// </summary>
    public sealed class RetirementSystem
    {
        public const int MinimumRetirementAge = 32;
        public const int MandatoryRetirementAge = 40;

        /// <summary>
        /// Determines whether a player is eligible to consider retirement.
        /// </summary>
        public bool IsEligibleForRetirement(int playerAge)
        {
            return playerAge >= MinimumRetirementAge;
        }

        /// <summary>
        /// Simulates late-career physical decline for players aged 32+.
        /// Physical attributes (pace, acceleration, stamina, agility) degrade progressively,
        /// while technical and mental attributes (vision, composure, decision making) remain resilient.
        /// </summary>
        public PlayerAbilities ApplyLateCareerDecline(
            PlayerAbilities abilities,
            int playerAge,
            SimulationRandom random)
        {
            if (abilities is null) throw new ArgumentNullException(nameof(abilities));
            if (random is null) throw new ArgumentNullException(nameof(random));

            if (playerAge < MinimumRetirementAge)
            {
                return abilities;
            }

            int paceDrop;
            int accelDrop;
            int staminaDrop;
            int agilityDrop;
            int strengthDrop;
            int mentalDrop;

            if (playerAge < 34)
            {
                // Mild early-veteran decline (32-33)
                paceDrop = random.NextInt(1, 3);
                accelDrop = random.NextInt(1, 3);
                staminaDrop = random.NextInt(2, 4);
                agilityDrop = random.NextInt(1, 2);
                strengthDrop = random.NextInt(0, 2);
                mentalDrop = random.NextInt(0, 1);
            }
            else if (playerAge < 37)
            {
                // Moderate mid-veteran decline (34-36)
                paceDrop = random.NextInt(2, 5);
                accelDrop = random.NextInt(2, 5);
                staminaDrop = random.NextInt(3, 6);
                agilityDrop = random.NextInt(1, 3);
                strengthDrop = random.NextInt(1, 3);
                mentalDrop = random.NextInt(0, 2);
            }
            else
            {
                // Severe late-veteran decline (37+)
                paceDrop = random.NextInt(3, 6);
                accelDrop = random.NextInt(3, 6);
                staminaDrop = random.NextInt(4, 7);
                agilityDrop = random.NextInt(2, 4);
                strengthDrop = random.NextInt(2, 4);
                mentalDrop = random.NextInt(0, 2);
            }

            return abilities with
            {
                Pace = (byte)Math.Max(PlayerAbilities.MinValue, abilities.Pace - paceDrop),
                Acceleration = (byte)Math.Max(PlayerAbilities.MinValue, abilities.Acceleration - accelDrop),
                Stamina = (byte)Math.Max(PlayerAbilities.MinValue, abilities.Stamina - staminaDrop),
                Agility = (byte)Math.Max(PlayerAbilities.MinValue, abilities.Agility - agilityDrop),
                Strength = (byte)Math.Max(PlayerAbilities.MinValue, abilities.Strength - strengthDrop),
                Vision = (byte)Math.Max(PlayerAbilities.MinValue, abilities.Vision - mentalDrop),
                Composure = (byte)Math.Max(PlayerAbilities.MinValue, abilities.Composure - mentalDrop),
                DecisionMaking = (byte)Math.Max(PlayerAbilities.MinValue, abilities.DecisionMaking - mentalDrop)
            };
        }

        /// <summary>
        /// Calculates the maximum contract length (in years) clubs are willing to offer veteran players.
        /// </summary>
        public int EvaluateContractMaxYears(int playerAge)
        {
            if (playerAge >= 35) return 1;
            if (playerAge >= 33) return 2;
            if (playerAge >= 31) return 3;
            return 4;
        }

        /// <summary>
        /// Generates a formal retirement decision record and public farewell statement.
        /// </summary>
        public RetirementDecision RetirePlayer(
            Player player,
            int season,
            int currentAge,
            RetirementReason reason,
            PostPlayingRole chosenRole)
        {
            if (player is null) throw new ArgumentNullException(nameof(player));

            string statement = FormatFarewellStatement(player.Name, currentAge, season, reason, chosenRole);

            return new RetirementDecision(
                playerId: player.Id,
                isRetired: true,
                retirementAge: currentAge,
                retirementSeason: season,
                reason: reason,
                chosenRole: chosenRole,
                statement: statement);
        }

        private static string FormatFarewellStatement(
            string playerName,
            int age,
            int season,
            RetirementReason reason,
            PostPlayingRole role)
        {
            string roleDescription = role switch
            {
                PostPlayingRole.Manager => "stepping immediately into senior football management",
                PostPlayingRole.AcademyCoach => "returning to grass-roots football to nurture the next generation of youth talent",
                PostPlayingRole.TVPundit => "joining television broadcast networks as a tactical studio analyst",
                PostPlayingRole.ClubAmbassador => "serving as an international club ambassador and community figurehead",
                PostPlayingRole.PrivateLife => "enjoying well-deserved peace, family life, and private ventures",
                _ => "embarking on a new chapter outside the white lines"
            };

            string reasonText = reason switch
            {
                RetirementReason.VoluntaryAtPeak => "standing on top of the mountain and finishing on my own terms",
                RetirementReason.TrophyCabinetComplete => "having achieved every footballing dream I set out as a boy",
                RetirementReason.ContractExpired => "with my contract reaching its natural conclusion and time for reflection",
                RetirementReason.MajorInjury => "after consulting medical teams regarding the physical toll of elite sport",
                _ => "listening to my body after years of top-flight competition"
            };

            return $"Official Statement from {playerName}: After {age} years of age and {season} professional seasons, I am announcing my retirement from professional football. {reasonText}. I look forward to {roleDescription}. Thank you to all the fans, teammates, and staff who supported this journey.";
        }
    }
}
