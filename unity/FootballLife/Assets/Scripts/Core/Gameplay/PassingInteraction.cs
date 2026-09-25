using System;
using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Event data dispatched upon pass execution.
    /// </summary>
    public readonly struct PassExecutedEvent
    {
        public MatchPawn Receiver { get; }
        public Vector3 TargetPosition { get; }
        public bool IsIntercepted { get; }

        public PassExecutedEvent(MatchPawn receiver, Vector3 targetPosition, bool isIntercepted)
        {
            Receiver = receiver;
            TargetPosition = targetPosition;
            IsIntercepted = isIntercepted;
        }
    }

    /// <summary>
    /// Coordinates teammate passing: tap detection on supporting teammates,
    /// delivery velocity calculation, defender interception checking, and receiver control.
    /// </summary>
    public sealed class PassingInteraction : MonoBehaviour
    {
        public static event Action<PassExecutedEvent>? OnPassExecuted;

        [Header("Components")]
        [SerializeField] private BallController? _ball;
        [SerializeField] private PlayerPawnController? _striker;
        [SerializeField] private MatchPawn? _teammate;
        [SerializeField] private MatchPawn[]? _defenders;

        [Header("Tuning")]
        [SerializeField] private float _interceptionRadius = 1.4f; // Radius around defender to intercept pass

        private bool _isPassInFlight;
        private MatchPawn? _activeReceiver;

        public bool IsPassInFlight => _isPassInFlight;

        public void Initialize(
            BallController? ball,
            PlayerPawnController? striker,
            MatchPawn? teammate,
            MatchPawn[]? defenders)
        {
            _ball = ball;
            _striker = striker;
            _teammate = teammate;
            _defenders = defenders;
        }

        private void OnEnable()
        {
            TouchGestureController.OnTapDetected += HandleTap;
        }

        private void OnDisable()
        {
            TouchGestureController.OnTapDetected -= HandleTap;
        }

        private void Update()
        {
            if (!_isPassInFlight || _ball == null || _activeReceiver == null) return;

            // Check if ball has arrived at receiver
            float distToReceiver = Vector3.Distance(_ball.Position, _activeReceiver.transform.position);
            if (distToReceiver < 1.6f)
            {
                _isPassInFlight = false;
                _ball.Stop();

                // Receiver traps ball and turns to face goal
                var receiverController = _activeReceiver.GetComponent<PlayerPawnController>();
                if (receiverController != null)
                {
                    receiverController.SetState(PawnAnimState.Idle);
                    receiverController.FaceTarget(_activeReceiver.transform.position + Vector3.forward * 10f);
                }
            }
        }

        private void HandleTap(Vector2 screenPosition)
        {
            if (_ball == null || _ball.IsKicked || _teammate == null) return;

            // Raycast into 3D world to check if teammate was tapped
            var mainCam = UnityEngine.Camera.main;
            if (mainCam == null) return;

            Ray ray = mainCam.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                var hitPawn = hit.collider.GetComponentInParent<MatchPawn>();
                if (hitPawn != null && hitPawn.Role == PawnRole.Teammate)
                {
                    ExecutePassToTeammate(hitPawn);
                    return;
                }
            }

            // Also check screen-space proximity to teammate position
            Vector3 teammateScreenPos = mainCam.WorldToScreenPoint(_teammate.transform.position);
            if (teammateScreenPos.z > 0f)
            {
                float screenDist = Vector2.Distance(screenPosition, new Vector2(teammateScreenPos.x, teammateScreenPos.y));
                if (screenDist < 120f) // Generous touch target
                {
                    ExecutePassToTeammate(_teammate);
                }
            }
        }

        public void ExecutePassToTeammate(MatchPawn receiver)
        {
            if (_ball == null || _striker == null) return;

            Vector3 startPos = _ball.Position;
            // Lead pass slightly ahead of teammate's stride
            Vector3 targetPos = receiver.transform.position + Vector3.forward * 1.5f;
            Vector3 toTarget = targetPos - startPos;
            float distance = toTarget.magnitude;

            // Check defender interception along passing ray
            bool isIntercepted = CheckDefenderInterception(startPos, targetPos);

            // Calculate ground pass velocity
            float passSpeed = Mathf.Clamp(distance * 1.6f, 14f, 24f);
            Vector3 passVelocity = toTarget.normalized * passSpeed;
            passVelocity.y = 0.4f; // Ground skim

            _isPassInFlight = true;
            _activeReceiver = receiver;

            _striker.FaceTarget(targetPos);
            _striker.ExecuteKick(passVelocity, Vector3.zero, _ball);

            var evt = new PassExecutedEvent(receiver, targetPos, isIntercepted);
            OnPassExecuted?.Invoke(evt);

            Debug.Log($"[PassingInteraction] ⚽ Pass executed to {receiver.PlayerName} (Dist: {distance:F1}m, Intercepted: {isIntercepted})");
        }

        private bool CheckDefenderInterception(Vector3 start, Vector3 target)
        {
            if (_defenders == null) return false;

            Vector3 passingLine = target - start;
            float lineLen = passingLine.magnitude;
            if (lineLen < 0.01f) return false;
            Vector3 lineDir = passingLine / lineLen;

            foreach (var def in _defenders)
            {
                if (def == null) continue;
                Vector3 toDef = def.transform.position - start;
                float proj = Vector3.Dot(toDef, lineDir);

                if (proj > 1.0f && proj < lineLen - 1.0f)
                {
                    Vector3 closestPoint = start + lineDir * proj;
                    float distToLine = Vector3.Distance(def.transform.position, closestPoint);
                    if (distToLine < _interceptionRadius)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
