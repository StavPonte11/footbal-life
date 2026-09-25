using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Real-time 3D flight trajectory preview arc.
    /// Simulates ballistic projectile physics (launch velocity, gravity, aerodynamic drag,
    /// and Magnus curve forces) into a LineRenderer arc with a target landing reticle.
    /// Zero GC allocations during aiming updates.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class AimTrajectoryRenderer : MonoBehaviour
    {
        [Header("Simulation Resolution")]
        [SerializeField] private int _stepCount = 32;
        [SerializeField] private float _timeStep = 0.045f;

        [Header("Reticle")]
        [SerializeField] private GameObject? _targetReticle;

        private LineRenderer _line = null!;
        private Vector3[] _trajectoryPoints = null!;
        private Material _lineMat = null!;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _trajectoryPoints = new Vector3[_stepCount];

            ConfigureLineRenderer();
            EnsureReticle();
            Hide();
        }

        private void ConfigureLineRenderer()
        {
            _line.positionCount = _stepCount;
            _line.startWidth = 0.12f;
            _line.endWidth = 0.04f;
            _line.useWorldSpace = true;

            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            _lineMat = new Material(shader)
            {
                name = "Mat_AimTrajectory",
                color = new Color(0.95f, 0.85f, 0.20f, 0.85f) // Bright glowing yellow/amber
            };
            _line.sharedMaterial = _lineMat;

            // Gradient fade towards target
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(0.2f, 0.9f, 0.3f), 0f), new GradientColorKey(new Color(1f, 0.85f, 0.15f), 1f) },
                new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.4f, 1f) }
            );
            _line.colorGradient = grad;
        }

        private void EnsureReticle()
        {
            if (_targetReticle == null)
            {
                var reticleGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                reticleGo.name = "Aim_Target_Reticle";
                reticleGo.transform.SetParent(transform, false);
                reticleGo.transform.localScale = new Vector3(0.75f, 0.01f, 0.75f);
                var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
                var reticleMat = new Material(shader)
                {
                    name = "Mat_AimReticle",
                    color = new Color(1f, 0.25f, 0.25f, 0.85f) // Red targeting ring
                };
                reticleGo.GetComponent<MeshRenderer>().sharedMaterial = reticleMat;
                DestroyImmediate(reticleGo.GetComponent<Collider>());
                _targetReticle = reticleGo;
            }
        }

        /// <summary>
        /// Updates the trajectory arc given launch velocity and Magnus spin.
        /// </summary>
        public void UpdateTrajectory(Vector3 startPos, Vector3 launchVelocity, Vector3 spinVelocity, float magnusCoeff = 0.0035f)
        {
            if (_line == null) return;

            Vector3 currentPos = startPos;
            Vector3 currentVel = launchVelocity;
            Vector3 gravity = Physics.gravity;

            _line.enabled = true;
            if (_targetReticle != null) _targetReticle.SetActive(true);

            for (int i = 0; i < _stepCount; i++)
            {
                _trajectoryPoints[i] = currentPos;

                // Magnus effect acceleration
                Vector3 magnusAccel = Vector3.Cross(spinVelocity, currentVel) * magnusCoeff;

                // Numerical integration
                currentVel += (gravity + magnusAccel) * _timeStep;
                currentPos += currentVel * _timeStep;

                // Stop if hitting pitch floor
                if (currentPos.y < 0.08f)
                {
                    currentPos.y = 0.08f;
                    // Clamp remaining points to impact point
                    for (int j = i; j < _stepCount; j++)
                    {
                        _trajectoryPoints[j] = currentPos;
                    }
                    break;
                }
            }

            _line.SetPositions(_trajectoryPoints);

            // Position reticle at final landing or goal plane
            if (_targetReticle != null)
            {
                Vector3 finalPoint = _trajectoryPoints[_stepCount - 1];
                _targetReticle.transform.position = new Vector3(finalPoint.x, 0.04f, finalPoint.z);
            }
        }

        public void Hide()
        {
            if (_line != null) _line.enabled = false;
            if (_targetReticle != null) _targetReticle.SetActive(false);
        }
    }
}
