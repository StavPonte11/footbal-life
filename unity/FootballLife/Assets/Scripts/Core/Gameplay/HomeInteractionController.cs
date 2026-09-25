#nullable enable
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using FootballLife.Domain;
using FootballLife.Unity.Core.Camera;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Coordinates user interaction with 3D apartment hotspots (Bed, Gym, Phone, Door).
    /// Detects raycast taps, directs the HomeCameraRig, and dispatches interaction events.
    /// </summary>
    public class HomeInteractionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HomeCameraRig? _cameraRig;
        [SerializeField] private UnityEngine.Camera? _mainCamera;

        public event Action<HomeZoneType>? OnZoneFocused;
        public event Action? OnBedInteracted;
        public event Action? OnGymInteracted;
        public event Action? OnPhoneRequested;
        public event Action? OnDoorExitRequested;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            HandleInput();
        }

        private void ResolveReferences()
        {
            if (_cameraRig == null) _cameraRig = FindAnyObjectByType<HomeCameraRig>();
            if (_mainCamera == null) _mainCamera = UnityEngine.Camera.main;
        }

        private void HandleInput()
        {
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame) return;

            ResolveReferences();
            if (_mainCamera == null) return;

            Vector2 screenPos = pointer.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 30.0f))
            {
                var zone = hit.collider.GetComponentInParent<HomeInteractionZone>();
                if (zone != null)
                {
                    SelectZone(zone.ZoneType);
                    DispatchInteraction(zone.ZoneType);
                }
            }
        }

        public void SelectZone(HomeZoneType zoneType)
        {
            ResolveReferences();
            if (_cameraRig != null)
            {
                _cameraRig.SetZoneMode(zoneType);
            }
            OnZoneFocused?.Invoke(zoneType);
        }

        public void DispatchInteraction(HomeZoneType zoneType)
        {
            switch (zoneType)
            {
                case HomeZoneType.Bed:
                    OnBedInteracted?.Invoke();
                    break;
                case HomeZoneType.Gym:
                    OnGymInteracted?.Invoke();
                    break;
                case HomeZoneType.Phone:
                    OnPhoneRequested?.Invoke();
                    break;
                case HomeZoneType.Door:
                    OnDoorExitRequested?.Invoke();
                    break;
            }
        }

        public void RestInBed()
        {
            SelectZone(HomeZoneType.Bed);
            OnBedInteracted?.Invoke();
        }

        public void DoHomeWorkout()
        {
            SelectZone(HomeZoneType.Gym);
            OnGymInteracted?.Invoke();
        }

        public void TriggerPhone()
        {
            SelectZone(HomeZoneType.Phone);
            OnPhoneRequested?.Invoke();
        }

        public void ExitToClub()
        {
            SelectZone(HomeZoneType.Door);
            OnDoorExitRequested?.Invoke();
        }
    }
}
