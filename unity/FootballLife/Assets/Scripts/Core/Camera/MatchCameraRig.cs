using UnityEngine;

namespace FootballLife.Unity.Core.Camera
{
    /// <summary>
    /// Dynamic Camera Rig controlling the match camera with 3 operational modes:
    ///   - Broadcast: Elevated tactical side-angle view showing player, defenders, and space.
    ///   - ActionAim: Over-the-shoulder / behind-the-ball angle facing the goal for shooting.
    ///   - ShotTrack: Dynamic follow zoom tracking the ball towards the net upon strike.
    /// Zero GC allocations in LateUpdate.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class MatchCameraRig : MonoBehaviour
    {
        public enum CameraMode
        {
            Broadcast,
            ActionAim,
            ShotTrack,
            Celebration
        }

        // ── Inspector Configuration ───────────────────────────────────────────
        [Header("Targets")]
        [SerializeField] private Transform? _playerTarget;
        [SerializeField] private Transform? _ballTarget;
        [SerializeField] private Transform? _goalTarget;

        [Header("Mode Offsets")]
        [SerializeField] private Vector3 _broadcastOffset = new Vector3(0f, 16f, -22f);
        [SerializeField] private Vector3 _actionAimOffset = new Vector3(0f, 2.8f, -6.5f);
        [SerializeField] private Vector3 _shotTrackOffset = new Vector3(0f, 3.5f, -8f);
        [SerializeField] private Vector3 _celebrationOffset = new Vector3(0f, 1.8f, -3.5f);

        [Header("Smoothing")]
        [SerializeField] private float _positionSmoothTime = 0.25f;
        [SerializeField] private float _rotationSmoothSpeed = 8.0f;
        [SerializeField] private float _fieldOfViewDefault = 55f;
        [SerializeField] private float _fieldOfViewShot = 45f;

        // ── State ─────────────────────────────────────────────────────────────
        private UnityEngine.Camera _cam = null!;
        private CameraMode _currentMode = CameraMode.Broadcast;
        private Vector3 _currentVelocity;

        public CameraMode CurrentMode => _currentMode;

        private void Awake()
        {
            _cam = GetComponent<UnityEngine.Camera>();
            _cam.fieldOfView = _fieldOfViewDefault;
        }

        private void LateUpdate()
        {
            Transform? activeTarget = GetActiveTarget();
            if (activeTarget == null) return;

            Vector3 targetPosition = CalculateTargetPosition(activeTarget);
            Quaternion targetRotation = CalculateTargetRotation(activeTarget);

            // Smooth position dampening
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _currentVelocity,
                _positionSmoothTime);

            // Smooth rotation interpolation
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * _rotationSmoothSpeed);

            // Dynamic FOV adjustment
            float targetFov = _currentMode == CameraMode.ShotTrack ? _fieldOfViewShot : _fieldOfViewDefault;
            if (Mathf.Abs(_cam.fieldOfView - targetFov) > 0.05f)
            {
                _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFov, Time.deltaTime * 4f);
            }
        }

        private Transform? GetActiveTarget()
        {
            return _currentMode switch
            {
                CameraMode.Broadcast  => _playerTarget != null ? _playerTarget : _ballTarget,
                CameraMode.ActionAim  => _ballTarget   != null ? _ballTarget   : _playerTarget,
                CameraMode.ShotTrack  => _ballTarget   != null ? _ballTarget   : _playerTarget,
                CameraMode.Celebration => _playerTarget != null ? _playerTarget : _ballTarget,
                _                     => _playerTarget
            };
        }

        private Vector3 CalculateTargetPosition(Transform target)
        {
            Vector3 basePos = target.position;

            return _currentMode switch
            {
                CameraMode.Broadcast =>
                    new Vector3(basePos.x * 0.4f, 0f, basePos.z) + _broadcastOffset,

                CameraMode.ActionAim =>
                    basePos + _actionAimOffset,

                CameraMode.ShotTrack =>
                    basePos + _shotTrackOffset,

                CameraMode.Celebration =>
                    basePos + _celebrationOffset,

                _ => basePos + _broadcastOffset
            };
        }

        private Quaternion CalculateTargetRotation(Transform target)
        {
            Vector3 lookAtPoint = target.position;

            switch (_currentMode)
            {
                case CameraMode.ActionAim:
                    // Look towards the opponent goal if assigned, otherwise look forward
                    if (_goalTarget != null)
                    {
                        lookAtPoint = _goalTarget.position + Vector3.up * 1.2f;
                    }
                    else
                    {
                        lookAtPoint = target.position + Vector3.forward * 20f + Vector3.up * 1.5f;
                    }
                    break;

                case CameraMode.ShotTrack:
                    if (_goalTarget != null)
                    {
                        lookAtPoint = Vector3.Lerp(target.position, _goalTarget.position, 0.4f) + Vector3.up * 1.0f;
                    }
                    else
                    {
                        lookAtPoint = target.position + Vector3.up * 0.5f;
                    }
                    break;

                case CameraMode.Broadcast:
                    lookAtPoint = new Vector3(target.position.x, 0.5f, target.position.z + 5f);
                    break;

                case CameraMode.Celebration:
                    lookAtPoint = target.position + Vector3.up * 1.5f;
                    break;
            }

            Vector3 direction = lookAtPoint - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                return Quaternion.LookRotation(direction, Vector3.up);
            }
            return transform.rotation;
        }

        // ── Public APIs ───────────────────────────────────────────────────────
        public void SetMode(CameraMode mode)
        {
            _currentMode = mode;
        }

        public void SetTargets(Transform? player, Transform? ball, Transform? goal)
        {
            _playerTarget = player;
            _ballTarget = ball;
            _goalTarget = goal;
        }

        public void SnapToTarget()
        {
            Transform? target = GetActiveTarget();
            if (target != null)
            {
                transform.position = CalculateTargetPosition(target);
                transform.rotation = CalculateTargetRotation(target);
                _currentVelocity = Vector3.zero;
            }
        }
    }
}
