#nullable enable
using System;
using FootballLife.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

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
        public Color NumberColor { get; }

        public PawnKitScheme(Color jersey, Color shorts, Color socks, Color skin, Color hair, Color? numberColor = null)
        {
            JerseyColor = jersey;
            ShortsColor = shorts;
            SocksColor = socks;
            SkinColor = skin;
            HairColor = hair;
            NumberColor = numberColor ?? (jersey.grayscale > 0.5f ? new Color(0.1f, 0.1f, 0.1f) : Color.white);
        }

        public PawnKitScheme WithVisualProfile(PlayerVisualProfile profile)
        {
            var skin = profile.GetSkinColor();
            var hair = profile.GetHairColor();
            return new PawnKitScheme(
                JerseyColor,
                ShortsColor,
                SocksColor,
                new Color(skin.R, skin.G, skin.B),
                new Color(hair.R, hair.G, hair.B),
                NumberColor
            );
        }

        // Presets
        public static PawnKitScheme HomeOutfield => new PawnKitScheme(
            new Color(0.10f, 0.25f, 0.65f), // Rich Royal Blue jersey
            new Color(0.95f, 0.95f, 0.95f), // White shorts
            new Color(0.10f, 0.25f, 0.65f), // Blue socks
            new Color(0.92f, 0.76f, 0.62f), // Fair/Tan skin
            new Color(0.20f, 0.15f, 0.10f), // Dark brown hair
            new Color(0.95f, 0.95f, 0.95f)  // White number
        );

        public static PawnKitScheme AwayOutfield => new PawnKitScheme(
            new Color(0.78f, 0.12f, 0.15f), // Crimson Red jersey
            new Color(0.15f, 0.15f, 0.18f), // Dark navy shorts
            new Color(0.78f, 0.12f, 0.15f), // Red socks
            new Color(0.85f, 0.68f, 0.52f), // Tan skin
            new Color(0.10f, 0.10f, 0.10f), // Black hair
            new Color(0.95f, 0.95f, 0.95f)  // White number
        );

        public static PawnKitScheme Goalkeeper => new PawnKitScheme(
            new Color(0.82f, 0.95f, 0.12f), // High-visibility fluorescent yellow/lime
            new Color(0.15f, 0.15f, 0.15f), // Black shorts
            new Color(0.82f, 0.95f, 0.12f), // Lime socks
            new Color(0.90f, 0.74f, 0.58f), // Skin
            new Color(0.40f, 0.25f, 0.15f), // Brown hair
            new Color(0.10f, 0.10f, 0.10f)  // Black number
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
    /// Uses stylized athletic mesh geometry, standard Humanoid bone hierarchy & Avatar,
    /// dynamic kit colors, squad numbers, and player visual profiles.
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

        public static Material CreateMaterial(string name, Color color, float smoothness = 0.2f)
        {
            var mat = new Material(GetShader())
            {
                name = name,
                color = color
            };
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        public static Color GetBootColor(BootStyleType bootStyle) => bootStyle switch
        {
            BootStyleType.ClassicBlack => new Color(0.12f, 0.12f, 0.12f),
            BootStyleType.NeonSpeed => new Color(0.88f, 0.98f, 0.12f),
            BootStyleType.CleanWhite => new Color(0.96f, 0.96f, 0.96f),
            BootStyleType.CrimsonStrike => new Color(0.85f, 0.12f, 0.18f),
            _ => new Color(0.12f, 0.12f, 0.12f)
        };

        /// <summary>
        /// Builds a fully articulated stylized footballer pawn GameObject with standard Humanoid rigging.
        /// </summary>
        public static GameObject CreatePawn(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            string name,
            PawnKitScheme kit,
            bool hasSelectionRing = false,
            PlayerVisualProfile? visualProfile = null,
            int squadNumber = 9,
            bool isGoalkeeper = false)
        {
            // Fallback or deterministic visual profile
            var profile = visualProfile ?? PlayerVisualProfile.CreateDeterministic(
                name, squadNumber, isGoalkeeper ? Position.GK : Position.ST
            );

            // Sync kit with visual profile skin and hair
            var resolvedKit = kit.WithVisualProfile(profile);

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
            var jerseyMat = CreateMaterial($"Mat_Jersey_{name}", resolvedKit.JerseyColor, 0.25f);
            var shortsMat = CreateMaterial($"Mat_Shorts_{name}", resolvedKit.ShortsColor, 0.20f);
            var socksMat = CreateMaterial($"Mat_Socks_{name}", resolvedKit.SocksColor, 0.20f);
            var skinMat = CreateMaterial($"Mat_Skin_{name}", resolvedKit.SkinColor, 0.10f);
            var hairMat = CreateMaterial($"Mat_Hair_{name}", resolvedKit.HairColor, 0.05f);
            var bootColor = GetBootColor(profile.BootStyle);
            var bootMat = CreateMaterial($"Mat_Boot_{name}", bootColor, 0.45f);

            // ── Hips / Pelvis (Root bone for movement) ──────────────────────────
            var hipsGo = new GameObject("Hips");
            hipsGo.transform.SetParent(pawnRoot.transform, false);
            hipsGo.transform.localPosition = new Vector3(0f, 0.85f, 0f);

            // Stylized Athletic Shorts
            var pelvisMesh = new GameObject("Shorts_Pelvis");
            pelvisMesh.transform.SetParent(hipsGo.transform, false);
            pelvisMesh.transform.localPosition = new Vector3(0f, -0.10f, 0f);
            var pelvisFilter = pelvisMesh.AddComponent<MeshFilter>();
            pelvisFilter.sharedMesh = StylizedMeshGenerator.CreateAthleticShorts();
            var pelvisRenderer = pelvisMesh.AddComponent<MeshRenderer>();
            pelvisRenderer.sharedMaterial = shortsMat;

            // ── Torso / Chest (Jersey) ─────────────────────────────────────────
            var torsoGo = new GameObject("Torso");
            torsoGo.transform.SetParent(hipsGo.transform, false);
            torsoGo.transform.localPosition = new Vector3(0f, 0.15f, 0f);

            var torsoMesh = new GameObject("Jersey_Chest");
            torsoMesh.transform.SetParent(torsoGo.transform, false);
            torsoMesh.transform.localPosition = new Vector3(0f, 0f, 0f);
            var torsoFilter = torsoMesh.AddComponent<MeshFilter>();
            torsoFilter.sharedMesh = StylizedMeshGenerator.CreateAthleticTorso(isGoalkeeper);
            var torsoRenderer = torsoMesh.AddComponent<MeshRenderer>();
            torsoRenderer.sharedMaterial = jerseyMat;

            // Jersey Back Squad Number Badge (Quad)
            var numberGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            numberGo.name = "Jersey_Squad_Number";
            numberGo.transform.SetParent(torsoMesh.transform, false);
            numberGo.transform.localPosition = new Vector3(0f, 0.26f, -0.125f);
            numberGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            numberGo.transform.localScale = new Vector3(0.20f, 0.20f, 1f);
            var numberTex = StylizedMeshGenerator.CreateSquadNumberTexture(
                squadNumber, resolvedKit.NumberColor, resolvedKit.JerseyColor
            );
            var numberMat = CreateMaterial($"Mat_Number_{name}", Color.white, 0.1f);
            numberMat.mainTexture = numberTex;
            numberGo.GetComponent<MeshRenderer>().sharedMaterial = numberMat;
            Object.DestroyImmediate(numberGo.GetComponent<Collider>());

            // ── Head & Hair ───────────────────────────────────────────────────
            var headGo = new GameObject("Head");
            headGo.transform.SetParent(torsoGo.transform, false);
            headGo.transform.localPosition = new Vector3(0f, 0.44f, 0f);

            var headMesh = new GameObject("Head_Face");
            headMesh.transform.SetParent(headGo.transform, false);
            headMesh.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            var headFilter = headMesh.AddComponent<MeshFilter>();
            headFilter.sharedMesh = StylizedMeshGenerator.CreateContouredHead();
            var headRenderer = headMesh.AddComponent<MeshRenderer>();
            headRenderer.sharedMaterial = skinMat;

            // Modular Stylized Hair
            var hairMesh = new GameObject("Hair_Top");
            hairMesh.transform.SetParent(headGo.transform, false);
            hairMesh.transform.localPosition = new Vector3(0f, 0.18f, -0.01f);
            var hairFilter = hairMesh.AddComponent<MeshFilter>();
            hairFilter.sharedMesh = StylizedMeshGenerator.CreateModularHair(profile.HairStyle);
            var hairRenderer = hairMesh.AddComponent<MeshRenderer>();
            hairRenderer.sharedMaterial = hairMat;

            // Stylized Eyes (Left & Right)
            void CreateEye(string eyeName, Vector3 localPos)
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Quad);
                eye.name = eyeName;
                eye.transform.SetParent(headGo.transform, false);
                eye.transform.localPosition = localPos;
                eye.transform.localRotation = Quaternion.identity;
                eye.transform.localScale = new Vector3(0.035f, 0.025f, 1f);
                var eyeMat = CreateMaterial("Mat_StylizedEye", new Color(0.12f, 0.12f, 0.15f), 0.8f);
                eye.GetComponent<MeshRenderer>().sharedMaterial = eyeMat;
                Object.DestroyImmediate(eye.GetComponent<Collider>());
            }
            CreateEye("Eye_L", new Vector3(-0.045f, 0.17f, 0.075f));
            CreateEye("Eye_R", new Vector3( 0.045f, 0.17f, 0.075f));

            // Facial Hair (if not clean shaven)
            if (profile.FacialHair != FacialHairType.CleanShaven)
            {
                var beardGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
                beardGo.name = "Beard_Accent";
                beardGo.transform.SetParent(headGo.transform, false);
                beardGo.transform.localPosition = new Vector3(0f, 0.045f, 0.065f);
                beardGo.transform.localScale = new Vector3(0.08f, 0.06f, 1f);
                var beardMat = CreateMaterial("Mat_Beard", resolvedKit.HairColor * 0.9f, 0.05f);
                beardGo.GetComponent<MeshRenderer>().sharedMaterial = beardMat;
                Object.DestroyImmediate(beardGo.GetComponent<Collider>());
            }

            // ── Left Arm (Shoulder pivot) ─────────────────────────────────────
            var leftArmGo = new GameObject("Arm_L");
            leftArmGo.transform.SetParent(torsoGo.transform, false);
            leftArmGo.transform.localPosition = new Vector3(-0.28f, 0.35f, 0f);

            var leftArmMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftArmMesh.name = "Arm_L_Mesh";
            leftArmMesh.transform.SetParent(leftArmGo.transform, false);
            leftArmMesh.transform.localPosition = new Vector3(0f, -0.20f, 0f);
            leftArmMesh.transform.localScale = new Vector3(0.09f, 0.20f, 0.09f);
            leftArmMesh.GetComponent<MeshRenderer>().sharedMaterial = isGoalkeeper ? jerseyMat : skinMat;
            Object.DestroyImmediate(leftArmMesh.GetComponent<Collider>());

            // Left Hand / GK Glove
            var leftHandGo = new GameObject("Hand_L");
            leftHandGo.transform.SetParent(leftArmGo.transform, false);
            leftHandGo.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            if (isGoalkeeper)
            {
                var gkGloveL = new GameObject("GK_Glove_L");
                gkGloveL.transform.SetParent(leftHandGo.transform, false);
                var gloveFilterL = gkGloveL.AddComponent<MeshFilter>();
                gloveFilterL.sharedMesh = StylizedMeshGenerator.CreateGoalkeeperGlove(true);
                var gloveRendererL = gkGloveL.AddComponent<MeshRenderer>();
                var gloveMat = CreateMaterial($"Mat_GKGlove_{name}", new Color(0.95f, 0.45f, 0.10f), 0.35f);
                gloveRendererL.sharedMaterial = gloveMat;
            }

            // ── Right Arm (Shoulder pivot) ────────────────────────────────────
            var rightArmGo = new GameObject("Arm_R");
            rightArmGo.transform.SetParent(torsoGo.transform, false);
            rightArmGo.transform.localPosition = new Vector3(0.28f, 0.35f, 0f);

            var rightArmMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightArmMesh.name = "Arm_R_Mesh";
            rightArmMesh.transform.SetParent(rightArmGo.transform, false);
            rightArmMesh.transform.localPosition = new Vector3(0f, -0.20f, 0f);
            rightArmMesh.transform.localScale = new Vector3(0.09f, 0.20f, 0.09f);
            rightArmMesh.GetComponent<MeshRenderer>().sharedMaterial = isGoalkeeper ? jerseyMat : skinMat;
            Object.DestroyImmediate(rightArmMesh.GetComponent<Collider>());

            // Right Hand / GK Glove
            var rightHandGo = new GameObject("Hand_R");
            rightHandGo.transform.SetParent(rightArmGo.transform, false);
            rightHandGo.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            if (isGoalkeeper)
            {
                var gkGloveR = new GameObject("GK_Glove_R");
                gkGloveR.transform.SetParent(rightHandGo.transform, false);
                var gloveFilterR = gkGloveR.AddComponent<MeshFilter>();
                gloveFilterR.sharedMesh = StylizedMeshGenerator.CreateGoalkeeperGlove(false);
                var gloveRendererR = gkGloveR.AddComponent<MeshRenderer>();
                var gloveMat = CreateMaterial($"Mat_GKGlove_{name}", new Color(0.95f, 0.45f, 0.10f), 0.35f);
                gloveRendererR.sharedMaterial = gloveMat;
            }

            // ── Left Leg (Hip pivot) ──────────────────────────────────────────
            var leftLegGo = new GameObject("Leg_L");
            leftLegGo.transform.SetParent(hipsGo.transform, false);
            leftLegGo.transform.localPosition = new Vector3(-0.13f, -0.10f, 0f);

            var leftLegMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftLegMesh.name = "Leg_L_Mesh";
            leftLegMesh.transform.SetParent(leftLegGo.transform, false);
            leftLegMesh.transform.localPosition = new Vector3(0f, -0.34f, 0f);
            leftLegMesh.transform.localScale = new Vector3(0.11f, 0.34f, 0.11f);
            leftLegMesh.GetComponent<MeshRenderer>().sharedMaterial = socksMat;
            Object.DestroyImmediate(leftLegMesh.GetComponent<Collider>());

            // Left Foot & Athletic Boot
            var leftFootGo = new GameObject("Foot_L");
            leftFootGo.transform.SetParent(leftLegGo.transform, false);
            leftFootGo.transform.localPosition = new Vector3(0f, -0.68f, 0.05f);

            var leftBootMesh = new GameObject("Boot_L_Mesh");
            leftBootMesh.transform.SetParent(leftFootGo.transform, false);
            leftBootMesh.transform.localPosition = new Vector3(0f, 0.02f, 0.02f);
            var leftBootFilter = leftBootMesh.AddComponent<MeshFilter>();
            leftBootFilter.sharedMesh = StylizedMeshGenerator.CreateAthleticBoot(true);
            var leftBootRenderer = leftBootMesh.AddComponent<MeshRenderer>();
            leftBootRenderer.sharedMaterial = bootMat;

            // ── Right Leg (Hip pivot - Kicking leg) ────────────────────────────
            var rightLegGo = new GameObject("Leg_R");
            rightLegGo.transform.SetParent(hipsGo.transform, false);
            rightLegGo.transform.localPosition = new Vector3(0.13f, -0.10f, 0f);

            var rightLegMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightLegMesh.name = "Leg_R_Mesh";
            rightLegMesh.transform.SetParent(rightLegGo.transform, false);
            rightLegMesh.transform.localPosition = new Vector3(0f, -0.34f, 0f);
            rightLegMesh.transform.localScale = new Vector3(0.11f, 0.34f, 0.11f);
            rightLegMesh.GetComponent<MeshRenderer>().sharedMaterial = socksMat;
            Object.DestroyImmediate(rightLegMesh.GetComponent<Collider>());

            // Right Foot & Athletic Boot
            var rightFootGo = new GameObject("Foot_R");
            rightFootGo.transform.SetParent(rightLegGo.transform, false);
            rightFootGo.transform.localPosition = new Vector3(0f, -0.68f, 0.05f);

            var rightBootMesh = new GameObject("Boot_R_Mesh");
            rightBootMesh.transform.SetParent(rightFootGo.transform, false);
            rightBootMesh.transform.localPosition = new Vector3(0f, 0.02f, 0.02f);
            var rightBootFilter = rightBootMesh.AddComponent<MeshFilter>();
            rightBootFilter.sharedMesh = StylizedMeshGenerator.CreateAthleticBoot(false);
            var rightBootRenderer = rightBootMesh.AddComponent<MeshRenderer>();
            rightBootRenderer.sharedMaterial = bootMat;

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

            // ── Mecanim Humanoid Avatar Setup ─────────────────────────────────
            var animator = pawnRoot.AddComponent<Animator>();
            animator.applyRootMotion = false;
            try
            {
                var avatar = HumanoidAvatarUtility.CreateHumanoidAvatar(pawnRoot);
                if (avatar != null && avatar.isValid)
                {
                    animator.avatar = avatar;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HumanoidPawnBuilder] Avatar creation fallback: {ex.Message}");
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
