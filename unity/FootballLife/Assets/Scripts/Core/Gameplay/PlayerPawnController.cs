using System;
using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    public enum PawnAnimState
    {
        Idle,
        Jog,
        Run,
        PrepKick,
        Kick,
        Tackle,
        Celebrate
    }

    /// <summary>
    /// Kinematic articulation controller that drives humanoid limb rotations and body posture.
    /// Provides responsive locomotion, aiming stance, and dynamic kicking animations.
    /// Zero GC allocations in Update loop.
    /// </summary>
    public sealed class PlayerPawnController : MonoBehaviour
    {
        public event Action? OnKickImpact;

        [Header("State")]
        [SerializeField] private PawnAnimState _state = PawnAnimState.Idle;
        [SerializeField] private float _moveSpeed = 0f;

        [Header("Tuning")]
        [SerializeField] private float _jogCycleSpeed = 8.0f;
        [SerializeField] private float _runCycleSpeed = 12.0f;
        [SerializeField] private float _kickDuration = 0.35f;

        private PawnRigTransforms? _rig;
        private PawnAnimationPlayer? _animPlayer;
        private float _animTimer;
        private float _kickTimer;
        private bool _impactDispatched;

        // Pending kick parameters
        private BallController? _targetBall;
        private Vector3 _kickImpulse;
        private Vector3 _kickSpin;

        public PawnAnimState State => _state;
        public float MoveSpeed => _moveSpeed;

        public void InitializeRig(PawnRigTransforms rig)
        {
            _rig = rig;
        }

        private void Awake()
        {
            EnsureRigBound();
            _animPlayer = GetComponent<PawnAnimationPlayer>() ?? gameObject.AddComponent<PawnAnimationPlayer>();
            if (_animPlayer != null)
            {
                _animPlayer.OnActionImpact += HandleActionImpact;
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
            if (_animPlayer != null && _animPlayer.isActiveAndEnabled)
            {
                // Mecanim clip-driven system is actively evaluating
                return;
            }

            // Headless / fallback procedural evaluation
            if (_rig == null)
            {
                EnsureRigBound();
                if (_rig == null) return;
            }

            float dt = Time.deltaTime;
            _animTimer += dt;

            switch (_state)
            {
                case PawnAnimState.Idle:
                    AnimateIdle();
                    break;

                case PawnAnimState.Jog:
                    AnimateLocomotion(dt, _jogCycleSpeed, 30f, 25f);
                    break;

                case PawnAnimState.Run:
                    AnimateLocomotion(dt, _runCycleSpeed, 50f, 45f);
                    break;

                case PawnAnimState.PrepKick:
                    AnimatePrepKick(dt);
                    break;

                case PawnAnimState.Kick:
                    AnimateKick(dt);
                    break;

                case PawnAnimState.Tackle:
                    AnimateTackle();
                    break;

                case PawnAnimState.Celebrate:
                    AnimateCelebrate();
                    break;
            }
        }

        // ── Locomotion & Postures ──────────────────────────────────────────────
        private void AnimateIdle()
        {
            if (_rig == null) return;

            // Breathing bob
            float breathe = Mathf.Sin(_animTimer * 2.0f) * 1.5f;
            _rig.Torso.localRotation = Quaternion.Euler(breathe, 0f, 0f);

            // Natural stance
            _rig.LeftLeg.localRotation = Quaternion.Euler(0f, 0f, -2f);
            _rig.RightLeg.localRotation = Quaternion.Euler(0f, 0f, 2f);
            _rig.LeftArm.localRotation = Quaternion.Euler(10f + breathe, 0f, -10f);
            _rig.RightArm.localRotation = Quaternion.Euler(10f + breathe, 0f, 10f);
        }

        private void AnimateLocomotion(float dt, float cycleSpeed, float legAngle, float armAngle)
        {
            if (_rig == null) return;

            float phase = _animTimer * cycleSpeed;
            float legSwing = Mathf.Sin(phase) * legAngle;
            float armSwing = -legSwing * (armAngle / legAngle);
            float bounce = Mathf.Abs(Mathf.Cos(phase)) * 0.05f;

            _rig.Hips.localPosition = new Vector3(0f, 0.85f + bounce, 0f);
            _rig.Torso.localRotation = Quaternion.Euler(8f, 0f, 0f); // Slight forward lean

            // Alternating leg swing
            _rig.LeftLeg.localRotation = Quaternion.Euler(legSwing, 0f, 0f);
            _rig.RightLeg.localRotation = Quaternion.Euler(-legSwing, 0f, 0f);

            // Arm counter-swing
            _rig.LeftArm.localRotation = Quaternion.Euler(armSwing, 0f, -12f);
            _rig.RightArm.localRotation = Quaternion.Euler(-armSwing, 0f, 12f);
        }

        private void AnimatePrepKick(float dt)
        {
            if (_rig == null) return;

            // Plant left foot, cock right kicking leg back
            _rig.Torso.localRotation = Quaternion.Slerp(_rig.Torso.localRotation, Quaternion.Euler(-10f, -15f, 0f), dt * 10f);
            _rig.LeftLeg.localRotation = Quaternion.Slerp(_rig.LeftLeg.localRotation, Quaternion.Euler(10f, 0f, -5f), dt * 10f);
            _rig.RightLeg.localRotation = Quaternion.Slerp(_rig.RightLeg.localRotation, Quaternion.Euler(-65f, 0f, 10f), dt * 10f);

            // Balance arms
            _rig.LeftArm.localRotation = Quaternion.Slerp(_rig.LeftArm.localRotation, Quaternion.Euler(30f, 20f, -35f), dt * 10f);
            _rig.RightArm.localRotation = Quaternion.Slerp(_rig.RightArm.localRotation, Quaternion.Euler(-25f, -10f, 30f), dt * 10f);
        }

        private void AnimateKick(float dt)
        {
            if (_rig == null) return;

            _kickTimer += dt;
            float normalizedTime = Mathf.Clamp01(_kickTimer / _kickDuration);

            // Strike through: rapid forward swing of kicking leg
            if (normalizedTime < 0.45f)
            {
                // Rapid follow through forward
                float strikePhase = normalizedTime / 0.45f;
                float legPitch = Mathf.Lerp(-65f, 55f, strikePhase);
                _rig.RightLeg.localRotation = Quaternion.Euler(legPitch, 0f, 0f);
                _rig.Torso.localRotation = Quaternion.Euler(15f, 5f, 0f);
            }
            else
            {
                // Dispatch ball kick impact exactly at the forward apex
                if (!_impactDispatched)
                {
                    _impactDispatched = true;
                    if (_targetBall != null)
                    {
                        _targetBall.Kick(_kickImpulse, _kickSpin);
                    }
                    OnKickImpact?.Invoke();
                }

                // Follow-through and recovery back to neutral
                float recoveryPhase = (normalizedTime - 0.45f) / 0.55f;
                float legPitch = Mathf.Lerp(55f, 0f, recoveryPhase);
                _rig.RightLeg.localRotation = Quaternion.Euler(legPitch, 0f, 0f);
                _rig.Torso.localRotation = Quaternion.Slerp(_rig.Torso.localRotation, Quaternion.identity, dt * 5f);
            }

            if (normalizedTime >= 1.0f)
            {
                SetState(PawnAnimState.Idle);
            }
        }

        private void AnimateTackle()
        {
            if (_rig == null) return;

            // Low slide stance
            _rig.Hips.localPosition = new Vector3(0f, 0.45f, 0f);
            _rig.Torso.localRotation = Quaternion.Euler(-25f, 0f, 0f);
            _rig.LeftLeg.localRotation = Quaternion.Euler(-15f, 0f, 0f);
            _rig.RightLeg.localRotation = Quaternion.Euler(65f, 0f, 0f);
            _rig.LeftArm.localRotation = Quaternion.Euler(-45f, 0f, -30f);
            _rig.RightArm.localRotation = Quaternion.Euler(45f, 0f, 30f);
        }

        private void AnimateCelebrate()
        {
            if (_rig == null) return;

            // Arms raised in V-shape, jumping bounce
            float bounce = Mathf.Abs(Mathf.Sin(_animTimer * 5.0f)) * 0.15f;
            _rig.Hips.localPosition = new Vector3(0f, 0.85f + bounce, 0f);
            _rig.Torso.localRotation = Quaternion.Euler(-5f, 0f, 0f);

            // Arms up high
            _rig.LeftArm.localRotation = Quaternion.Euler(-145f, 0f, -35f);
            _rig.RightArm.localRotation = Quaternion.Euler(-145f, 0f, 35f);

            _rig.LeftLeg.localRotation = Quaternion.Euler(5f, 0f, -3f);
            _rig.RightLeg.localRotation = Quaternion.Euler(5f, 0f, 3f);
        }

        // ── Public Action APIs ────────────────────────────────────────────────
        public void SetState(PawnAnimState newState)
        {
            if (_state == newState) return;
            _state = newState;
            _animTimer = 0f;

            if (_animPlayer != null)
            {
                switch (newState)
                {
                    case PawnAnimState.Idle:
                        _animPlayer.StopAction();
                        _animPlayer.NormalizedSpeed = 0f;
                        break;
                    case PawnAnimState.Jog:
                        _animPlayer.StopAction();
                        _animPlayer.NormalizedSpeed = 0.50f;
                        break;
                    case PawnAnimState.Run:
                        _animPlayer.StopAction();
                        _animPlayer.NormalizedSpeed = 1.0f;
                        break;
                    case PawnAnimState.PrepKick:
                        _animPlayer.NormalizedSpeed = 0f;
                        break;
                    case PawnAnimState.Tackle:
                        _animPlayer.PlayAction(ActionClipType.SlidingTackle, 0.45f);
                        break;
                    case PawnAnimState.Celebrate:
                        _animPlayer.PlayCelebration(CelebrationClipType.KneeSlide);
                        break;
                }
            }

            if (newState == PawnAnimState.Kick)
            {
                _kickTimer = 0f;
                _impactDispatched = false;
            }
        }

        public void PrepareKick()
        {
            SetState(PawnAnimState.PrepKick);
        }

        public void ExecuteKick(Vector3 impulse, Vector3 spin, BallController? ball = null)
        {
            _kickImpulse = impulse;
            _kickSpin = spin;
            _targetBall = ball;
            _impactDispatched = false;
            _kickTimer = 0f;
            _animTimer = 0f;
            _state = PawnAnimState.Kick;

            if (_animPlayer != null)
            {
                var action = spin.sqrMagnitude > 15f ? ActionClipType.FinesseCurl : ActionClipType.PowerShot;
                _animPlayer.PlayAction(action, 0.65f);
            }
        }

        private void HandleActionImpact()
        {
            if (!_impactDispatched)
            {
                _impactDispatched = true;
                if (_targetBall != null)
                {
                    _targetBall.Kick(_kickImpulse, _kickSpin);
                }
                OnKickImpact?.Invoke();
            }
        }

        public void FaceTarget(Vector3 worldTargetPosition)
        {
            Vector3 dir = worldTargetPosition - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }
    }
}
