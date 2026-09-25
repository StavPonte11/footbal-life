using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Event data for touch drag and aim gestures.
    /// </summary>
    public readonly struct AimGestureData
    {
        public Vector2 StartScreenPos { get; }
        public Vector2 CurrentScreenPos { get; }
        public Vector2 SwipeDelta { get; }
        public float Power01 { get; }      // 0.0 to 1.0 based on swipe distance
        public float SpinValue { get; }    // -1.0 (left curve) to +1.0 (right curve)

        public AimGestureData(Vector2 startPos, Vector2 currentPos, Vector2 delta, float power, float spin)
        {
            StartScreenPos = startPos;
            CurrentScreenPos = currentPos;
            SwipeDelta = delta;
            Power01 = power;
            SpinValue = spin;
        }
    }

    /// <summary>
    /// Processes mobile touch and mouse input:
    ///   - Tap: Selects teammates or on-pitch targets.
    ///   - Swipe/Drag: Drives shot aim trajectory, power, and Magnus curvature.
    /// Zero GC allocations in Update.
    /// </summary>
    public sealed class TouchGestureController : MonoBehaviour
    {
        public static event Action<AimGestureData>? OnAimStarted;
        public static event Action<AimGestureData>? OnAimUpdated;
        public static event Action<AimGestureData>? OnAimReleased;
        public static event Action<Vector2>? OnTapDetected;

        [Header("Tuning")]
        [SerializeField] private float _maxDragDistance = 280f; // Screen pixels for 100% power
        [SerializeField] private float _minDragThreshold = 18f;  // Pixels before drag begins
        [SerializeField] private float _maxTapDuration = 0.25f;  // Seconds for tap vs drag
        [SerializeField] private float _maxTapMovement = 15f;    // Max drift pixels for tap

        private bool _isDragging;
        private Vector2 _touchStartPos;
        private float _touchStartTime;
        private Vector2 _lastTouchPos;
        private Vector2 _midTouchPos; // Used for measuring trajectory curvature / spin

        public bool IsDragging => _isDragging;

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            Vector2 currentInputPos = Vector2.zero;
            bool inputBegan = false;
            bool inputHeld = false;
            bool inputEnded = false;

#if ENABLE_INPUT_SYSTEM
            var pointer = Pointer.current;
            if (pointer != null)
            {
                currentInputPos = pointer.position.ReadValue();
                inputBegan = pointer.press.wasPressedThisFrame;
                inputHeld = pointer.press.isPressed;
                inputEnded = pointer.press.wasReleasedThisFrame;
            }
#else
            // Fallback for legacy input
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                currentInputPos = t.position;
                if (t.phase == TouchPhase.Began) inputBegan = true;
                else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) inputHeld = true;
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) inputEnded = true;
            }
            else
            {
                currentInputPos = Input.mousePosition;
                if (Input.GetMouseButtonDown(0)) inputBegan = true;
                else if (Input.GetMouseButton(0)) inputHeld = true;
                else if (Input.GetMouseButtonUp(0)) inputEnded = true;
            }
#endif

            // 1. Gesture Start
            if (inputBegan)
            {
                _touchStartPos = currentInputPos;
                _touchStartTime = Time.time;
                _lastTouchPos = currentInputPos;
                _midTouchPos = currentInputPos;
                _isDragging = false;
            }

            // 2. Gesture In Progress
            if (inputHeld)
            {
                float totalMoveDist = Vector2.Distance(_touchStartPos, currentInputPos);

                if (!_isDragging && totalMoveDist > _minDragThreshold)
                {
                    _isDragging = true;
                    var startData = ComputeAimData(_touchStartPos, currentInputPos);
                    OnAimStarted?.Invoke(startData);
                }

                if (_isDragging)
                {
                    // Track midpoint to calculate curvature
                    _midTouchPos = Vector2.Lerp(_midTouchPos, currentInputPos, 0.2f);
                    _lastTouchPos = currentInputPos;

                    var updateData = ComputeAimData(_touchStartPos, currentInputPos);
                    OnAimUpdated?.Invoke(updateData);
                }
            }

            // 3. Gesture End
            if (inputEnded)
            {
                float touchDuration = Time.time - _touchStartTime;
                float touchDistance = Vector2.Distance(_touchStartPos, currentInputPos);

                if (_isDragging)
                {
                    _isDragging = false;
                    var releaseData = ComputeAimData(_touchStartPos, currentInputPos);
                    OnAimReleased?.Invoke(releaseData);
                }
                else if (touchDuration <= _maxTapDuration && touchDistance <= _maxTapMovement)
                {
                    OnTapDetected?.Invoke(currentInputPos);
                }
            }
        }

        private AimGestureData ComputeAimData(Vector2 start, Vector2 current)
        {
            Vector2 delta = current - start;
            float rawDist = delta.magnitude;
            float power = Mathf.Clamp01(rawDist / _maxDragDistance);

            // Calculate spin from curvature: perpendicular deviation of midpoint from straight chord
            float spin = 0f;
            if (rawDist > 25f)
            {
                Vector2 chord = delta.normalized;
                Vector2 chordNormal = new Vector2(-chord.y, chord.x);
                Vector2 midOffset = _midTouchPos - (start + delta * 0.5f);
                float lateralDeviation = Vector2.Dot(midOffset, chordNormal);
                // Clamped -1.0 to 1.0 (positive = right curve, negative = left curve)
                spin = Mathf.Clamp(lateralDeviation / 45f, -1.0f, 1.0f);
            }

            return new AimGestureData(start, current, delta, power, spin);
        }
    }
}
