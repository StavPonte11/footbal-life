using System;
using System.Collections.Generic;
using UnityEngine;
using FootballLife.Domain;
using FootballLife.Unity.Core.Gameplay;

namespace FootballLife.Unity.Core.Environment
{
    /// <summary>
    /// Manages the procedural mobile-budget stadium spectator crowd.
    /// Batches spectator seating geometry to strictly comply with mobile frame (60 FPS) and memory (&lt;200MB) budgets.
    /// Reacts to match highlights (Murmur -> Roar -> Gasp) with zero per-frame GC allocations.
    /// </summary>
    public sealed class CrowdController : MonoBehaviour
    {
        public static CrowdController? Instance { get; private set; }

        [Header("State")]
        [SerializeField] private CrowdExcitementState _currentState = CrowdExcitementState.Murmur;
        [SerializeField] private float _excitementDuration = 0f;

        // References to the 4 grandstand crowd root transforms
        private readonly Transform?[] _standRoots = new Transform?[4];
        private readonly Vector3[] _standBasePositions = new Vector3[4];

        // Cached sine oscillation phases to avoid per-frame allocations
        private float _animTime = 0f;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            GoalTrigger.OnGoalScored += HandleGoalScored;
            BallController.OnWoodworkHit += HandleWoodworkHit;
        }

        private void OnDisable()
        {
            GoalTrigger.OnGoalScored -= HandleGoalScored;
            BallController.OnWoodworkHit -= HandleWoodworkHit;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _animTime += dt;

            if (_excitementDuration > 0f)
            {
                _excitementDuration -= dt;
                if (_excitementDuration <= 0f)
                {
                    _currentState = CrowdExcitementState.Murmur;
                }
            }

            AnimateCrowdStands(dt);
        }

        private void AnimateCrowdStands(float dt)
        {
            for (int i = 0; i < 4; i++)
            {
                var tr = _standRoots[i];
                if (tr == null) continue;

                Vector3 basePos = _standBasePositions[i];
                float standPhase = i * 1.57f; // 90 degree offset per stand

                switch (_currentState)
                {
                    case CrowdExcitementState.Roar:
                    {
                        // Explosive jumping and celebratory Mexican-wave ripple
                        float wave = Mathf.Sin((_animTime * 14f) + standPhase);
                        float jumpY = Mathf.Max(0f, wave) * 0.42f;
                        tr.localPosition = new Vector3(basePos.x, basePos.y + jumpY, basePos.z);
                        break;
                    }
                    case CrowdExcitementState.Gasp:
                    {
                        // Immediate collective recoil backwards and slight dip
                        float recoil = Mathf.Sin(Mathf.Clamp01(1f - (_excitementDuration / 2.2f)) * Mathf.PI) * 0.35f;
                        tr.localPosition = new Vector3(basePos.x, basePos.y - (recoil * 0.15f), basePos.z - (recoil * 0.3f));
                        break;
                    }
                    case CrowdExcitementState.Anticipation:
                    {
                        // Supporter rise to tiptoes leaning forward
                        float riseY = Mathf.PingPong(_animTime * 4f, 0.15f);
                        tr.localPosition = new Vector3(basePos.x, basePos.y + riseY, basePos.z);
                        break;
                    }
                    case CrowdExcitementState.Murmur:
                    default:
                    {
                        // Gentle ambient sway & breathing rhythm
                        float swayY = Mathf.Sin((_animTime * 2.2f) + standPhase) * 0.035f;
                        tr.localPosition = new Vector3(basePos.x, basePos.y + swayY, basePos.z);
                        break;
                    }
                }
            }
        }

        private void HandleGoalScored(GoalScoredEvent evt)
        {
            TriggerGoalRoar();
        }

        private void HandleWoodworkHit(Vector3 hitPoint)
        {
            TriggerGasp();
        }

        public void TriggerGoalRoar(float duration = 6.0f)
        {
            _currentState = CrowdExcitementState.Roar;
            _excitementDuration = duration;
        }

        public void TriggerGasp(float duration = 2.2f)
        {
            _currentState = CrowdExcitementState.Gasp;
            _excitementDuration = duration;
        }

        public void TriggerAnticipation(float duration = 3.5f)
        {
            _currentState = CrowdExcitementState.Anticipation;
            _excitementDuration = duration;
        }

        public void RegisterStand(int standIndex, Transform standTransform)
        {
            if (standIndex >= 0 && standIndex < 4)
            {
                _standRoots[standIndex] = standTransform;
                _standBasePositions[standIndex] = standTransform.localPosition;
            }
        }

        /// <summary>
        /// Procedurally generates batched spectator seating meshes for all 4 stadium stands.
        /// </summary>
        public static CrowdController CreateCrowdSystem(
            Transform stadiumRoot,
            StadiumReputationTier tier,
            Color homePrimary,
            Color awayPrimary)
        {
            var crowdGo = new GameObject("Crowd_Spectator_System");
            crowdGo.transform.SetParent(stadiumRoot, false);
            var controller = crowdGo.AddComponent<CrowdController>();

            var tierConfig = StadiumAtmosphereUtility.GetTierConfig(tier);

            var crowdMat = CreateCrowdMaterial();

            // Neutral supporter wardrobe colors
            Color[] neutralColors = new[]
            {
                new Color(0.18f, 0.20f, 0.24f), // Charcoal navy
                new Color(0.65f, 0.67f, 0.70f), // Heather grey
                new Color(0.92f, 0.92f, 0.94f), // White supporter jersey
                new Color(0.75f, 0.20f, 0.18f), // Red scarf
                new Color(0.95f, 0.78f, 0.20f)  // Amber jacket
            };

            float pitchHalfW = PitchBuilder.kPitchWidth * 0.5f;
            float margin = 4.5f;

            // 0: North Stand (Goal End)
            Vector3 northOrigin = new Vector3(0f, 0f, PitchBuilder.kGoalLineZ + margin);
            var northStand = BuildStandSpectators(crowdGo.transform, "Crowd_North_Stand", northOrigin, Quaternion.identity, 74f, tierConfig, crowdMat, homePrimary, awayPrimary, neutralColors, 0);
            controller.RegisterStand(0, northStand.transform);

            // 1: South Stand (Away End - higher away fan density)
            Vector3 southOrigin = new Vector3(0f, 0f, -14f - margin);
            var southStand = BuildStandSpectators(crowdGo.transform, "Crowd_South_Stand", southOrigin, Quaternion.Euler(0f, 180f, 0f), 74f, tierConfig, crowdMat, homePrimary, awayPrimary, neutralColors, 1);
            controller.RegisterStand(1, southStand.transform);

            // 2: East Stand (Sideline)
            Vector3 eastOrigin = new Vector3(pitchHalfW + margin, 0f, 10f);
            var eastStand = BuildStandSpectators(crowdGo.transform, "Crowd_East_Stand", eastOrigin, Quaternion.Euler(0f, 90f, 0f), 63f, tierConfig, crowdMat, homePrimary, awayPrimary, neutralColors, 2);
            controller.RegisterStand(2, eastStand.transform);

            // 3: West Stand (Main Stand)
            Vector3 westOrigin = new Vector3(-pitchHalfW - margin, 0f, 10f);
            var westStand = BuildStandSpectators(crowdGo.transform, "Crowd_West_Stand", westOrigin, Quaternion.Euler(0f, -90f, 0f), 63f, tierConfig, crowdMat, homePrimary, awayPrimary, neutralColors, 3);
            controller.RegisterStand(3, westStand.transform);

            return controller;
        }

        private static GameObject BuildStandSpectators(
            Transform parent,
            string name,
            Vector3 pos,
            Quaternion rot,
            float width,
            StadiumTierConfig config,
            Material crowdMat,
            Color homeCol,
            Color awayCol,
            Color[] neutralColors,
            int standIndex)
        {
            var standGo = new GameObject(name);
            standGo.transform.SetParent(parent, false);
            standGo.transform.position = pos;
            standGo.transform.rotation = rot;

            var mesh = GenerateStandSpectatorMesh(width, config, homeCol, awayCol, neutralColors, standIndex);
            var mf = standGo.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = standGo.AddComponent<MeshRenderer>();
            mr.sharedMaterial = crowdMat;

            return standGo;
        }

        private static Mesh GenerateStandSpectatorMesh(
            float width,
            StadiumTierConfig config,
            Color homeCol,
            Color awayCol,
            Color[] neutrals,
            int standIndex)
        {
            var mesh = new Mesh { name = $"Mesh_Spectators_{standIndex}" };
            var verts = new List<Vector3>();
            var colors = new List<Color>();
            var tris = new List<int>();

            int rows = config.SpectatorDensityRows;
            float stepDepth = config.StandDepthMeters / config.RowSteps;
            float stepHeight = config.StandHeightMeters / config.RowSteps;

            float spectatorSpacing = 0.85f;
            int spectatorsPerRow = Mathf.FloorToInt((width - 4f) / spectatorSpacing);
            float startX = -((spectatorsPerRow - 1) * spectatorSpacing) * 0.5f;

            var rnd = new System.Random(standIndex * 7919 + 42);

            for (int r = 0; r < rows; r++)
            {
                float z = (r * stepDepth) + (stepDepth * 0.45f);
                float y = (r + 1) * stepHeight + 0.22f; // resting on top of bench seat

                for (int s = 0; s < spectatorsPerRow; s++)
                {
                    // Random aisle gap
                    if (s % 9 == 0 && s > 0) continue;

                    float x = startX + (s * spectatorSpacing) + ((float)rnd.NextDouble() * 0.1f - 0.05f);

                    // Pick fan shirt color
                    Color fanShirtColor;
                    double pick = rnd.NextDouble();
                    if (standIndex == 1 && s > spectatorsPerRow * 0.65f)
                    {
                        // Away section in South Stand
                        fanShirtColor = awayCol;
                    }
                    else if (pick < 0.58)
                    {
                        fanShirtColor = homeCol;
                    }
                    else
                    {
                        fanShirtColor = neutrals[rnd.Next(neutrals.Length)];
                    }

                    Color skinColor = new Color(0.85f, 0.68f, 0.55f);

                    // 1. Torso Quad (facing forward along -Z)
                    float torsoW = 0.36f;
                    float torsoH = 0.48f;
                    int tStart = verts.Count;

                    verts.Add(new Vector3(x - torsoW * 0.5f, y, z));
                    verts.Add(new Vector3(x + torsoW * 0.5f, y, z));
                    verts.Add(new Vector3(x + torsoW * 0.5f, y + torsoH, z));
                    verts.Add(new Vector3(x - torsoW * 0.5f, y + torsoH, z));

                    colors.Add(fanShirtColor);
                    colors.Add(fanShirtColor);
                    colors.Add(fanShirtColor);
                    colors.Add(fanShirtColor);

                    tris.Add(tStart); tris.Add(tStart + 2); tris.Add(tStart + 1);
                    tris.Add(tStart); tris.Add(tStart + 3); tris.Add(tStart + 2);

                    // 2. Head Quad
                    float headSize = 0.24f;
                    float headY = y + torsoH + 0.02f;
                    int hStart = verts.Count;

                    verts.Add(new Vector3(x - headSize * 0.5f, headY, z));
                    verts.Add(new Vector3(x + headSize * 0.5f, headY, z));
                    verts.Add(new Vector3(x + headSize * 0.5f, headY + headSize, z));
                    verts.Add(new Vector3(x - headSize * 0.5f, headY + headSize, z));

                    colors.Add(skinColor);
                    colors.Add(skinColor);
                    colors.Add(skinColor);
                    colors.Add(skinColor);

                    tris.Add(hStart); tris.Add(hStart + 2); tris.Add(hStart + 1);
                    tris.Add(hStart); tris.Add(hStart + 3); tris.Add(hStart + 2);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetColors(colors);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Material CreateCrowdMaterial()
        {
            // Vertex color lit shader for zero extra texture memory
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader)
            {
                name = "Mat_Crowd_Spectators",
                color = Color.white
            };
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);
            return mat;
        }
    }
}
