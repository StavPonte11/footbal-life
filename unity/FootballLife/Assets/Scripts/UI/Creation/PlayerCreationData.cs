using System;
using FootballLife.Domain;

namespace FootballLife.Unity.UI.Creation
{
    /// <summary>
    /// User-configured data payload captured during the player creation flow.
    /// </summary>
    public sealed class PlayerCreationData
    {
        public string FirstName { get; set; } = "Marcus";
        public string LastName { get; set; } = "Vance";
        public string Nationality { get; set; } = "England";
        public Position PrimaryPosition { get; set; } = Position.ST;
        public Foot PreferredFoot { get; set; } = Foot.Right;
        public int KitNumber { get; set; } = 9;

        public string FullName => $"{FirstName} {LastName}".Trim();

        public bool IsValid => !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName);
    }
}
