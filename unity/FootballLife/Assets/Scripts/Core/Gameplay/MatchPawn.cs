using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    public enum PawnRole
    {
        UserStriker,
        Teammate,
        Defender,
        Goalkeeper
    }

    public enum PawnTeam
    {
        Home,
        Away
    }

    /// <summary>
    /// Represents an on-pitch footballer entity (Player, Teammate, Defender, Goalkeeper).
    /// Holds player identity, role, team, and selection visuals.
    /// </summary>
    public sealed class MatchPawn : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string _playerName = "Player";
        [SerializeField] private int _kitNumber = 9;
        [SerializeField] private PawnRole _role = PawnRole.UserStriker;
        [SerializeField] private PawnTeam _team = PawnTeam.Home;

        [Header("Indicators")]
        [SerializeField] private GameObject? _selectionRing;

        public string PlayerName => _playerName;
        public int KitNumber => _kitNumber;
        public PawnRole Role => _role;
        public PawnTeam Team => _team;

        public void Initialize(string playerName, int kitNumber, PawnRole role, PawnTeam team)
        {
            _playerName = playerName;
            _kitNumber = kitNumber;
            _role = role;
            _team = team;

            gameObject.name = $"Pawn_{role}_{playerName}";
        }

        public void SetSelected(bool isSelected)
        {
            if (_selectionRing != null)
            {
                _selectionRing.SetActive(isSelected);
            }
        }

        public void AssignSelectionRing(GameObject ring)
        {
            _selectionRing = ring;
        }
    }
}
