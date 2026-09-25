using System;
using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Realistic 3D Ball Physics Controller with aerodynamic Magnus curve forces,
    /// calibrated PhysicMaterial bounce/roll friction, and trajectory tracing.
    /// Zero GC allocations in physics updates.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public sealed class BallController : MonoBehaviour
    {
        // ── Events ────────────────────────────────────────────────────────────
        public event Action? OnBallKicked;
        public event Action? OnBallStopped;
        public event Action<Collision>? OnBallCollided;

        // ── Physics Tuning ────────────────────────────────────────────────────
        [Header("Ball Specification (FIFA Regulation)")]
        [SerializeField] private float _ballRadius = 0.11f;       // 22cm diameter
        [SerializeField] private float _ballMass = 0.43f;         // 430 grams
        [SerializeField] private float _linearDrag = 0.15f;
        [SerializeField] private float _angularDrag = 0.25f;

        [Header("Aerodynamics (Magnus Effect)")]
        [SerializeField] private float _magnusCoeff = 0.003f;
        [SerializeField] private float _airResistance = 0.0008f;
        [SerializeField] private float _minSpeedForMagnus = 2.0f;

        [Header("Ground & Surface Friction")]
        [SerializeField] private float _surfaceBounciness = 0.68f;
        [SerializeField] private float _dynamicFriction = 0.35f;
        [SerializeField] private float _staticFriction = 0.45f;

        // ── State ─────────────────────────────────────────────────────────────
        private Rigidbody _rb = null!;
        private SphereCollider _collider = null!;
        private TrailRenderer? _trail;
        private bool _isKicked;
        private Vector3 _initialPosition;

        // ── Public Accessors ──────────────────────────────────────────────────
        public bool IsKicked => _isKicked;
        public float CurrentSpeed => _rb != null ? _rb.linearVelocity.magnitude : 0f;
        public Vector3 Velocity => _rb != null ? _rb.linearVelocity : Vector3.zero;
        public Vector3 Position => transform.position;

        private void Awake()
        {
            EnsureComponents();
        }

        private void EnsureComponents()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            if (_collider == null) _collider = GetComponent<SphereCollider>() ?? gameObject.AddComponent<SphereCollider>();
            if (_trail == null) _trail = GetComponent<TrailRenderer>();

            ConfigurePhysics();
            if (_initialPosition == Vector3.zero) _initialPosition = transform.position;
        }

        private void ConfigurePhysics()
        {
            _rb.mass = _ballMass;
            _rb.linearDamping = _linearDrag;
            _rb.angularDamping = _angularDrag;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            _collider.radius = _ballRadius;

            // Generate or assign regulation ball PhysicMaterial
            var mat = new PhysicsMaterial("BallPhysicsMaterial")
            {
                bounciness = _surfaceBounciness,
                dynamicFriction = _dynamicFriction,
                staticFriction = _staticFriction,
                bounceCombine = PhysicsMaterialCombine.Maximum,
                frictionCombine = PhysicsMaterialCombine.Average
            };
            _collider.material = mat;

            if (_trail != null)
            {
                _trail.emitting = false;
            }
        }

        private void FixedUpdate()
        {
            if (!_isKicked) return;

            Vector3 vel = _rb.linearVelocity;
            Vector3 angVel = _rb.angularVelocity;
            float speedSqr = vel.sqrMagnitude;

            // Apply Magnus lift / curve force when spinning
            if (speedSqr > (_minSpeedForMagnus * _minSpeedForMagnus))
            {
                Vector3 magnusForce = Vector3.Cross(angVel, vel) * _magnusCoeff;
                _rb.AddForce(magnusForce, ForceMode.Force);

                // High-speed aerodynamic drag
                Vector3 dragForce = -vel.normalized * (speedSqr * _airResistance);
                _rb.AddForce(dragForce, ForceMode.Force);
            }

            // Detect rest/stopped condition
            if (speedSqr < 0.05f && transform.position.y <= (_ballRadius + 0.05f))
            {
                _isKicked = false;
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;

                if (_trail != null)
                {
                    _trail.emitting = false;
                }

                OnBallStopped?.Invoke();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            OnBallCollided?.Invoke(collision);
        }

        // ── Public Action APIs ────────────────────────────────────────────────
        /// <summary>
        /// Kicks the ball with specified linear impulse and angular spin.
        /// </summary>
        /// <param name="impulseVelocity">Linear launch velocity in m/s.</param>
        /// <param name="spinVelocity">Angular rotation in radians/sec (creates Magnus curve).</param>
        public void Kick(Vector3 impulseVelocity, Vector3 spinVelocity)
        {
            if (_rb == null) EnsureComponents();
            _isKicked = true;
            _rb.linearVelocity = impulseVelocity;
            _rb.angularVelocity = spinVelocity;

            if (_trail != null)
            {
                _trail.Clear();
                _trail.emitting = true;
            }

            OnBallKicked?.Invoke();
        }

        /// <summary>
        /// Instantly stops all ball motion and angular rotation.
        /// </summary>
        public void Stop()
        {
            if (_rb == null) EnsureComponents();
            _isKicked = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            if (_trail != null)
            {
                _trail.emitting = false;
            }
        }

        /// <summary>
        /// Teleports the ball to the given position and resets motion.
        /// </summary>
        public void ResetBall(Vector3 position)
        {
            Stop();
            transform.position = position;
            _initialPosition = position;
        }

        /// <summary>
        /// Resets the ball back to its configured starting position.
        /// </summary>
        public void ResetToInitialPosition()
        {
            ResetBall(_initialPosition);
        }

        /// <summary>
        /// Factory helper creating a fully configured regulation football GameObject.
        /// </summary>
        public static GameObject CreateBallGameObject(Transform? parent = null, Vector3? position = null)
        {
            var ballGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballGo.name = "Match_Ball";
            if (parent != null) ballGo.transform.SetParent(parent, false);
            ballGo.transform.position = position ?? new Vector3(0f, 0.11f, 15f);
            ballGo.transform.localScale = Vector3.one * 0.22f;

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader)
            {
                name = "Mat_Match_Ball",
                color = new Color(0.96f, 0.96f, 0.94f)
            };
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.65f);
            ballGo.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Physics component
            var ctrl = ballGo.AddComponent<BallController>();

            // Trail Renderer for trajectory feedback
            var trail = ballGo.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 0.12f;
            trail.endWidth = 0.01f;
            var unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            var trailMat = new Material(unlit)
            {
                color = new Color(1f, 0.85f, 0.2f, 0.7f)
            };
            trail.sharedMaterial = trailMat;
            trail.emitting = false;

            return ballGo;
        }
    }
}
