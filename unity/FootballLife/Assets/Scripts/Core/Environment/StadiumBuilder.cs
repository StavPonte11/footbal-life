using System;
using System.Collections.Generic;
using UnityEngine;
using FootballLife.Domain;

namespace FootballLife.Unity.Core.Environment
{
    /// <summary>
    /// Procedurally constructs 3D stadium grandstands, tiered seating, dugouts, technical areas,
    /// roofs, and architectural lighting scaled by club reputation tier (Grassroots, MidTier, Elite).
    /// </summary>
    public static class StadiumBuilder
    {
        public static GameObject BuildStadium(
            Transform parent,
            StadiumReputationTier tier = StadiumReputationTier.MidTier,
            Color? homePrimaryColor = null,
            Color? homeSecondaryColor = null)
        {
            var stadiumRoot = new GameObject("Stadium_Architecture");
            stadiumRoot.transform.SetParent(parent, false);

            var config = StadiumAtmosphereUtility.GetTierConfig(tier);

            var primaryColor = homePrimaryColor ?? new Color(0.12f, 0.35f, 0.75f);   // Club primary blue
            var secondaryColor = homeSecondaryColor ?? new Color(0.92f, 0.85f, 0.2f); // Accent gold
            var concreteColor = new Color(0.68f, 0.70f, 0.72f);
            var roofColor = new Color(0.22f, 0.25f, 0.28f);
            var glassColor = new Color(0.35f, 0.60f, 0.75f, 0.55f);

            var concreteMat = CreateMaterial("Mat_Stadium_Concrete", concreteColor, 0.15f);
            var seatMat = CreateMaterial("Mat_Stadium_Seats", primaryColor, 0.5f);
            var accentSeatMat = CreateMaterial("Mat_Stadium_Seats_Accent", secondaryColor, 0.5f);
            var roofMat = CreateMaterial("Mat_Stadium_Roof", roofColor, 0.25f);
            var glassMat = CreateTransparentMaterial("Mat_Dugout_Glass", glassColor);
            var metalMat = CreateMaterial("Mat_Stadium_Metal", new Color(0.3f, 0.32f, 0.35f), 0.7f);

            // 1. Build Grandstands (North, South, East, West)
            BuildGrandstands(stadiumRoot.transform, config, concreteMat, seatMat, accentSeatMat, roofMat, metalMat);

            // 2. Build Dugouts & Technical Areas (along West touchline)
            BuildDugouts(stadiumRoot.transform, config, concreteMat, seatMat, glassMat, metalMat);

            // 3. Build Scaled Floodlights
            BuildTierFloodlights(stadiumRoot.transform, config, metalMat);

            return stadiumRoot;
        }

        // ── Grandstands ────────────────────────────────────────────────────────
        private static void BuildGrandstands(
            Transform parent,
            StadiumTierConfig config,
            Material concreteMat,
            Material seatMat,
            Material accentMat,
            Material roofMat,
            Material metalMat)
        {
            var standsRoot = new GameObject("Grandstands");
            standsRoot.transform.SetParent(parent, false);

            float pitchHalfW = PitchBuilder.kPitchWidth * 0.5f; // 34m
            float pitchHalfL = PitchBuilder.kPitchHalfLength;   // 55m
            float goalZ = PitchBuilder.kGoalLineZ;              // 35m
            float margin = 4.5f;

            // North Stand (Behind Goal, Z > goalZ)
            Vector3 northPos = new Vector3(0f, 0f, goalZ + margin);
            BuildStandBlock(standsRoot.transform, "North_Stand_GoalEnd", northPos, Quaternion.identity, 76f, config, concreteMat, seatMat, accentMat, roofMat, metalMat);

            // South Stand (Opposite End, Z < -12m)
            Vector3 southPos = new Vector3(0f, 0f, -14f - margin);
            BuildStandBlock(standsRoot.transform, "South_Stand_AwayEnd", southPos, Quaternion.Euler(0f, 180f, 0f), 76f, config, concreteMat, seatMat, accentMat, roofMat, metalMat);

            // East Stand (Right Touchline, X > pitchHalfW)
            Vector3 eastPos = new Vector3(pitchHalfW + margin, 0f, 10f);
            BuildStandBlock(standsRoot.transform, "East_Stand_Sideline", eastPos, Quaternion.Euler(0f, 90f, 0f), 65f, config, concreteMat, seatMat, accentMat, roofMat, metalMat);

            // West Stand (Main Stand / Dugouts, X < -pitchHalfW)
            Vector3 westPos = new Vector3(-pitchHalfW - margin, 0f, 10f);
            BuildStandBlock(standsRoot.transform, "West_Stand_MainStand", westPos, Quaternion.Euler(0f, -90f, 0f), 65f, config, concreteMat, seatMat, accentMat, roofMat, metalMat);
        }

        private static void BuildStandBlock(
            Transform parent,
            string name,
            Vector3 origin,
            Quaternion rotation,
            float width,
            StadiumTierConfig config,
            Material concreteMat,
            Material seatMat,
            Material accentMat,
            Material roofMat,
            Material metalMat)
        {
            var standGo = new GameObject(name);
            standGo.transform.SetParent(parent, false);
            standGo.transform.position = origin;
            standGo.transform.rotation = rotation;

            int steps = config.RowSteps;
            float stepDepth = config.StandDepthMeters / steps;
            float stepHeight = config.StandHeightMeters / steps;

            // 1. Procedural Terraced Steps Mesh
            var terraceMesh = GenerateTerraceMesh(width, steps, stepDepth, stepHeight);
            var terraceGo = new GameObject("Terrace_Deck");
            terraceGo.transform.SetParent(standGo.transform, false);
            var mf = terraceGo.AddComponent<MeshFilter>();
            mf.sharedMesh = terraceMesh;
            var mr = terraceGo.AddComponent<MeshRenderer>();
            mr.sharedMaterial = concreteMat;

            // 2. Seating Blocks Mesh
            var seatsMesh = GenerateSeatsMesh(width, steps, stepDepth, stepHeight);
            var seatsGo = new GameObject("Seating_Rows");
            seatsGo.transform.SetParent(standGo.transform, false);
            var seatMf = seatsGo.AddComponent<MeshFilter>();
            seatMf.sharedMesh = seatsMesh;
            var seatMr = seatsGo.AddComponent<MeshRenderer>();
            seatMr.sharedMaterial = seatMat;

            // 3. Cantilever Roof if configured
            if (config.HasCantileverRoof)
            {
                BuildRoofCanopy(standGo.transform, width, config, roofMat, metalMat);
            }

            // 4. Executive Suites if Elite tier
            if (config.HasExecutiveBoxes)
            {
                BuildExecutiveSuites(standGo.transform, width, config, concreteMat, metalMat);
            }
        }

        private static Mesh GenerateTerraceMesh(float width, int steps, float stepDepth, float stepHeight)
        {
            var mesh = new Mesh { name = "Mesh_Terrace_Deck" };
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            float halfW = width * 0.5f;

            for (int s = 0; s < steps; s++)
            {
                float zFront = s * stepDepth;
                float zBack = (s + 1) * stepDepth;
                float yBottom = s * stepHeight;
                float yTop = (s + 1) * stepHeight;

                // Vertical Riser Face
                int rStart = verts.Count;
                verts.Add(new Vector3(-halfW, yBottom, zFront));
                verts.Add(new Vector3(halfW, yBottom, zFront));
                verts.Add(new Vector3(halfW, yTop, zFront));
                verts.Add(new Vector3(-halfW, yTop, zFront));

                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(0f, 1f));

                tris.Add(rStart); tris.Add(rStart + 2); tris.Add(rStart + 1);
                tris.Add(rStart); tris.Add(rStart + 3); tris.Add(rStart + 2);

                // Horizontal Tread Face
                int tStart = verts.Count;
                verts.Add(new Vector3(-halfW, yTop, zFront));
                verts.Add(new Vector3(halfW, yTop, zFront));
                verts.Add(new Vector3(halfW, yTop, zBack));
                verts.Add(new Vector3(-halfW, yTop, zBack));

                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(0f, 1f));

                tris.Add(tStart); tris.Add(tStart + 2); tris.Add(tStart + 1);
                tris.Add(tStart); tris.Add(tStart + 3); tris.Add(tStart + 2);
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh GenerateSeatsMesh(float width, int steps, float stepDepth, float stepHeight)
        {
            var mesh = new Mesh { name = "Mesh_Seating_Blocks" };
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            float halfW = width * 0.5f;
            float seatHeight = 0.22f;
            float seatDepth = stepDepth * 0.65f;

            for (int s = 0; s < steps; s++)
            {
                float zCenter = (s * stepDepth) + (stepDepth * 0.45f);
                float yBase = (s + 1) * stepHeight;

                // Row block spanning width with aisle gaps
                int seatSections = 6;
                float sectionWidth = (width - 4f) / seatSections;

                for (int sec = 0; sec < seatSections; sec++)
                {
                    float xMin = -halfW + 1f + (sec * sectionWidth) + 0.15f;
                    float xMax = xMin + sectionWidth - 0.30f;

                    // Bench seat top face
                    int startIdx = verts.Count;
                    verts.Add(new Vector3(xMin, yBase + seatHeight, zCenter - (seatDepth * 0.5f)));
                    verts.Add(new Vector3(xMax, yBase + seatHeight, zCenter - (seatDepth * 0.5f)));
                    verts.Add(new Vector3(xMax, yBase + seatHeight, zCenter + (seatDepth * 0.5f)));
                    verts.Add(new Vector3(xMin, yBase + seatHeight, zCenter + (seatDepth * 0.5f)));

                    uvs.Add(new Vector2(0f, 0f));
                    uvs.Add(new Vector2(1f, 0f));
                    uvs.Add(new Vector2(1f, 1f));
                    uvs.Add(new Vector2(0f, 1f));

                    tris.Add(startIdx); tris.Add(startIdx + 2); tris.Add(startIdx + 1);
                    tris.Add(startIdx); tris.Add(startIdx + 3); tris.Add(startIdx + 2);

                    // Bench seat front face
                    int fIdx = verts.Count;
                    verts.Add(new Vector3(xMin, yBase, zCenter - (seatDepth * 0.5f)));
                    verts.Add(new Vector3(xMax, yBase, zCenter - (seatDepth * 0.5f)));
                    verts.Add(new Vector3(xMax, yBase + seatHeight, zCenter - (seatDepth * 0.5f)));
                    verts.Add(new Vector3(xMin, yBase + seatHeight, zCenter - (seatDepth * 0.5f)));

                    uvs.Add(new Vector2(0f, 0f));
                    uvs.Add(new Vector2(1f, 0f));
                    uvs.Add(new Vector2(1f, 1f));
                    uvs.Add(new Vector2(0f, 1f));

                    tris.Add(fIdx); tris.Add(fIdx + 2); tris.Add(fIdx + 1);
                    tris.Add(fIdx); tris.Add(fIdx + 3); tris.Add(fIdx + 2);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void BuildRoofCanopy(Transform parent, float width, StadiumTierConfig config, Material roofMat, Material metalMat)
        {
            var roofGo = new GameObject("Cantilever_Roof");
            roofGo.transform.SetParent(parent, false);

            float roofY = config.StandHeightMeters + 2.5f;
            float roofDepth = config.StandDepthMeters + 3.0f;
            float roofZ = (config.StandDepthMeters * 0.5f) - 1.5f;

            // Canopy Slab
            var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "Roof_Slab";
            slab.transform.SetParent(roofGo.transform, false);
            slab.transform.localPosition = new Vector3(0f, roofY, roofZ);
            slab.transform.localRotation = Quaternion.Euler(6f, 0f, 0f); // slight forward slope
            slab.transform.localScale = new Vector3(width + 2f, 0.45f, roofDepth);
            slab.GetComponent<MeshRenderer>().sharedMaterial = roofMat;
            UnityEngine.Object.DestroyImmediate(slab.GetComponent<Collider>());

            // Steel Support Trusses / Columns at back
            int pillarCount = 5;
            for (int p = 0; p < pillarCount; p++)
            {
                float t = p / (float)(pillarCount - 1);
                float px = Mathf.Lerp(-width * 0.45f, width * 0.45f, t);

                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = $"Roof_Pillar_{p}";
                pillar.transform.SetParent(roofGo.transform, false);
                pillar.transform.localPosition = new Vector3(px, roofY * 0.5f, config.StandDepthMeters);
                pillar.transform.localScale = new Vector3(0.4f, roofY * 0.5f, 0.4f);
                pillar.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
                UnityEngine.Object.DestroyImmediate(pillar.GetComponent<Collider>());
            }
        }

        private static void BuildExecutiveSuites(Transform parent, float width, StadiumTierConfig config, Material concreteMat, Material metalMat)
        {
            var suiteRoot = new GameObject("Executive_Suites_Ribbon");
            suiteRoot.transform.SetParent(parent, false);

            float midY = config.StandHeightMeters * 0.48f;
            float midZ = config.StandDepthMeters * 0.5f;

            var suiteBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            suiteBar.name = "VIP_Glass_Lounge";
            suiteBar.transform.SetParent(suiteRoot.transform, false);
            suiteBar.transform.localPosition = new Vector3(0f, midY, midZ);
            suiteBar.transform.localScale = new Vector3(width - 2f, 1.8f, 1.2f);
            suiteBar.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
            UnityEngine.Object.DestroyImmediate(suiteBar.GetComponent<Collider>());
        }

        // ── Dugouts & Technical Area ───────────────────────────────────────────
        private static void BuildDugouts(
            Transform parent,
            StadiumTierConfig config,
            Material concreteMat,
            Material seatMat,
            Material glassMat,
            Material metalMat)
        {
            var dugoutsRoot = new GameObject("Dugouts_And_Technical_Areas");
            dugoutsRoot.transform.SetParent(parent, false);

            float sidelineX = -(PitchBuilder.kPitchWidth * 0.5f) - 3.2f;

            // Home Dugout (Z = -5m)
            BuildSingleDugout(dugoutsRoot.transform, "Dugout_Home", new Vector3(sidelineX, 0f, -5f), config, seatMat, glassMat, metalMat, true);

            // Away Dugout (Z = +5m)
            BuildSingleDugout(dugoutsRoot.transform, "Dugout_Away", new Vector3(sidelineX, 0f, 5f), config, seatMat, glassMat, metalMat, false);

            // Technical Areas (White line boxes on pitch edge)
            var lineMat = CreateMaterial("Mat_TechArea_Lines", new Color(0.95f, 0.95f, 0.95f), 0.2f);
            BuildTechnicalAreaBox(dugoutsRoot.transform, "TechArea_Home", new Vector3(-(PitchBuilder.kPitchWidth * 0.5f) - 1.2f, 0.02f, -5f), lineMat);
            BuildTechnicalAreaBox(dugoutsRoot.transform, "TechArea_Away", new Vector3(-(PitchBuilder.kPitchWidth * 0.5f) - 1.2f, 0.02f, 5f), lineMat);
        }

        private static void BuildSingleDugout(
            Transform parent,
            string name,
            Vector3 pos,
            StadiumTierConfig config,
            Material seatMat,
            Material glassMat,
            Material metalMat,
            bool isHome)
        {
            var dugoutGo = new GameObject(name);
            dugoutGo.transform.SetParent(parent, false);
            dugoutGo.transform.position = pos;
            dugoutGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f); // Face towards pitch

            float width = 5.2f;
            float depth = 1.8f;
            float height = 2.1f;

            // Floor base
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Dugout_Floor";
            floor.transform.SetParent(dugoutGo.transform, false);
            floor.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            floor.transform.localScale = new Vector3(width, 0.1f, depth);
            floor.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
            UnityEngine.Object.DestroyImmediate(floor.GetComponent<Collider>());

            // Canopy Shelter (Back wall + Roof)
            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Canopy_Back";
            back.transform.SetParent(dugoutGo.transform, false);
            back.transform.localPosition = new Vector3(0f, height * 0.5f, -depth * 0.5f);
            back.transform.localScale = new Vector3(width, height, 0.06f);
            back.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
            UnityEngine.Object.DestroyImmediate(back.GetComponent<Collider>());

            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Canopy_Roof";
            roof.transform.SetParent(dugoutGo.transform, false);
            roof.transform.localPosition = new Vector3(0f, height, 0f);
            roof.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
            roof.transform.localScale = new Vector3(width + 0.2f, 0.06f, depth + 0.3f);
            roof.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
            UnityEngine.Object.DestroyImmediate(roof.GetComponent<Collider>());

            // Bench Seats (6 seats for substitutes & staff)
            int seats = (config.Tier == StadiumReputationTier.Elite) ? 8 : 6;
            for (int s = 0; s < seats; s++)
            {
                float sx = Mathf.Lerp(-width * 0.42f, width * 0.42f, s / (float)(seats - 1));
                var chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chair.name = $"Seat_{s}";
                chair.transform.SetParent(dugoutGo.transform, false);
                chair.transform.localPosition = new Vector3(sx, 0.35f, -0.2f);
                chair.transform.localScale = new Vector3(0.42f, 0.45f, 0.42f);
                chair.GetComponent<MeshRenderer>().sharedMaterial = seatMat;
                UnityEngine.Object.DestroyImmediate(chair.GetComponent<Collider>());
            }
        }

        private static void BuildTechnicalAreaBox(Transform parent, string name, Vector3 pos, Material lineMat)
        {
            var boxGo = new GameObject(name);
            boxGo.transform.SetParent(parent, false);
            boxGo.transform.position = pos;

            float boxW = 2.4f;
            float boxL = 4.5f;
            float lineThick = 0.08f;

            // 4 boundary lines
            CreateLine(boxGo.transform, new Vector3(0f, 0f, boxL * 0.5f), new Vector3(boxW, 0.005f, lineThick), lineMat);
            CreateLine(boxGo.transform, new Vector3(0f, 0f, -boxL * 0.5f), new Vector3(boxW, 0.005f, lineThick), lineMat);
            CreateLine(boxGo.transform, new Vector3(boxW * 0.5f, 0f, 0f), new Vector3(lineThick, 0.005f, boxL), lineMat);
            CreateLine(boxGo.transform, new Vector3(-boxW * 0.5f, 0f, 0f), new Vector3(lineThick, 0.005f, boxL), lineMat);
        }

        private static void CreateLine(Transform parent, Vector3 localPos, Vector3 scale, Material mat)
        {
            var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.transform.SetParent(parent, false);
            seg.transform.localPosition = localPos;
            seg.transform.localScale = scale;
            seg.GetComponent<MeshRenderer>().sharedMaterial = mat;
            UnityEngine.Object.DestroyImmediate(seg.GetComponent<Collider>());
        }

        // ── Floodlights ────────────────────────────────────────────────────────
        private static void BuildTierFloodlights(Transform parent, StadiumTierConfig config, Material metalMat)
        {
            var lightRoot = new GameObject("Tier_Lighting_Pylons");
            lightRoot.transform.SetParent(parent, false);

            float halfW = (PitchBuilder.kPitchWidth * 0.5f) + config.StandDepthMeters + 3f;
            float goalZ = PitchBuilder.kGoalLineZ + config.StandDepthMeters + 2f;
            float awayZ = -14f - config.StandDepthMeters - 2f;

            float towerH = config.StandHeightMeters + 8f;

            // 4 Corner Towers
            Vector3[] cornerPositions = new[]
            {
                new Vector3(-halfW, 0f, goalZ),  // North-West
                new Vector3(halfW, 0f, goalZ),   // North-East
                new Vector3(-halfW, 0f, awayZ),  // South-West
                new Vector3(halfW, 0f, awayZ)    // South-East
            };

            for (int i = 0; i < cornerPositions.Length; i++)
            {
                var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tower.name = $"Pylon_Mast_{i}";
                tower.transform.SetParent(lightRoot.transform, false);
                tower.transform.position = new Vector3(cornerPositions[i].x, towerH * 0.5f, cornerPositions[i].z);
                tower.transform.localScale = new Vector3(0.7f, towerH * 0.5f, 0.7f);
                tower.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
                UnityEngine.Object.DestroyImmediate(tower.GetComponent<Collider>());

                // Floodlight lamp head bank
                var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
                head.name = $"Lamp_Head_{i}";
                head.transform.SetParent(tower.transform, false);
                head.transform.localPosition = new Vector3(0f, 1.0f, 0f);
                head.transform.localScale = new Vector3(3.5f, 1.2f, 1.2f);
                head.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
                UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());
            }
        }

        // ── Material Helpers ───────────────────────────────────────────────────
        private static Material CreateMaterial(string name, Color color, float smoothness)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader)
            {
                name = name,
                color = color
            };
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        private static Material CreateTransparentMaterial(string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader)
            {
                name = name,
                color = color
            };
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // Transparent in URP
            return mat;
        }
    }
}
