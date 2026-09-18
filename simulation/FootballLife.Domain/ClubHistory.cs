using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Encapsulates the historical legacy, founding heritage, and trophy honours of a club.
    /// Contributes to institutional prestige, media narratives, and veteran player career aspirations.
    /// </summary>
    public sealed record ClubHistory
    {
        public int FoundedYear { get; init; }
        public int LeagueTitles { get; init; }
        public int DomesticCups { get; init; }
        public int ContinentalTrophies { get; init; }

        public ClubHistory(int foundedYear, int leagueTitles = 0, int domesticCups = 0, int continentalTrophies = 0)
        {
            if (foundedYear < 1850 || foundedYear > 2100)
            {
                throw new ArgumentOutOfRangeException(nameof(foundedYear), $"Founded year {foundedYear} must be realistic [1850–2100].");
            }

            if (leagueTitles < 0) throw new ArgumentOutOfRangeException(nameof(leagueTitles), "League titles cannot be negative.");
            if (domesticCups < 0) throw new ArgumentOutOfRangeException(nameof(domesticCups), "Domestic cups cannot be negative.");
            if (continentalTrophies < 0) throw new ArgumentOutOfRangeException(nameof(continentalTrophies), "Continental trophies cannot be negative.");

            FoundedYear = foundedYear;
            LeagueTitles = leagueTitles;
            DomesticCups = domesticCups;
            ContinentalTrophies = continentalTrophies;
        }

        public int TotalMajorTrophies => LeagueTitles + DomesticCups + ContinentalTrophies;

        public static ClubHistory Default => new ClubHistory(1900, 0, 0, 0);
    }
}
