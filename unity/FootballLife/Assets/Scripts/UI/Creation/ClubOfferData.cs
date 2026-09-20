using System.Collections.Generic;

namespace FootballLife.Unity.UI.Creation
{
    /// <summary>
    /// Data contract describing an entry-level professional club offer for a rookie footballer.
    /// </summary>
    public sealed class ClubOfferData
    {
        public string ClubName { get; set; } = string.Empty;
        public string Division { get; set; } = "Division 4";
        public string Stadium { get; set; } = string.Empty;
        public int WeeklyWage { get; set; } = 500;
        public int ContractYears { get; set; } = 2;
        public string SquadRole { get; set; } = "Youth Prospect";
        public int InitialManagerTrust { get; set; } = 50;
        public string AccentColorHex { get; set; } = "#10B981";

        public static IReadOnlyList<ClubOfferData> GetDefaultStarterOffers()
        {
            return new[]
            {
                new ClubOfferData
                {
                    ClubName = "Northfield Town",
                    Division = "Division 4",
                    Stadium = "Meadow Lane",
                    WeeklyWage = 450,
                    ContractYears = 2,
                    SquadRole = "Youth Prospect",
                    InitialManagerTrust = 50,
                    AccentColorHex = "#10B981"
                },
                new ClubOfferData
                {
                    ClubName = "Bristol Rovers",
                    Division = "Division 4",
                    Stadium = "Memorial Ground",
                    WeeklyWage = 525,
                    ContractYears = 2,
                    SquadRole = "Prospect",
                    InitialManagerTrust = 45,
                    AccentColorHex = "#38BDF8"
                },
                new ClubOfferData
                {
                    ClubName = "Harrogate Town",
                    Division = "Division 4",
                    Stadium = "Wetherby Road",
                    WeeklyWage = 480,
                    ContractYears = 2,
                    SquadRole = "Youth Prospect",
                    InitialManagerTrust = 55,
                    AccentColorHex = "#F59E0B"
                }
            };
        }
    }
}
