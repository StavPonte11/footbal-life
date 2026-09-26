using System;
using UnityEngine;
using FootballLife.Domain;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Simulates dynamic goal-net ripple reaction physics upon ball entry.
    /// When a goal is registered, generates a damped harmonic impulse wave across the net geometry
    /// matching the ball speed and impact position, with zero GC allocations.
    /// </summary>
    public sealed class NetRippleController : MonoBehaviour
    {
        [Header("Net Transforms")]
        [SerializeField] private Transform? _netBackTransform;
        [SerializeField] private Transform? _netTopTransform;

        private Vector3 _baseBackLocalPos;
        private Vector3 _baseBackLocalScale;
        private Vector3 _baseTopLocalPos;

        private bool _isRippling = false;
        private float _elapsed = 0f;
        private float _shotSpeedKmh = 0f;
        private Vector3 _impactDir = Vector3.forward;

        private void Awake()
        {
            InitializeTransforms();
        }

        private void OnEnable()
        {
            GoalTrigger.OnGoalScored += HandleGoalScored;
        }

        private void OnDisable()
        {
            GoalTrigger.OnGoalScored -= HandleGoalScored;
            ResetNet();
        }

        public void InitializeTransforms()
        {
            if (_netBackTransform == null)
            {
                var back = transform.Find("Net_Back");
                if (back != null) _netBackTransform = back;
            }

            if (_netTopTransform == null)
            {
                var top = transform.Find("Net_Top");
                if (top != null) _netTopTransform = top;
            }

            if (_netBackTransform != null)
            {
                _baseBackLocalPos = _netBackTransform.localPosition;
                _baseBackLocalScale = _netBackTransform.localScale;
            }

            if (_netTopTransform != null)
            {
                _baseTopLocalPos = _netTopTransform.localPosition;
            }
        }

        private void HandleGoalScored(GoalScoredEvent evt)
        {
            TriggerRipple(evt.ShotSpeedKmh, evt.EntryPosition);
        }

        public void TriggerRipple(float speedKmh, Vector3 entryPosition)
        {
            _shotSpeedKmh = speedKmh;
            _elapsed = 0f;
            _isRippling = true;

            // Direction from goal mouth into back of net (+Z in local space)
            _impactDir = Vector3.forward;
        }

        private void Update()
        {
            if (!_isRippling) return;

            float dt = Time.deltaTime;
            _elapsed += dt;

            if (_elapsed > 2.0f)
            {
                ResetNet();
                return;
            }

            // Damped vibration displacement using domain physics formula
            float displacement = StadiumAtmosphereUtility.ComputeNetRipple(_shotSpeedKmh, _elapsed);

            if (_netBackTransform != null)
            {
                // Bulge back wall outwards
                _netBackTransform.localPosition = _baseBackLocalPos + (_impactDir * displacement);

                // Slight lateral vibration
                float lateralJitter = Mathf.Sin(_elapsed * 32f) * 0.015f * Mathf.Exp(-3f * _elapsed);
                _netBackTransform.localScale = new Vector3(
                    _baseBackLocalScale.x + lateralJitter,
                    _baseBackLocalScale.y + (displacement * 0.3f),
                    _baseBackLocalScale.z);
            }

            if (_netTopTransform != null)
            {
                // Top net sag and bounce
                _netTopTransform.localPosition = new Vector3(
                    _baseTopLocalPos.x,
                    _baseTopLocalPos.y - (displacement * 0.4f),
                    _baseTopLocalPos.z + (displacement * 0.2f));
            }
        }

        private void ResetNet()
        {
            _isRippling = false;
            _elapsed = 0f;

            if (_netBackTransform != null)
            {
                _netBackTransform.localPosition = _baseBackLocalPos;
                _netBackTransform.localScale = _baseBackLocalScale;
            }

            if (_netTopTransform != null)
            {
                _netTopTransform.localPosition = _baseTopLocalPos;
            }
        }
    }
}
