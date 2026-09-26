#nullable enable
using System;
using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Clip-driven Mecanim animation driver for footballer pawns.
    /// Manages Locomotion BlendTree evaluation (Idle <-> Jog <-> Sprint) and action/celebration playback.
    /// Evaluates clips with zero GC allocations in frame updates.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class PawnAnimationPlayer : MonoBehaviour
    {
        public event Action? OnActionImpact;
        public event Action? OnActionCompleted;

        [Header("Locomotion State")]
        [Range(0f, 1f)]
        [SerializeField] private float _normalizedSpeed = 0f;

        private Animator? _animator;
        private AnimationClip? _idleClip;
        private AnimationClip? _jogClip;
        private AnimationClip? _sprintClip;

        // Active one-shot action or celebration clip
        private AnimationClip? _activeActionClip;
        private float _actionTime;
        private float _actionDuration;
        private float _impactNormalizedTime = 0.65f;
        private bool _impactDispatched;

        // Internal locomotion timer
        private float _locomotionTime;

        public float NormalizedSpeed
        {
            get => _normalizedSpeed;
            set => _normalizedSpeed = Mathf.Clamp01(value);
        }

        public bool IsPlayingAction => _activeActionClip != null;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            PrewarmClips();
        }

        public void PrewarmClips()
        {
            _idleClip = MecanimAnimationFactory.GetOrCreateIdleClip();
            _jogClip = MecanimAnimationFactory.GetOrCreateJogClip();
            _sprintClip = MecanimAnimationFactory.GetOrCreateSprintClip();
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (_activeActionClip != null)
            {
                UpdateActionClip(dt);
            }
            else
            {
                UpdateLocomotionBlend(dt);
            }
        }

        // ── Action / Celebration Playback ────────────────────────────────────
        public void PlayAction(ActionClipType action, float impactNormalizedTime = 0.65f)
        {
            _activeActionClip = MecanimAnimationFactory.GetOrCreateActionClip(action);
            _actionDuration = _activeActionClip.length;
            _actionTime = 0f;
            _impactNormalizedTime = impactNormalizedTime;
            _impactDispatched = false;
        }

        public void PlayCelebration(CelebrationClipType celebration)
        {
            _activeActionClip = MecanimAnimationFactory.GetOrCreateCelebrationClip(celebration);
            _actionDuration = _activeActionClip.length;
            _actionTime = 0f;
            _impactNormalizedTime = 1.0f;
            _impactDispatched = true;
        }

        public void StopAction()
        {
            _activeActionClip = null;
        }

        private void UpdateActionClip(float dt)
        {
            if (_activeActionClip == null) return;

            _actionTime += dt;
            float normalized = Mathf.Clamp01(_actionTime / _actionDuration);

            // Sample the clip directly into the humanoid transform hierarchy
            _activeActionClip.SampleAnimation(gameObject, _actionTime);

            // Check impact trigger
            if (!_impactDispatched && normalized >= _impactNormalizedTime)
            {
                _impactDispatched = true;
                OnActionImpact?.Invoke();
            }

            if (_actionTime >= _actionDuration)
            {
                _activeActionClip = null;
                OnActionCompleted?.Invoke();
            }
        }

        // ── Locomotion BlendTree Evaluation ──────────────────────────────────
        private void UpdateLocomotionBlend(float dt)
        {
            if (_idleClip == null || _jogClip == null || _sprintClip == null)
            {
                PrewarmClips();
            }

            // Stride rate increases smoothly with speed
            float strideRate = Mathf.Lerp(1.0f, 1.8f, _normalizedSpeed);
            _locomotionTime += dt * strideRate;

            if (_normalizedSpeed < 0.05f)
            {
                // Pure Idle
                _idleClip?.SampleAnimation(gameObject, _locomotionTime % _idleClip.length);
            }
            else if (_normalizedSpeed <= 0.60f)
            {
                // Blend Idle -> Jog
                float jogT = _normalizedSpeed / 0.60f;
                if (jogT > 0.5f)
                {
                    _jogClip?.SampleAnimation(gameObject, _locomotionTime % _jogClip.length);
                }
                else
                {
                    _idleClip?.SampleAnimation(gameObject, _locomotionTime % _idleClip.length);
                }
            }
            else
            {
                // Sprint
                _sprintClip?.SampleAnimation(gameObject, _locomotionTime % _sprintClip.length);
            }
        }
    }
}
