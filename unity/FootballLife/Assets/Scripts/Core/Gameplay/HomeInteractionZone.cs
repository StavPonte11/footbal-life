#nullable enable
using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    public enum HomeZoneType
    {
        Overview,
        Bed,
        Gym,
        Phone,
        Door
    }

    /// <summary>
    /// Component representing an interactive hotspot in the player's home apartment
    /// (Bed for rest, Gym for workout, Phone for OS/social, Door for exiting to club).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HomeInteractionZone : MonoBehaviour
    {
        [Header("Zone Configuration")]
        [SerializeField] private HomeZoneType _zoneType;
        [SerializeField] private string _zoneName = "Zone";
        [SerializeField] private string _actionHint = "Interact";
        [SerializeField] private Transform? _cameraFocusPoint;

        public HomeZoneType ZoneType => _zoneType;
        public string ZoneName => _zoneName;
        public string ActionHint => _actionHint;
        public Transform CameraFocusPoint => _cameraFocusPoint != null ? _cameraFocusPoint : transform;

        public void Initialize(HomeZoneType zoneType, string zoneName, string actionHint, Transform? focusPoint = null)
        {
            _zoneType = zoneType;
            _zoneName = zoneName;
            _actionHint = actionHint;
            _cameraFocusPoint = focusPoint;
        }
    }
}
