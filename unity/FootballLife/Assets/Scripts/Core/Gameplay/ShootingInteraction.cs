using UnityEngine;
using FootballLife.Unity.Core.Camera;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Coordinates the shooting mini-interaction: converts touch aim gestures into
    /// calibrated 3D ball launch velocity and Magnus curve spin, drives trajectory preview,
    /// and triggers striker kicking animations and camera tracking.
    /// Zero GC allocations in event handlers.
    /// </summary>
    public sealed class ShootingInteraction : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private BallController? _ball;
        [SerializeField] private PlayerPawnController? _striker;
        [SerializeField] private AimTrajectoryRenderer? _trajectoryGuide;
        [SerializeField] private MatchCameraRig? _cameraRig;

        [Header("Tuning")]
        [SerializeField] private float _minShotSpeed = 18.0f; // m/s (~65 km/h)
        [SerializeField] private float _maxShotSpeed = 30.0f; // m/s (~108 km/h)
        [SerializeField] private float _maxSpinAngularVel = 10.0f; // rad/s

        private bool _isAiming;
        private Vector3 _calculatedVelocity;
        private Vector3 _calculatedSpin;

        public bool IsAiming => _isAiming;

        public void Initialize(
            BallController? ball,
            PlayerPawnController? striker,
            AimTrajectoryRenderer? trajectoryGuide,
            MatchCameraRig? cameraRig)
        {
            _ball = ball;
            _striker = striker;
            _trajectoryGuide = trajectoryGuide;
            _cameraRig = cameraRig;
        }

        private void OnEnable()
        {
            TouchGestureController.OnAimStarted += HandleAimStart;
            TouchGestureController.OnAimUpdated += HandleAimUpdate;
            TouchGestureController.OnAimReleased += HandleAimRelease;
            GoalTrigger.OnGoalScored += HandleGoalScored;
        }

        private void OnDisable()
        {
            TouchGestureController.OnAimStarted -= HandleAimStart;
            TouchGestureController.OnAimUpdated -= HandleAimUpdate;
            TouchGestureController.OnAimReleased -= HandleAimRelease;
            GoalTrigger.OnGoalScored -= HandleGoalScored;
        }

        private void HandleAimStart(AimGestureData data)
        {
            if (_ball == null || _ball.IsKicked) return;
            _isAiming = true;
            if (_striker != null)
            {
                _striker.PrepareKick();
            }
        }

        private void HandleAimUpdate(AimGestureData data)
        {
            if (!_isAiming || _ball == null) return;

            ComputeVelocityAndSpin(data, out _calculatedVelocity, out _calculatedSpin);

            if (_trajectoryGuide != null)
            {
                _trajectoryGuide.UpdateTrajectory(_ball.Position, _calculatedVelocity, _calculatedSpin);
            }

            if (_striker != null)
            {
                Vector3 aimTarget = _ball.Position + _calculatedVelocity.normalized * 5f;
                _striker.FaceTarget(aimTarget);
            }
        }

        private void HandleAimRelease(AimGestureData data)
        {
            if (!_isAiming) return;
            _isAiming = false;

            if (_trajectoryGuide != null)
            {
                _trajectoryGuide.Hide();
            }

            if (_ball == null || _ball.IsKicked) return;

            ComputeVelocityAndSpin(data, out _calculatedVelocity, out _calculatedSpin);

            // Execute kick through striker animation
            if (_striker != null)
            {
                _striker.ExecuteKick(_calculatedVelocity, _calculatedSpin, _ball);
            }
            else
            {
                _ball.Kick(_calculatedVelocity, _calculatedSpin);
            }

            // Audio: Kick impact scaled by power
            Audio.AudioManager.Instance?.PlayKick(data.Power01, _ball.Position);

            // VFX: Turf dust burst (#P7-403)
            TurfVfxPool.Instance?.SpawnKickDust(_ball.Position, _calculatedVelocity, data.Power01);

            // Follow shot with camera
            if (_cameraRig != null)
            {
                _cameraRig.SetMode(MatchCameraRig.CameraMode.ShotTrack);
            }
        }

        private void ComputeVelocityAndSpin(AimGestureData data, out Vector3 velocity, out Vector3 spin)
        {
            // Upward swipe (delta.y > 0) translates to forward flight towards the goal line (+Z)
            float forwardFactor = Mathf.Clamp(data.SwipeDelta.y / 150f, 0.2f, 1.2f);
            float lateralFactor = Mathf.Clamp(data.SwipeDelta.x / 120f, -0.8f, 0.8f);

            // Speed scaled by swipe power (0.0 to 1.0)
            float speed = Mathf.Lerp(_minShotSpeed, _maxShotSpeed, data.Power01);

            // Forward flight towards goal line
            float vz = forwardFactor * speed * 0.90f;
            float vx = lateralFactor * speed * 0.65f;
            // Vertical lift
            float vy = Mathf.Clamp(2.5f + (data.Power01 * 3.5f), 1.8f, 6.2f);

            velocity = new Vector3(vx, vy, vz);

            // Curvature / spin around vertical Y axis (banana curl) and horizontal X axis (topspin/dip)
            float curlRate = data.SpinValue * _maxSpinAngularVel;
            float dipRate = (data.Power01 > 0.7f) ? 3.5f : 0f; // High power creates dipping topspin
            spin = new Vector3(dipRate, curlRate, 0f);
        }

        private void HandleGoalScored(GoalScoredEvent evt)
        {
            if (_striker != null)
            {
                _striker.SetState(PawnAnimState.Celebrate);
            }
            if (_cameraRig != null)
            {
                _cameraRig.SetMode(MatchCameraRig.CameraMode.Celebration);
            }

            // Audio: Stadium crowd roar & referee goal whistle
            Audio.AudioManager.Instance?.TriggerCrowdRoar();
            Audio.AudioManager.Instance?.PlayWhistle(Audio.WhistleType.GoalScored);
        }
    }
}
