using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Generates a deterministic round-robin fixture schedule for a league season.
    /// Uses the circle method (Berger table) to ensure every club plays every other club
    /// exactly twice (once at home, once away) across the season.
    /// </summary>
    /// <remarks>
    /// The circle method works by fixing one club at position 0 and rotating the rest
    /// clockwise for each matchday. The second half of the season swaps home/away assignments.
    /// Output is fully deterministic for a given <see cref="SimulationRandom"/> seed.
    /// </remarks>
    public static class FixtureGenerator
    {
        /// <summary>
        /// Generates a complete season fixture list using the round-robin circle method.
        /// </summary>
        /// <param name="league">The league whose clubs will be scheduled.</param>
        /// <param name="world">World state — used to resolve which clubs belong to the league.</param>
        /// <param name="year">Season year (used to construct fixture dates).</param>
        /// <param name="seasonStartDate">The date of the first matchday.</param>
        /// <param name="rng">
        /// Deterministic RNG — used only to randomize the initial club ordering so that
        /// home/away assignment varies per seed. Same seed → same schedule.
        /// </param>
        /// <returns>
        /// A <see cref="Season"/> whose <see cref="Season.Matchdays"/> contains all generated fixtures.
        /// The <see cref="Season.Table"/> is initialized with all clubs at 0 points.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the league has fewer than 2 clubs in the world state.
        /// </exception>
        public static Season Generate(
            League league,
            WorldState world,
            int year,
            DateOnly seasonStartDate,
            SimulationRandom rng)
        {
            if (league is null) throw new ArgumentNullException(nameof(league));
            if (world is null) throw new ArgumentNullException(nameof(world));
            if (rng is null) throw new ArgumentNullException(nameof(rng));

            // Collect all clubs in this league from world state
            var clubIds = new List<Guid>();
            foreach (var club in world.Clubs.Values)
            {
                if (club.LeagueId == league.Id)
                    clubIds.Add(club.Id);
            }

            if (clubIds.Count < 2)
                throw new ArgumentException(
                    $"League '{league.Name}' has fewer than 2 clubs in the world state ({clubIds.Count} found). Cannot generate fixtures.",
                    nameof(league));

            // Randomize initial club order (seed-deterministic)
            Shuffle(clubIds, rng);

            // If odd number of clubs, add a BYE slot (Guid.Empty = no match for the round team)
            bool hasBye = clubIds.Count % 2 != 0;
            if (hasBye)
                clubIds.Add(Guid.Empty);

            int n = clubIds.Count; // must be even now
            int totalRoundsHalf = n - 1;
            int matchesPerRound = n / 2;

            var matchdays = new List<Matchday>();
            DateOnly currentDate = seasonStartDate;

            // First half: generate n-1 matchdays using circle rotation
            for (int round = 0; round < totalRoundsHalf; round++)
            {
                var fixtures = new List<ScheduledMatch>(matchesPerRound);

                for (int match = 0; match < matchesPerRound; match++)
                {
                    Guid home, away;

                    if (match == 0)
                    {
                        // Fixed club at position 0 paired with the club at the midpoint
                        home = clubIds[0];
                        away = clubIds[n / 2];
                    }
                    else
                    {
                        home = clubIds[match];
                        away = clubIds[n - match];
                    }

                    // Skip BYE matches
                    if (home == Guid.Empty || away == Guid.Empty) continue;

                    fixtures.Add(ScheduledMatch.Create(currentDate, home, away, league.Id));
                }

                if (fixtures.Count > 0)
                {
                    matchdays.Add(new Matchday(round + 1, currentDate, fixtures.AsReadOnly()));
                }

                // Rotate all clubs except the fixed first one
                Rotate(clubIds);
                currentDate = currentDate.AddDays(7);
            }

            // Reset club order for second half (swap home/away)
            // Re-build from the original shuffled order (replay shuffle with same rng state is not possible,
            // so we derive second-half from first-half matchdays by swapping H/A)
            var firstHalfMatchdays = new List<Matchday>(matchdays);
            for (int i = 0; i < firstHalfMatchdays.Count; i++)
            {
                var firstHalfDay = firstHalfMatchdays[i];
                var reversedFixtures = new List<ScheduledMatch>(firstHalfDay.Fixtures.Count);

                foreach (var fixture in firstHalfDay.Fixtures)
                {
                    // Swap home and away
                    reversedFixtures.Add(ScheduledMatch.Create(currentDate, fixture.AwayClubId, fixture.HomeClubId, league.Id));
                }

                int secondHalfWeek = totalRoundsHalf + i + 1;
                matchdays.Add(new Matchday(secondHalfWeek, currentDate, reversedFixtures.AsReadOnly()));
                currentDate = currentDate.AddDays(7);
            }

            // Build initial league table
            var allClubIds = new List<Guid>();
            foreach (var club in world.Clubs.Values)
            {
                if (club.LeagueId == league.Id)
                    allClubIds.Add(club.Id);
            }
            var table = LeagueTable.Create(league.Id, allClubIds);

            var endDate = currentDate.AddDays(-1);
            return new Season(year, seasonStartDate, endDate, table, matchdays.AsReadOnly());
        }

        // ─── Private Helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Performs an in-place Fisher-Yates shuffle using the provided RNG.
        /// </summary>
        private static void Shuffle<T>(List<T> list, SimulationRandom rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.NextInt(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        /// <summary>
        /// Rotates all elements at indices [1..n-1] one position to the right (circle method).
        /// Element at index 0 is fixed.
        /// </summary>
        private static void Rotate<T>(List<T> list)
        {
            if (list.Count <= 1) return;
            // Save the last element
            T last = list[list.Count - 1];
            // Shift elements [1..n-2] one position to the right
            for (int i = list.Count - 1; i > 1; i--)
                list[i] = list[i - 1];
            // Place saved last at index 1
            list[1] = last;
        }
    }
}
