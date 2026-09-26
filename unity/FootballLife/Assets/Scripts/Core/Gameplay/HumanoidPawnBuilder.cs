#nullable enable
using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Kit color scheme definition for outfield players and goalkeepers.
    /// </summary>
    public readonly struct PawnKitScheme
    {
        public Color JerseyColor { get; }
        public Color ShortsColor { get; }
        public Color SocksColor { get; }
        public Color SkinColor { get; }
        public Color HairColor { get; }

        public PawnKitScheme(Color jersey, Color shorts, Color socks, Color skin, Color hair)
        {
            JerseyColor = jersey;
            ShortsColor = shorts;
            SocksColor = socks;
            SkinColor = skin;
            HairColor = hair;
        }

        // Presets
        public static PawnKitScheme HomeOutfield => new PawnKitScheme(
            new Color(0.10f, 0.25f, 0.65f), // Rich Royal Blue jersey
            new Color(0.95f, 0.95f, 0.95f), // White shorts
            new Color(0.10f, 0.25f, 0.65f), // Blue socks
            new Color(0.92f, 0.76f, 0.62f), // Fair/Tan skin
            new Color(0.20f, 0.15f, 0.10f)  // Dark brown hair
        );

        public static PawnKitScheme AwayOutfield => new PawnKitScheme(
            new Color(0.78f, 0.12f, 0.15f), // Crimson Red jersey
            new Color(0.15f, 0.15f, 0.18f), // Dark navy shorts
            new Color(0.78f, 0.12f, 0.15f), // Red socks
            new Color(0.85f, 0.68f, 0.52f), // Tan skin
            new Color(0.10f, 0.10f, 0.10f)  // Black hair
        );

        public static PawnKitScheme Goalkeeper => new PawnKitScheme(
            new Color(0.82f, 0.95f, 0.12f), // High-visibility fluorescent yellow/lime
            new Color(0.15f, 0.15f, 0.15f), // Black shorts
            new Color(0.82f, 0.95f, 0.12f), // Lime socks
            new Color(0.90f, 0.74f, 0.58f), // Skin
            new Color(0.40f, 0.25f, 0.15f)  // Brown hair
        );
    }

    /// <summary>
    /// References to anatomical limb transforms for procedural articulation.
    /// </summary>
    public sealed class PawnRigTransforms
    {
        public Transform Hips { get; }
        public Transform Torso { get; }
        public Transform Head { get; }
        public Transform LeftArm { get; }
        public Transform RightArm { get; }
        public Transform LeftLeg { get; }
        public Transform RightLeg { get; }
        public Transform LeftFoot { get; }
        public Transform RightFoot { get; }

        public PawnRigTransforms(
            Transform hips, Transform torso, Transform head,
            Transform leftArm, Transform rightArm,
            Transform leftLeg, Transform rightLeg,
            Transform leftFoot, Transform rightFoot)
        {
            Hips = hips;
            Torso = torso;
            Head = head;
            LeftArm = leftArm;
            RightArm = rightArm;
            LeftLeg = leftLeg;
            RightLeg = rightLeg;
            LeftFoot = leftFoot;
            RightFoot = rightFoot;
        }
    }

    /// <summary>
    /// Procedural factory that constructs mobile-optimized, stylized 3D footballer pawns.
    /// Creates clean articulated hierarchies with customized kits and boots.
    /// </summary>
    public static class HumanoidPawnBuilder
    {
        private static Shader? _urpLitShader;

        private static Shader GetShader()
        {
            if (_urpLitShader == null)
            {
                _urpLitShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            }
            return _urpLitShader;
        }

        private static Material CreateMaterial(string name, Color color, float smoothness = 0.2f)
        {
            var mat = new Material(GetShader())
            {
                name = name,
                color = color
            };
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        /// <summary>
        /// Builds a fully articulated footballer pawn GameObject.
        /// </summary>
        public static GameObject CreatePawn(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            string name,
            PawnKitScheme kit,
            bool hasSelectionRing = false)
        {
            var pawnRoot = new GameObject(name);
            pawnRoot.transform.SetParent(parent, false);
            pawnRoot.transform.position = position;
            pawnRoot.transform.rotation = rotation;

            // Character controller / collider
            var col = pawnRoot.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.90f, 0f);
            col.radius = 0.32f;
            col.height = 1.80f;

            // Shared materials
            var jerseyMat = CreateMaterial($"Mat_Jersey_{name}", kit.JerseyColor);
            var shortsMat = CreateMaterial($"Mat_Shorts_{name}", kit.ShortsColor);
            var socksMat = CreateMaterial($"Mat_Socks_{name}", kit.SocksColor);
            var skinMat = CreateMaterial($"Mat_Skin_{name}", kit.SkinColor, 0.1f);
            var hairMat = CreateMaterial($"Mat_Hair_{name}", kit.HairColor, 0.05f);
            var bootMat = CreateMaterial($"Mat_Boot_{name}", new Color(0.12f, 0.12f, 0.12f), 0.4f);

            // ── Hips / Pelvis (Root bone for movement) ──────────────────────────
            var hipsGo = new GameObject("Hips");
            hipsGo.transform.SetParent(pawnRoot.transform, false);
            hipsGo.transform.localPosition = new Vector3(0f, 0.85f, 0f);

            // Shorts / Pelvis mesh
            var pelvisMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pelvisMesh.name = "Shorts_Pelvis";
            pelvisMesh.transform.SetParent(hipsGo.transform, false);
            pelvisMesh.transform.localPosition = Vector3.zero;
            pelvisMesh.transform.localScale = new Vector3(0.40f, 0.22f, 0.24f);
            pelvisMesh.GetComponent<MeshRenderer>().sharedMaterial = shortsMat;
            Object.DestroyImmediate(pelvisMesh.GetComponent<Collider>());

            // ── Torso / Chest (Jersey) ─────────────────────────────────────────
            var torsoGo = new GameObject("Torso");
            torsoGo.transform.SetParent(hipsGo.transform, false);
            torsoGo.transform.localPosition = new Vector3(0f, 0.15f, 0f);

            var torsoMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torsoMesh.name = "Jersey_Chest";
            torsoMesh.transform.SetParent(torsoGo.transform, false);
            torsoMesh.transform.localPosition = new Vector3(0f, 0.20f, 0f);
            torsoMesh.transform.localScale = new Vector3(0.44f, 0.42f, 0.25f);
            torsoMesh.GetComponent<MeshRenderer>().sharedMaterial = jerseyMat;
            Object.DestroyImmediate(torsoMesh.GetComponent<Collider>());

            // ── Head & Hair ───────────────────────────────────────────────────
            var headGo = new GameObject("Head");
            headGo.transform.SetParent(torsoGo.transform, false);
            headGo.transform.localPosition = new Vector3(0f, 0.45f, 0f);

            var headMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            headMesh.name = "Head_Face";
            headMesh.transform.SetParent(headGo.transform, false);
            headMesh.transform.localPosition = new Vector3(0f, 0.14f, 0f);
            headMesh.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
            headMesh.GetComponent<MeshRenderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(headMesh.GetComponent<Collider>());

            var hairMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hairMesh.name = "Hair_Top";
            hairMesh.transform.SetParent(headGo.transform, false);
            hairMesh.transform.localPosition = new Vector3(0f, 0.20f, -0.02f);
            hairMesh.transform.localScale = new Vector3(0.25f, 0.16f, 0.25f);
            hairMesh.GetComponent<MeshRenderer>().sharedMaterial = hairMat;
            Object.DestroyImmediate(hairMesh.GetComponent<Collider>());

            // ── Left Arm (Shoulder pivot) ─────────────────────────────────────
            var leftArmGo = new GameObject("Arm_L");
            leftArmGo.transform.SetParent(torsoGo.transform, false);
            leftArmGo.transform.localPosition = new Vector3(-0.28f, 0.35f, 0f);

            var leftArmMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftArmMesh.name = "Arm_L_Mesh";
            leftArmMesh.transform.SetParent(leftArmGo.transform, false);
            leftArmMesh.transform.localPosition = new Vector3(0f, -0.22f, 0f);
            leftArmMesh.transform.localScale = new Vector3(0.10f, 0.22f, 0.10f);
            leftArmMesh.GetComponent<MeshRenderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(leftArmMesh.GetComponent<Collider>());

            // ── Right Arm (Shoulder pivot) ────────────────────────────────────
            var rightArmGo = new GameObject("Arm_R");
            rightArmGo.transform.SetParent(torsoGo.transform, false);
            rightArmGo.transform.localPosition = new Vector3(0.28f, 0.35f, 0f);

            var rightArmMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightArmMesh.name = "Arm_R_Mesh";
            rightArmMesh.transform.SetParent(rightArmGo.transform, false);
            rightArmMesh.transform.localPosition = new Vector3(0f, -0.22f, 0f);
            rightArmMesh.transform.localScale = new Vector3(0.10f, 0.22f, 0.10f);
            rightArmMesh.GetComponent<MeshRenderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(rightArmMesh.GetComponent<Collider>());

            // ── Left Leg (Hip pivot) ──────────────────────────────────────────
            var leftLegGo = new GameObject("Leg_L");
            leftLegGo.transform.SetParent(hipsGo.transform, false);
            leftLegGo.transform.localPosition = new Vector3(-0.13f, -0.10f, 0f);

            var leftLegMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftLegMesh.name = "Leg_L_Mesh";
            leftLegMesh.transform.SetParent(leftLegGo.transform, false);
            leftLegMesh.transform.localPosition = new Vector3(0f, -0.34f, 0f);
            leftLegMesh.transform.localScale = new Vector3(0.12f, 0.34f, 0.12f);
            leftLegMesh.GetComponent<MeshRenderer>().sharedMaterial = socksMat;
            Object.DestroyImmediate(leftLegMesh.GetComponent<Collider>());

            var leftFootGo = new GameObject("Foot_L");
            leftFootGo.transform.SetParent(leftLegGo.transform, false);
            leftFootGo.transform.localPosition = new Vector3(0f, -0.68f, 0.05f);

            var leftBootMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftBootMesh.name = "Boot_L_Mesh";
            leftBootMesh.transform.SetParent(leftFootGo.transform, false);
            leftBootMesh.transform.localPosition = new Vector3(0f, 0.04f, 0.06f);
            leftBootMesh.transform.localScale = new Vector3(0.12f, 0.08f, 0.22f);
            leftBootMesh.GetComponent<MeshRenderer>().sharedMaterial = bootMat;
            Object.DestroyImmediate(leftBootMesh.GetComponent<Collider>());

            // ── Right Leg (Hip pivot - Kicking leg) ────────────────────────────
            var rightLegGo = new GameObject("Leg_R");
            rightLegGo.transform.SetParent(hipsGo.transform, false);
            rightLegGo.transform.localPosition = new Vector3(0.13f, -0.10f, 0f);

            var rightLegMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightLegMesh.name = "Leg_R_Mesh";
            rightLegMesh.transform.SetParent(rightLegGo.transform, false);
            rightLegMesh.transform.localPosition = new Vector3(0f, -0.34f, 0f);
            rightLegMesh.transform.localScale = new Vector3(0.12f, 0.34f, 0.12f);
            rightLegMesh.GetComponent<MeshRenderer>().sharedMaterial = socksMat;
            Object.DestroyImmediate(rightLegMesh.GetComponent<Collider>());

            var rightFootGo = new GameObject("Foot_R");
            rightFootGo.transform.SetParent(rightLegGo.transform, false);
            rightFootGo.transform.localPosition = new Vector3(0f, -0.68f, 0.05f);

            var rightBootMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightBootMesh.name = "Boot_R_Mesh";
            rightBootMesh.transform.SetParent(rightFootGo.transform, false);
            rightBootMesh.transform.localPosition = new Vector3(0f, 0.04f, 0.06f);
            rightBootMesh.transform.localScale = new Vector3(0.12f, 0.08f, 0.22f);
            rightBootMesh.GetComponent<MeshRenderer>().sharedMaterial = bootMat;
            Object.DestroyImmediate(rightBootMesh.GetComponent<Collider>());

            // ── Selection Ring ────────────────────────────────────────────────
            GameObject? ringGo = null;
            if (hasSelectionRing)
            {
                ringGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ringGo.name = "Selection_Ring";
                ringGo.transform.SetParent(pawnRoot.transform, false);
                ringGo.transform.localPosition = new Vector3(0f, 0.02f, 0f);
                ringGo.transform.localScale = new Vector3(1.1f, 0.01f, 1.1f);
                var ringMat = CreateMaterial("Mat_SelectionRing", new Color(0.95f, 0.85f, 0.15f, 0.75f), 0.5f);
                ringGo.GetComponent<MeshRenderer>().sharedMaterial = ringMat;
                Object.DestroyImmediate(ringGo.GetComponent<Collider>());
            }

            // Bind Rig Transforms to controller
            var rig = new PawnRigTransforms(
                hipsGo.transform, torsoGo.transform, headGo.transform,
                leftArmGo.transform, rightArmGo.transform,
                leftLegGo.transform, rightLegGo.transform,
                leftFootGo.transform, rightFootGo.transform
            );

            var pawnController = pawnRoot.AddComponent<PlayerPawnController>();
            pawnController.InitializeRig(rig);

            var matchPawn = pawnRoot.AddComponent<MatchPawn>();
            if (ringGo != null)
            {
                matchPawn.AssignSelectionRing(ringGo);
            }

            return pawnRoot;
        }
    }
}
