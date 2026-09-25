using UnityEngine;
using FootballLife.Unity.Core.Gameplay;

namespace FootballLife.Unity.Core.Environment
{
    /// <summary>
    /// Constructs regulation 3D football pitch, line markings, regulation goalposts,
    /// goal net with goal detection trigger, perimeter advertising boards, and floodlighting.
    /// Standard FIFA dimensions (attacking half 68m width x 55m length).
    /// </summary>
    public static class PitchBuilder
    {
        public const float kPitchWidth = 68.0f;
        public const float kPitchHalfLength = 55.0f;
        public const float kGoalLineWidth = 68.0f;
        public const float kGoalLineZ = 35.0f;

        // Regulation goal: 7.32m wide, 2.44m high, 0.12m post diameter
        public const float kGoalWidth = 7.32f;
        public const float kGoalHeight = 2.44f;
        public const float kPostRadius = 0.06f;
        public const float kNetDepth = 2.0f;

        public static GameObject BuildFullPitchEnvironment(Transform parent)
        {
            var pitchRoot = new GameObject("Pitch_Environment");
            pitchRoot.transform.SetParent(parent, false);

            var grassMat = CreateGrassMaterial();
            var lineMat = CreateLineMaterial();
            var postMat = CreatePostMaterial();
            var netMat = CreateNetMaterial();
            var boardMat = CreateAdBoardMaterial();

            // 1. Turf surface
            BuildTurfSurface(pitchRoot.transform, grassMat);

            // 2. Markings
            BuildPitchMarkings(pitchRoot.transform, lineMat);

            // 3. Goalposts and net
            BuildGoalStructure(pitchRoot.transform, postMat, netMat);

            // 4. Perimeter boards
            BuildPerimeterBoards(pitchRoot.transform, boardMat);

            // 5. Stadium Floodlights
            BuildFloodlights(pitchRoot.transform);

            return pitchRoot;
        }

        // ── Turf ──────────────────────────────────────────────────────────────
        private static void BuildTurfSurface(Transform parent, Material grassMat)
        {
            var turf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            turf.name = "Turf_Ground";
            turf.transform.SetParent(parent, false);
            // 72m wide x 1m deep x 80m long (extends beyond boundaries)
            turf.transform.position = new Vector3(0f, -0.5f, 10f);
            turf.transform.localScale = new Vector3(kPitchWidth + 6f, 1f, (kPitchHalfLength * 2f) - 10f);

            var renderer = turf.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = grassMat;

            // PhysicMaterial for grass
            var collider = turf.GetComponent<BoxCollider>();
            collider.material = new PhysicsMaterial("TurfPhysics")
            {
                bounciness = 0.55f,
                dynamicFriction = 0.45f,
                staticFriction = 0.6f,
                bounceCombine = PhysicsMaterialCombine.Average
            };
        }

        // ── Lines & Markings ──────────────────────────────────────────────────
        private static void BuildPitchMarkings(Transform parent, Material lineMat)
        {
            var linesRoot = new GameObject("Pitch_Markings");
            linesRoot.transform.SetParent(parent, false);

            float lineWidth = 0.12f; // FIFA standard 12cm line thickness
            float yPos = 0.015f;    // Just above grass to avoid z-fighting

            // Goal line (Z = +35m)
            CreateLineSegment(linesRoot.transform, lineMat, new Vector3(0f, yPos, kGoalLineZ), new Vector3(kPitchWidth, 0.005f, lineWidth));

            // Touchlines (X = -34m and +34m)
            float length = 60f;
            CreateLineSegment(linesRoot.transform, lineMat, new Vector3(-kPitchWidth * 0.5f, yPos, 5f), new Vector3(lineWidth, 0.005f, length));
            CreateLineSegment(linesRoot.transform, lineMat, new Vector3(kPitchWidth * 0.5f, yPos, 5f), new Vector3(lineWidth, 0.005f, length));

            // Penalty box: 40.32m wide x 16.5m deep (Z from 18.5m to 35m)
            float penWidth = 40.32f;
            float penDepth = 16.5f;
            float penFrontZ = kGoalLineZ - penDepth; // 18.5m
            CreateLineSegment(linesRoot.transform, lineMat, new Vector3(0f, yPos, penFrontZ), new Vector3(penWidth, 0.005f, lineWidth));
            CreateLineSegment(linesRoot.transform, lineMat, new Vector3(-penWidth * 0.5f, yPos, kGoalLineZ - (penDepth * 0.5f)), new Vector3(lineWidth, 0.005f, penDepth));
            CreateLineSegment(linesRoot.transform, lineMat, new Vector3(penWidth * 0.5f, yPos, kGoalLineZ - (penDepth * 0.5f)), new Vector3(lineWidth, 0.005f, penDepth));

            // 6-yard box: 18.32m wide x 5.5m deep (Z from 29.5m to 35m)
            float box6Width = 18.32f;
            float box6Depth = 5.5f;
            float box6FrontZ = kGoalLineZ - box6Depth;
            CreateLineSegment(linesRoot.transform, lineMat, new Vector3(0f, yPos, box6FrontZ), new Vector3(box6Width, 0.005f, lineWidth));
            CreateLineSegment(linesRoot.transform, lineMat, new Vector3(-box6Width * 0.5f, yPos, kGoalLineZ - (box6Depth * 0.5f)), new Vector3(lineWidth, 0.005f, box6Depth));
            CreateLineSegment(linesRoot.transform, lineMat, new Vector3(box6Width * 0.5f, yPos, kGoalLineZ - (box6Depth * 0.5f)), new Vector3(lineWidth, 0.005f, box6Depth));

            // Penalty Spot (X = 0, Z = 24m) — 11m from goal line
            var spot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spot.name = "Penalty_Spot";
            spot.transform.SetParent(linesRoot.transform, false);
            spot.transform.position = new Vector3(0f, yPos, kGoalLineZ - 11.0f);
            spot.transform.localScale = new Vector3(0.24f, 0.005f, 0.24f);
            spot.GetComponent<MeshRenderer>().sharedMaterial = lineMat;
            Object.DestroyImmediate(spot.GetComponent<Collider>());
        }

        private static void CreateLineSegment(Transform parent, Material mat, Vector3 pos, Vector3 scale)
        {
            var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "Line_Segment";
            line.transform.SetParent(parent, false);
            line.transform.position = pos;
            line.transform.localScale = scale;
            line.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.DestroyImmediate(line.GetComponent<Collider>());
        }

        // ── Goalposts & Net ───────────────────────────────────────────────────
        private static void BuildGoalStructure(Transform parent, Material postMat, Material netMat)
        {
            var goalRoot = new GameObject("Goal_Structure");
            goalRoot.transform.SetParent(parent, false);
            goalRoot.transform.position = new Vector3(0f, 0f, kGoalLineZ);

            float postRadius = kPostRadius;
            float postDiameter = postRadius * 2f;
            float halfWidth = kGoalWidth * 0.5f; // 3.66m

            // Metal post physic material
            var postPhysicMat = new PhysicsMaterial("MetalPostPhysics")
            {
                bounciness = 0.85f,
                dynamicFriction = 0.2f,
                staticFriction = 0.2f,
                bounceCombine = PhysicsMaterialCombine.Maximum
            };

            // Left Post
            var leftPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftPost.name = "Left_Post";
            leftPost.transform.SetParent(goalRoot.transform, false);
            leftPost.transform.localPosition = new Vector3(-halfWidth, kGoalHeight * 0.5f, 0f);
            leftPost.transform.localScale = new Vector3(postDiameter, kGoalHeight * 0.5f, postDiameter);
            leftPost.GetComponent<MeshRenderer>().sharedMaterial = postMat;
            leftPost.GetComponent<CapsuleCollider>().material = postPhysicMat;

            // Right Post
            var rightPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightPost.name = "Right_Post";
            rightPost.transform.SetParent(goalRoot.transform, false);
            rightPost.transform.localPosition = new Vector3(halfWidth, kGoalHeight * 0.5f, 0f);
            rightPost.transform.localScale = new Vector3(postDiameter, kGoalHeight * 0.5f, postDiameter);
            rightPost.GetComponent<MeshRenderer>().sharedMaterial = postMat;
            rightPost.GetComponent<CapsuleCollider>().material = postPhysicMat;

            // Crossbar
            var crossbar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crossbar.name = "Crossbar";
            crossbar.transform.SetParent(goalRoot.transform, false);
            crossbar.transform.localPosition = new Vector3(0f, kGoalHeight, 0f);
            crossbar.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            crossbar.transform.localScale = new Vector3(postDiameter, (kGoalWidth * 0.5f) + postRadius, postDiameter);
            crossbar.GetComponent<MeshRenderer>().sharedMaterial = postMat;
            crossbar.GetComponent<CapsuleCollider>().material = postPhysicMat;

            // Net Enclosure (Back wall, Top, Left, Right)
            var netBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            netBack.name = "Net_Back";
            netBack.transform.SetParent(goalRoot.transform, false);
            netBack.transform.localPosition = new Vector3(0f, kGoalHeight * 0.5f, kNetDepth);
            netBack.transform.localScale = new Vector3(kGoalWidth + postDiameter, kGoalHeight, 0.05f);
            netBack.GetComponent<MeshRenderer>().sharedMaterial = netMat;

            var netTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            netTop.name = "Net_Top";
            netTop.transform.SetParent(goalRoot.transform, false);
            netTop.transform.localPosition = new Vector3(0f, kGoalHeight, kNetDepth * 0.5f);
            netTop.transform.localScale = new Vector3(kGoalWidth + postDiameter, 0.05f, kNetDepth);
            netTop.GetComponent<MeshRenderer>().sharedMaterial = netMat;

            var netLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            netLeft.name = "Net_Left";
            netLeft.transform.SetParent(goalRoot.transform, false);
            netLeft.transform.localPosition = new Vector3(-halfWidth, kGoalHeight * 0.5f, kNetDepth * 0.5f);
            netLeft.transform.localScale = new Vector3(0.05f, kGoalHeight, kNetDepth);
            netLeft.GetComponent<MeshRenderer>().sharedMaterial = netMat;

            var netRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            netRight.name = "Net_Right";
            netRight.transform.SetParent(goalRoot.transform, false);
            netRight.transform.localPosition = new Vector3(halfWidth, kGoalHeight * 0.5f, kNetDepth * 0.5f);
            netRight.transform.localScale = new Vector3(0.05f, kGoalHeight, kNetDepth);
            netRight.GetComponent<MeshRenderer>().sharedMaterial = netMat;

            // Goal Trigger Volume
            var triggerGo = new GameObject("Goal_Trigger_Detector");
            triggerGo.transform.SetParent(goalRoot.transform, false);
            triggerGo.transform.localPosition = new Vector3(0f, kGoalHeight * 0.5f, kNetDepth * 0.5f);

            var boxCol = triggerGo.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
            boxCol.size = new Vector3(kGoalWidth - 0.2f, kGoalHeight - 0.1f, kNetDepth - 0.2f);
            triggerGo.AddComponent<GoalTrigger>();
        }

        // ── Perimeter Boards ──────────────────────────────────────────────────
        private static void BuildPerimeterBoards(Transform parent, Material boardMat)
        {
            var boardsRoot = new GameObject("Perimeter_Ad_Boards");
            boardsRoot.transform.SetParent(parent, false);

            float boardHeight = 1.0f;
            float backZ = kGoalLineZ + kNetDepth + 2.5f;

            // Back boards behind goal
            var backBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backBoard.name = "Ad_Board_Back";
            backBoard.transform.SetParent(boardsRoot.transform, false);
            backBoard.transform.position = new Vector3(0f, boardHeight * 0.5f, backZ);
            backBoard.transform.localScale = new Vector3(kPitchWidth + 4f, boardHeight, 0.2f);
            backBoard.GetComponent<MeshRenderer>().sharedMaterial = boardMat;

            // Left & right sideline boards
            float sideX = (kPitchWidth * 0.5f) + 2.5f;
            float sideLength = 65f;
            var leftBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftBoard.name = "Ad_Board_Left";
            leftBoard.transform.SetParent(boardsRoot.transform, false);
            leftBoard.transform.position = new Vector3(-sideX, boardHeight * 0.5f, 5f);
            leftBoard.transform.localScale = new Vector3(0.2f, boardHeight, sideLength);
            leftBoard.GetComponent<MeshRenderer>().sharedMaterial = boardMat;

            var rightBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightBoard.name = "Ad_Board_Right";
            rightBoard.transform.SetParent(boardsRoot.transform, false);
            rightBoard.transform.position = new Vector3(sideX, boardHeight * 0.5f, 5f);
            rightBoard.transform.localScale = new Vector3(0.2f, boardHeight, sideLength);
            rightBoard.GetComponent<MeshRenderer>().sharedMaterial = boardMat;
        }

        // ── Stadium Lighting ──────────────────────────────────────────────────
        private static void BuildFloodlights(Transform parent)
        {
            var lightsRoot = new GameObject("Stadium_Lighting");
            lightsRoot.transform.SetParent(parent, false);

            // Main Key Floodlight (Sun / High Tower)
            var keyLightGo = new GameObject("Main_Floodlight_Key");
            keyLightGo.transform.SetParent(lightsRoot.transform, false);
            keyLightGo.transform.position = new Vector3(25f, 35f, -10f);
            keyLightGo.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            var keyLight = keyLightGo.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(0.95f, 0.96f, 1.0f);
            keyLight.intensity = 1.25f;
            keyLight.shadows = LightShadows.Soft;

            // Fill Floodlight
            var fillLightGo = new GameObject("Fill_Floodlight");
            fillLightGo.transform.SetParent(lightsRoot.transform, false);
            fillLightGo.transform.position = new Vector3(-25f, 30f, 20f);
            fillLightGo.transform.rotation = Quaternion.Euler(50f, 120f, 0f);

            var fillLight = fillLightGo.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.85f, 0.90f, 1.0f);
            fillLight.intensity = 0.45f;
            fillLight.shadows = LightShadows.None;
        }

        // ── Materials Helper ──────────────────────────────────────────────────
        private static Material CreateGrassMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader)
            {
                name = "Mat_Turf_Grass",
                color = new Color(0.13f, 0.45f, 0.18f) // Rich pitch green
            };
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);
            return mat;
        }

        private static Material CreateLineMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader)
            {
                name = "Mat_Pitch_Lines",
                color = new Color(0.95f, 0.95f, 0.95f) // Clean chalk white
            };
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.2f);
            return mat;
        }

        private static Material CreatePostMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader)
            {
                name = "Mat_Goal_Posts",
                color = new Color(0.98f, 0.98f, 0.98f) // High gloss post white
            };
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.85f);
            return mat;
        }

        private static Material CreateNetMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader)
            {
                name = "Mat_Goal_Net",
                color = new Color(0.85f, 0.88f, 0.92f, 0.65f) // Semi-transparent mesh
            };
            return mat;
        }

        private static Material CreateAdBoardMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader)
            {
                name = "Mat_Perimeter_Board",
                color = new Color(0.08f, 0.12f, 0.22f) // Sleek navy perimeter
            };
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.5f);
            return mat;
        }
    }
}
