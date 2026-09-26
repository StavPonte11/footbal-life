using System;
using UnityEngine;
using UnityEngine.Pool;
using FootballLife.Domain;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// High-performance object pool for match turf dust and grass blade visual effects.
    /// Strictly adheres to zero GC allocations in hot paths using UnityEngine.Pool.ObjectPool.
    /// </summary>
    public sealed class TurfVfxPool : MonoBehaviour
    {
        public static TurfVfxPool? Instance { get; private set; }

        [SerializeField] private int _defaultPoolCapacity = 12;
        [SerializeField] private int _maxPoolSize = 24;

        private ObjectPool<ParticleSystem>? _pool;
        private Material? _particleMaterial;

        private void Awake()
        {
            Instance = this;
            InitializePool();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            _pool?.Clear();
        }

        private void InitializePool()
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                         Shader.Find("Mobile/Particles/Additive") ??
                         Shader.Find("Standard");

            _particleMaterial = new Material(shader)
            {
                name = "Mat_Turf_VFX_Particle",
                color = new Color(0.28f, 0.48f, 0.22f, 0.8f) // Grass turf tint
            };

            _pool = new ObjectPool<ParticleSystem>(
                createFunc: CreateNewParticleSystem,
                actionOnGet: ps => ps.gameObject.SetActive(true),
                actionOnRelease: ps =>
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.gameObject.SetActive(false);
                },
                actionOnDestroy: ps =>
                {
                    if (ps != null) Destroy(ps.gameObject);
                },
                collectionCheck: false,
                defaultCapacity: _defaultPoolCapacity,
                maxSize: _maxPoolSize
            );
        }

        private ParticleSystem CreateNewParticleSystem()
        {
            var go = new GameObject("Turf_Particle_Instance");
            go.transform.SetParent(transform, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.65f;
            main.startLifetime = 0.55f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            main.gravityModifier = 1.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.None;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.15f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(0.35f, 0.55f, 0.25f), 0f), new GradientColorKey(new Color(0.45f, 0.38f, 0.28f), 1f) },
                new[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (_particleMaterial != null) renderer.sharedMaterial = _particleMaterial;

            go.AddComponent<PooledParticleReleaser>().Init(this, ps);
            go.SetActive(false);
            return ps;
        }

        public void ReleaseToPool(ParticleSystem ps)
        {
            _pool?.Release(ps);
        }

        /// <summary>
        /// Spawns a turf dust and grass blade burst at the ball kick point.
        /// </summary>
        public void SpawnKickDust(Vector3 position, Vector3 kickVelocity, float power01 = 0.5f)
        {
            if (_pool == null) return;

            var ps = _pool.Get();
            ps.transform.position = new Vector3(position.x, 0.04f, position.z);
            ps.transform.rotation = Quaternion.LookRotation(new Vector3(kickVelocity.x, 0.5f, kickVelocity.z).normalized);

            int count = StadiumAtmosphereUtility.ComputeTurfParticleCount(power01, isSlide: false);
            ps.Emit(count);
        }

        /// <summary>
        /// Spawns an elongated spray of turf furrow particles along a player's slide tackle path.
        /// </summary>
        public void SpawnSlideDust(Vector3 position, Vector3 slideDirection, float power01 = 0.75f)
        {
            if (_pool == null) return;

            var ps = _pool.Get();
            ps.transform.position = new Vector3(position.x, 0.03f, position.z);
            ps.transform.rotation = Quaternion.LookRotation(new Vector3(slideDirection.x, 0.2f, slideDirection.z).normalized);

            int count = StadiumAtmosphereUtility.ComputeTurfParticleCount(power01, isSlide: true);
            ps.Emit(count);
        }

        /// <summary>
        /// Spawns a radial turf impact puff when the ball strikes the grass at speed.
        /// </summary>
        public void SpawnBallBounceDust(Vector3 position, float impactSpeed)
        {
            if (_pool == null || impactSpeed < 3.0f) return;

            var ps = _pool.Get();
            ps.transform.position = new Vector3(position.x, 0.03f, position.z);
            ps.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // Upwards fountain

            float power = Mathf.Clamp01(impactSpeed / 20.0f);
            int count = Mathf.Clamp((int)(power * 16), 4, 18);
            ps.Emit(count);
        }
    }

    /// <summary>
    /// Helper component attached to pooled particle systems to release them back after duration.
    /// </summary>
    internal sealed class PooledParticleReleaser : MonoBehaviour
    {
        private TurfVfxPool? _pool;
        private ParticleSystem? _ps;
        private float _releaseTimer = 0f;
        private bool _active = false;

        public void Init(TurfVfxPool pool, ParticleSystem ps)
        {
            _pool = pool;
            _ps = ps;
        }

        private void OnEnable()
        {
            _releaseTimer = 0.75f;
            _active = true;
        }

        private void Update()
        {
            if (!_active) return;
            _releaseTimer -= Time.deltaTime;
            if (_releaseTimer <= 0f)
            {
                _active = false;
                if (_pool != null && _ps != null)
                {
                    _pool.ReleaseToPool(_ps);
                }
            }
        }
    }
}
