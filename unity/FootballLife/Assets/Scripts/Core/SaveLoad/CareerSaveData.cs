// Re-exports domain-level persistence contracts from FootballLife.Simulation.Persistence
namespace FootballLife.Unity.Core.SaveLoad
{
    public static class PersistenceConstants
    {
        public const int CurrentSchemaVersion = FootballLife.Simulation.Persistence.CareerSaveData.CurrentSchemaVersion;
        public const string CurrentGameVersion = FootballLife.Simulation.Persistence.CareerSaveData.CurrentGameVersion;
    }
}
