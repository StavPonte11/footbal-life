using System;
using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Event data dispatched when a goal is detected inside the net.
    /// </summary>
    public readonly struct GoalScoredEvent
    {
        public Vector3 EntryPosition { get; }
        public float ShotSpeedKmh { get; }
        public float TimeStamp { get; }

        public GoalScoredEvent(Vector3 entryPosition, float shotSpeedKmh, float timeStamp)
        {
            EntryPosition = entryPosition;
            ShotSpeedKmh = shotSpeedKmh;
            TimeStamp = timeStamp;
        }
    }

    /// <summary>
    /// Volume trigger positioned inside the 3D goal net that registers goals.
    /// Includes debouncing to prevent multiple triggers from a single shot.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class GoalTrigger : MonoBehaviour
    {
        public static event Action<GoalScoredEvent>? OnGoalScored;

        [SerializeField] private float _debounceTime = 2.0f;
        private float _lastGoalTime = -999f;
        private BoxCollider? _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            _collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.time - _lastGoalTime < _debounceTime)
                return;

            var ball = other.GetComponent<BallController>();
            if (ball == null)
            {
                ball = other.GetComponentInParent<BallController>();
            }

            if (ball != null)
            {
                _lastGoalTime = Time.time;
                float speedKmh = ball.CurrentSpeed * 3.6f;
                var evt = new GoalScoredEvent(ball.transform.position, speedKmh, Time.time);

                Debug.Log($"[GoalTrigger] ⚽ GOAL SCORED! Speed: {speedKmh:F1} km/h at pos {ball.transform.position}");
                OnGoalScored?.Invoke(evt);
            }
        }
    }
}
