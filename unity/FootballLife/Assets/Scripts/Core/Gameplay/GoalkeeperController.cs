using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    public enum GoalkeeperState
    {
        ReadyStance,
        Tracking,
        DiveLeft,
        DiveRight,
        JumpSave,
        Recover
    }

    /// <summary>
    /// Specialized controller for the opponent Goalkeeper positioned on the goal line.
    /// Tracks incoming ball trajectory laterally across the goal mouth, reacts with
    /// dynamic diving / jumping save postures, and maintains continuous goal protection.
    /// Zero GC allocations in Update.
    /// </summary>
    public sealed class GoalkeeperController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _goalLineZ = 34.8f;
        [SerializeField] private float _maxLateralReach = 3.3f; // Goal width is 7.32m (half is 3.66m)
        [SerializeField] private float _trackingSpeed = 6.0f;
        [SerializeField] private float _diveThresholdSpeed = 12.0f; // m/s required to trigger dive

        [Header("References")]
        [SerializeField] private BallController? _ball;

        private PawnRigTransforms? _rig;
        private GoalkeeperState _state = GoalkeeperState.ReadyStance;
        private float _animTimer;
        private float _diveTimer;
        private Vector3 _startPosition;

        public GoalkeeperState State => _state;

        public void Initialize(PawnRigTransforms rig, BallController? ball)
        {
            _rig = rig;
            _ball = ball;
            _startPosition = transform.position;
        }

        private void Awake()
        {
            _startPosition = transform.position;
            EnsureRigBound();
            if (_ball == null)
            {
                _ball = FindAnyObjectByType<BallController>();
            }
        }

        public void EnsureRigBound()
        {
            if (_rig != null) return;
            var hips = transform.Find("Hips");
            if (hips == null) return;
            var torso = hips.Find("Torso");
            if (torso == null) return;
            var head = torso.Find("Head");
            var leftArm = torso.Find("Arm_L");
            var rightArm = torso.Find("Arm_R");
            var leftLeg = hips.Find("Leg_L");
            var rightLeg = hips.Find("Leg_R");
            var leftFoot = leftLeg?.Find("Foot_L");
            var rightFoot = rightLeg?.Find("Foot_R");

            if (head != null && leftArm != null && rightArm != null &&
                leftLeg != null && rightLeg != null && leftFoot != null && rightFoot != null)
            {
                _rig = new PawnRigTransforms(
                    hips, torso, head, leftArm, rightArm, leftLeg, rightLeg, leftFoot, rightFoot
                );
            }
        }

        private void Update()
        {
            if (_rig == null)
            {
                EnsureRigBound();
                if (_rig == null) return;
            }
            if (_ball == null)
            {
                _ball = FindFirstObjectByType<BallController>();
            }

            float dt = Time.deltaTime;
            _animTimer += dt;

            switch (_state)
            {
                case GoalkeeperState.ReadyStance:
                case GoalkeeperState.Tracking:
                    UpdateTrackingAndStance(dt);
                    CheckForShotThreat();
                    break;

                case GoalkeeperState.DiveLeft:
                case GoalkeeperState.DiveRight:
                    UpdateDive(dt);
                    break;

                case GoalkeeperState.JumpSave:
                    UpdateJumpSave(dt);
                    break;

                case GoalkeeperState.Recover:
                    UpdateRecovery(dt);
                    break;
            }
        }

        // ── Lateral Tracking & Ready Posture ──────────────────────────────────
        private void UpdateTrackingAndStance(float dt)
        {
            if (_rig == null) return;

            // Ready posture: crouch, hands spread forward
            float breathe = Mathf.Sin(_animTimer * 3.0f) * 1.5f;
            _rig.Hips.localPosition = new Vector3(0f, 0.72f, 0f); // Crouched hips
            _rig.Torso.localRotation = Quaternion.Euler(18f + breathe, 0f, 0f); // Forward lean

            // Bent knees
            _rig.LeftLeg.localRotation = Quaternion.Euler(15f, 0f, -10f);
            _rig.RightLeg.localRotation = Quaternion.Euler(15f, 0f, 10f);

            // Wide ready hands facing shooter
            _rig.LeftArm.localRotation = Quaternion.Euler(-35f, -20f, -40f);
            _rig.RightArm.localRotation = Quaternion.Euler(-35f, 20f, 40f);

            // Lateral movement along goal line towards ball X
            if (_ball != null)
            {
                float targetX = Mathf.Clamp(_ball.Position.x * 0.75f, -_maxLateralReach, _maxLateralReach);
                Vector3 currentPos = transform.position;
                float newX = Mathf.MoveTowards(currentPos.x, targetX, _trackingSpeed * dt);
                transform.position = new Vector3(newX, currentPos.y, _goalLineZ);

                // Face the ball
                Vector3 lookDir = _ball.Position - transform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
                }
            }
        }

        private void CheckForShotThreat()
        {
            if (_ball == null || !_ball.IsKicked) return;

            Vector3 ballVel = _ball.Velocity;
            // Ball is moving towards goal line at high velocity
            if (ballVel.z > _diveThresholdSpeed && _ball.Position.z > 18f && _ball.Position.z < _goalLineZ)
            {
                // Predict intercept X
                float timeToGoal = (_goalLineZ - _ball.Position.z) / ballVel.z;
                float predictedX = _ball.Position.x + (ballVel.x * timeToGoal);
                float deltaX = predictedX - transform.position.x;

                if (Mathf.Abs(deltaX) > 0.6f)
                {
                    if (deltaX < 0) TriggerDive(GoalkeeperState.DiveLeft);
                    else TriggerDive(GoalkeeperState.DiveRight);
                }
                else if (_ball.Position.y > 1.4f || ballVel.y > 3f)
                {
                    TriggerDive(GoalkeeperState.JumpSave);
                }
            }
        }

        private void TriggerDive(GoalkeeperState diveState)
        {
            _state = diveState;
            _diveTimer = 0f;
        }

        private void UpdateDive(float dt)
        {
            if (_rig == null) return;

            _diveTimer += dt;
            float diveDir = (_state == GoalkeeperState.DiveLeft) ? -1f : 1f;

            // Horizontal dive pose
            float diveRoll = diveDir * 70f;
            _rig.Hips.localPosition = new Vector3(diveDir * 0.4f, 0.45f, 0f);
            _rig.Torso.localRotation = Quaternion.Euler(0f, 0f, diveRoll);

            // Extended arms reaching towards corner
            _rig.LeftArm.localRotation = Quaternion.Euler(-90f, 0f, diveRoll - 30f);
            _rig.RightArm.localRotation = Quaternion.Euler(-90f, 0f, diveRoll + 30f);

            if (_diveTimer > 1.2f)
            {
                _state = GoalkeeperState.Recover;
            }
        }

        private void UpdateJumpSave(float dt)
        {
            if (_rig == null) return;

            _diveTimer += dt;
            _rig.Hips.localPosition = new Vector3(0f, 1.15f, 0f); // Leaping up
            _rig.Torso.localRotation = Quaternion.Euler(-10f, 0f, 0f);
            _rig.LeftArm.localRotation = Quaternion.Euler(-160f, 0f, -20f);
            _rig.RightArm.localRotation = Quaternion.Euler(-160f, 0f, 20f);

            if (_diveTimer > 1.0f)
            {
                _state = GoalkeeperState.Recover;
            }
        }

        private void UpdateRecovery(float dt)
        {
            if (_rig == null) return;

            _diveTimer += dt;
            _rig.Hips.localPosition = Vector3.Lerp(_rig.Hips.localPosition, new Vector3(0f, 0.72f, 0f), dt * 4f);
            _rig.Torso.localRotation = Quaternion.Slerp(_rig.Torso.localRotation, Quaternion.Euler(18f, 0f, 0f), dt * 4f);

            if (_diveTimer > 1.8f)
            {
                _state = GoalkeeperState.ReadyStance;
            }
        }

        public void ResetPosition()
        {
            transform.position = _startPosition;
            _state = GoalkeeperState.ReadyStance;
        }
    }
}
