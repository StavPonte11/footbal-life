#nullable enable
using UnityEngine;
using FootballLife.Unity.Core.Gameplay;

namespace FootballLife.Unity.Core.Camera
{
    /// <summary>
    /// Smooth cinematic camera rig for the Home Apartment scene.
    /// Supports wide room overview with gentle ambient drift, plus smooth framing
    /// focus transitions onto interactive hotspots (Bed, Gym, Phone, Door).
    /// Zero GC allocations in update loops.
    /// </summary>
    public class HomeCameraRig : MonoBehaviour
    {
        [Header("Overview Positioning")]
        [SerializeField] private Vector3 _overviewPosition = new Vector3(0f, 5.0f, -7.8f);
        [SerializeField] private Vector3 _overviewLookTarget = new Vector3(0f, 1.1f, 0f);

        [Header("Motion Tuning")]
        [SerializeField] private float _transitionSpeed = 4.5f;
        [SerializeField] private float _ambientDriftAmount = 0.25f;
        [SerializeField] private float _ambientDriftSpeed = 0.4f;

        private HomeZoneType _activeMode = HomeZoneType.Overview;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private Vector3 _velocity;

        private float _driftTimer;

        private void Awake()
        {
            SetZoneMode(HomeZoneType.Overview, immediate: true);
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            Vector3 desiredPos = _targetPosition;

            // Ambient gentle drift in Overview mode
            if (_activeMode == HomeZoneType.Overview)
            {
                _driftTimer += dt * _ambientDriftSpeed;
                float driftX = Mathf.Sin(_driftTimer) * _ambientDriftAmount;
                float driftY = Mathf.Cos(_driftTimer * 0.7f) * (_ambientDriftAmount * 0.5f);
                desiredPos += new Vector3(driftX, driftY, 0f);
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _velocity, 1f / _transitionSpeed, Mathf.Infinity, dt);
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, dt * _transitionSpeed);
        }

        public void SetZoneMode(HomeZoneType zoneType, bool immediate = false)
        {
            _activeMode = zoneType;

            switch (zoneType)
            {
                case HomeZoneType.Overview:
                    _targetPosition = _overviewPosition;
                    _targetRotation = Quaternion.LookRotation(_overviewLookTarget - _overviewPosition);
                    break;

                case HomeZoneType.Bed:
                    _targetPosition = new Vector3(-2.0f, 2.2f, 0.9f);
                    _targetRotation = Quaternion.LookRotation(new Vector3(-4.5f, 0.7f, 3.2f) - _targetPosition);
                    break;

                case HomeZoneType.Gym:
                    _targetPosition = new Vector3(2.0f, 2.2f, 0.4f);
                    _targetRotation = Quaternion.LookRotation(new Vector3(4.5f, 0.8f, 2.6f) - _targetPosition);
                    break;

                case HomeZoneType.Phone:
                    _targetPosition = new Vector3(0f, 2.0f, -2.8f);
                    _targetRotation = Quaternion.LookRotation(new Vector3(0f, 0.4f, -0.8f) - _targetPosition);
                    break;

                case HomeZoneType.Door:
                    _targetPosition = new Vector3(-2.6f, 2.0f, -2.6f);
                    _targetRotation = Quaternion.LookRotation(new Vector3(-4.8f, 1.2f, -4.5f) - _targetPosition);
                    break;
            }

            if (immediate)
            {
                transform.position = _targetPosition;
                transform.rotation = _targetRotation;
                _velocity = Vector3.zero;
            }
        }

        public HomeZoneType ActiveMode => _activeMode;
    }
}
