#nullable enable
using UnityEngine;
using FootballLife.Domain;
using FootballLife.Unity.Core.Gameplay;

namespace FootballLife.Unity.Core.Environment
{
    /// <summary>
    /// Procedurally constructs the player's 3D apartment environment, including
    /// walls, flooring, ceiling, windows, interactive zones (Bed, Gym, Phone, Door),
    /// and dynamic aesthetic styling based on the active LifestyleTier.
    /// </summary>
    public static class ApartmentBuilder
    {
        public const float kRoomWidth = 14.0f;
        public const float kRoomDepth = 12.0f;
        public const float kRoomHeight = 3.6f;

        public static GameObject BuildApartment(Transform parent, LifestyleTier tier = LifestyleTier.Comfortable)
        {
            var root = new GameObject("Apartment_Environment");
            root.transform.SetParent(parent, false);

            var theme = GetTierTheme(tier);

            // 1. Structure (Floor, Walls, Ceiling, Window)
            BuildRoomStructure(root.transform, theme);

            // 2. Interior Lighting
            BuildInteriorLighting(root.transform, theme);

            // 3. Bed Zone (Rest & Recovery)
            BuildBedZone(root.transform, theme);

            // 4. Gym Zone (Fitness & Workout)
            BuildGymZone(root.transform, theme);

            // 5. Lounge & Phone Zone (Smartphone, Media)
            BuildLoungeZone(root.transform, theme);

            // 6. Front Door Zone (Exit to Club)
            BuildDoorZone(root.transform, theme);

            // 7. Decorative Accents (Rugs, Wall Art, Plants)
            BuildDecorations(root.transform, theme);

            return root;
        }

        public static GameObject RebuildForTier(Transform parent, LifestyleTier tier)
        {
            var existing = parent.Find("Apartment_Environment");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
            return BuildApartment(parent, tier);
        }

        // ── Room Structure ────────────────────────────────────────────────────
        private static void BuildRoomStructure(Transform parent, ApartmentTheme theme)
        {
            var structRoot = new GameObject("Room_Structure");
            structRoot.transform.SetParent(parent, false);

            // Floor
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(structRoot.transform, false);
            floor.transform.position = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = new Vector3(kRoomWidth, 0.2f, kRoomDepth);
            var floorMat = CreateLitMaterial("Mat_Floor", theme.FloorColor, theme.FloorSmoothness, theme.FloorMetallic);
            floor.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

            // Ceiling
            var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.SetParent(structRoot.transform, false);
            ceiling.transform.position = new Vector3(0f, kRoomHeight + 0.1f, 0f);
            ceiling.transform.localScale = new Vector3(kRoomWidth, 0.2f, kRoomDepth);
            var ceilingMat = CreateLitMaterial("Mat_Ceiling", new Color(0.92f, 0.92f, 0.94f), 0.05f, 0f);
            ceiling.GetComponent<MeshRenderer>().sharedMaterial = ceilingMat;

            var wallMat = CreateLitMaterial("Mat_Walls", theme.WallColor, 0.15f, 0f);
            var accentMat = CreateLitMaterial("Mat_AccentWall", theme.AccentWallColor, 0.25f, 0.05f);

            // Back Wall (Feature Wall)
            var backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.name = "Back_Wall";
            backWall.transform.SetParent(structRoot.transform, false);
            backWall.transform.position = new Vector3(0f, kRoomHeight * 0.5f, kRoomDepth * 0.5f);
            backWall.transform.localScale = new Vector3(kRoomWidth, kRoomHeight, 0.3f);
            backWall.GetComponent<MeshRenderer>().sharedMaterial = accentMat;

            // Left Wall
            var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "Left_Wall";
            leftWall.transform.SetParent(structRoot.transform, false);
            leftWall.transform.position = new Vector3(-kRoomWidth * 0.5f, kRoomHeight * 0.5f, 0f);
            leftWall.transform.localScale = new Vector3(0.3f, kRoomHeight, kRoomDepth);
            leftWall.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Right Wall with Panoramic Skyline Window
            BuildWindowWall(structRoot.transform, theme, wallMat);
        }

        private static void BuildWindowWall(Transform parent, ApartmentTheme theme, Material wallMat)
        {
            var wallRoot = new GameObject("Right_Window_Wall");
            wallRoot.transform.SetParent(parent, false);

            float rightX = kRoomWidth * 0.5f;

            // Wall Lower Sill
            var lowerSill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lowerSill.transform.SetParent(wallRoot.transform, false);
            lowerSill.transform.position = new Vector3(rightX, 0.45f, 0f);
            lowerSill.transform.localScale = new Vector3(0.3f, 0.9f, kRoomDepth);
            lowerSill.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Wall Upper Header
            var upperHeader = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upperHeader.transform.SetParent(wallRoot.transform, false);
            upperHeader.transform.position = new Vector3(rightX, kRoomHeight - 0.25f, 0f);
            upperHeader.transform.localScale = new Vector3(0.3f, 0.5f, kRoomDepth);
            upperHeader.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // Glass Pane
            var glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "Window_Glass";
            glass.transform.SetParent(wallRoot.transform, false);
            glass.transform.position = new Vector3(rightX, (kRoomHeight * 0.5f) + 0.1f, 0f);
            glass.transform.localScale = new Vector3(0.05f, kRoomHeight - 1.4f, kRoomDepth - 1.5f);
            var glassMat = CreateLitMaterial("Mat_Glass", new Color(0.6f, 0.85f, 0.95f, 0.4f), 0.9f, 0.1f);
            glass.GetComponent<MeshRenderer>().sharedMaterial = glassMat;

            // City Skyline Backdrop (Outdoor view)
            var backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backdrop.name = "Skyline_Backdrop";
            backdrop.transform.SetParent(wallRoot.transform, false);
            backdrop.transform.position = new Vector3(rightX + 6.0f, kRoomHeight * 0.6f, 0f);
            backdrop.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            backdrop.transform.localScale = new Vector3(18f, 10f, 1f);
            var skyMat = CreateLitMaterial("Mat_Skyline", theme.SkylineColor, 0f, 0f);
            backdrop.GetComponent<MeshRenderer>().sharedMaterial = skyMat;
        }

        // ── Interior Lighting ─────────────────────────────────────────────────
        private static void BuildInteriorLighting(Transform parent, ApartmentTheme theme)
        {
            var lightRoot = new GameObject("Apartment_Lighting");
            lightRoot.transform.SetParent(parent, false);

            // Warm Key Light (Indoor Ambient Sun / Skylight)
            var keyLightGo = new GameObject("Key_Light");
            keyLightGo.transform.SetParent(lightRoot.transform, false);
            keyLightGo.transform.position = new Vector3(4f, 5f, 2f);
            keyLightGo.transform.rotation = Quaternion.Euler(50f, -60f, 0f);
            var keyLight = keyLightGo.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = theme.LightColor;
            keyLight.intensity = theme.LightIntensity;
            keyLight.shadows = LightShadows.Soft;

            // Soft Bedside Warm Lamp Light
            var bedLightGo = new GameObject("Bed_Lamp_PointLight");
            bedLightGo.transform.SetParent(lightRoot.transform, false);
            bedLightGo.transform.position = new Vector3(-5.5f, 1.2f, 3.8f);
            var bedLight = bedLightGo.AddComponent<Light>();
            bedLight.type = LightType.Point;
            bedLight.color = new Color(1.0f, 0.78f, 0.55f);
            bedLight.range = 6.0f;
            bedLight.intensity = 1.2f;

            // Gym Ceiling Spot
            var gymLightGo = new GameObject("Gym_Ceiling_SpotLight");
            gymLightGo.transform.SetParent(lightRoot.transform, false);
            gymLightGo.transform.position = new Vector3(4.5f, 3.4f, 2.5f);
            gymLightGo.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var gymLight = gymLightGo.AddComponent<Light>();
            gymLight.type = LightType.Spot;
            gymLight.spotAngle = 65f;
            gymLight.color = new Color(0.9f, 0.95f, 1.0f);
            gymLight.range = 5.0f;
            gymLight.intensity = 2.0f;
        }

        // ── Interactive Zones ─────────────────────────────────────────────────

        // 1. Bed Zone (Sleep / Rest / Recovery)
        private static void BuildBedZone(Transform parent, ApartmentTheme theme)
        {
            var zoneGo = new GameObject("Zone_Bed");
            zoneGo.transform.SetParent(parent, false);
            zoneGo.transform.position = new Vector3(-4.5f, 0f, 3.2f);

            var bedCol = zoneGo.AddComponent<BoxCollider>();
            bedCol.center = new Vector3(0f, 0.6f, 0f);
            bedCol.size = new Vector3(2.4f, 1.2f, 2.8f);

            var zoneComp = zoneGo.AddComponent<HomeInteractionZone>();
            zoneComp.Initialize(HomeZoneType.Bed, "Master Bed", "🛌 Rest & Sleep Recovery", zoneGo.transform);

            // Bed Frame
            var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Bed_Frame";
            frame.transform.SetParent(zoneGo.transform, false);
            frame.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            frame.transform.localScale = new Vector3(2.1f, 0.4f, 2.4f);
            frame.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Wood_Bed", theme.WoodColor, 0.3f, 0.05f);

            // Mattress
            var mattress = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mattress.name = "Mattress";
            mattress.transform.SetParent(zoneGo.transform, false);
            mattress.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            mattress.transform.localScale = new Vector3(1.95f, 0.35f, 2.25f);
            mattress.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Linen_Bed", theme.LinenColor, 0.05f, 0f);

            // Headboard
            var headboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            headboard.name = "Headboard";
            headboard.transform.SetParent(zoneGo.transform, false);
            headboard.transform.localPosition = new Vector3(0f, 0.85f, 1.15f);
            headboard.transform.localScale = new Vector3(2.2f, 1.1f, 0.15f);
            headboard.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Headboard", theme.WoodColor * 0.9f, 0.25f, 0f);

            // Pillows
            for (int i = -1; i <= 1; i += 2)
            {
                var pillow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillow.name = $"Pillow_{(i == -1 ? "L" : "R")}";
                pillow.transform.SetParent(zoneGo.transform, false);
                pillow.transform.localPosition = new Vector3(i * 0.55f, 0.72f, 0.8f);
                pillow.transform.localScale = new Vector3(0.65f, 0.15f, 0.45f);
                pillow.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Pillow", Color.white, 0.05f, 0f);
            }

            // Nightstand with table lamp
            var nightstand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nightstand.name = "Nightstand";
            nightstand.transform.SetParent(zoneGo.transform, false);
            nightstand.transform.localPosition = new Vector3(-1.35f, 0.3f, 0.9f);
            nightstand.transform.localScale = new Vector3(0.55f, 0.6f, 0.55f);
            nightstand.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Nightstand", theme.WoodColor, 0.3f, 0f);
        }

        // 2. Gym Zone (Fitness / Home Workout)
        private static void BuildGymZone(Transform parent, ApartmentTheme theme)
        {
            var zoneGo = new GameObject("Zone_Gym");
            zoneGo.transform.SetParent(parent, false);
            zoneGo.transform.position = new Vector3(4.5f, 0f, 2.6f);

            var gymCol = zoneGo.AddComponent<BoxCollider>();
            gymCol.center = new Vector3(0f, 0.7f, 0f);
            gymCol.size = new Vector3(2.8f, 1.4f, 2.8f);

            var zoneComp = zoneGo.AddComponent<HomeInteractionZone>();
            zoneComp.Initialize(HomeZoneType.Gym, "Home Gym", "🏋️ Workout & Conditioning", zoneGo.transform);

            // Heavy duty rubber floor mat
            var gymMat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gymMat.name = "Gym_Floor_Mat";
            gymMat.transform.SetParent(zoneGo.transform, false);
            gymMat.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            gymMat.transform.localScale = new Vector3(2.6f, 0.03f, 2.6f);
            gymMat.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Gym_Rubber", new Color(0.12f, 0.13f, 0.15f), 0.1f, 0f);

            // Workout Weight Bench
            var benchPad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            benchPad.name = "Weight_Bench";
            benchPad.transform.SetParent(zoneGo.transform, false);
            benchPad.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            benchPad.transform.localScale = new Vector3(0.5f, 0.12f, 1.4f);
            benchPad.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Bench_Leather", new Color(0.08f, 0.08f, 0.1f), 0.4f, 0f);

            // Bench Legs
            var legMat = CreateLitMaterial("Mat_Steel_Black", new Color(0.18f, 0.18f, 0.2f), 0.6f, 0.7f);
            var benchLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            benchLeg.transform.SetParent(zoneGo.transform, false);
            benchLeg.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            benchLeg.transform.localScale = new Vector3(0.45f, 0.4f, 1.1f);
            benchLeg.GetComponent<MeshRenderer>().sharedMaterial = legMat;

            // Barbell on Rack
            var barbell = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barbell.name = "Barbell";
            barbell.transform.SetParent(zoneGo.transform, false);
            barbell.transform.localPosition = new Vector3(0f, 1.05f, 0.5f);
            barbell.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            barbell.transform.localScale = new Vector3(0.04f, 0.85f, 0.04f);
            barbell.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Chrome_Steel", new Color(0.85f, 0.85f, 0.88f), 0.85f, 0.9f);

            // Weight Plates
            for (int i = -1; i <= 1; i += 2)
            {
                var plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                plate.name = $"Weight_Plate_{(i == -1 ? "L" : "R")}";
                plate.transform.SetParent(zoneGo.transform, false);
                plate.transform.localPosition = new Vector3(i * 0.7f, 1.05f, 0.5f);
                plate.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                plate.transform.localScale = new Vector3(0.35f, 0.06f, 0.35f);
                plate.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Bumper_Plate", theme.AccentColor, 0.2f, 0.1f);
            }

            // Dumbbell Rack
            var dumbMat = CreateLitMaterial("Mat_Dumbbell", new Color(0.15f, 0.15f, 0.18f), 0.5f, 0.4f);
            for (int i = 0; i < 2; i++)
            {
                var dumbbell = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                dumbbell.name = $"Dumbbell_{i}";
                dumbbell.transform.SetParent(zoneGo.transform, false);
                dumbbell.transform.localPosition = new Vector3(0.9f, 0.1f, -0.4f + (i * 0.45f));
                dumbbell.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
                dumbbell.transform.localScale = new Vector3(0.12f, 0.18f, 0.12f);
                dumbbell.GetComponent<MeshRenderer>().sharedMaterial = dumbMat;
            }
        }

        // 3. Lounge & Phone Zone (Device, Social, Media)
        private static void BuildLoungeZone(Transform parent, ApartmentTheme theme)
        {
            var zoneGo = new GameObject("Zone_Phone");
            zoneGo.transform.SetParent(parent, false);
            zoneGo.transform.position = new Vector3(0f, 0f, -0.8f);

            var phoneCol = zoneGo.AddComponent<BoxCollider>();
            phoneCol.center = new Vector3(0f, 0.5f, 0f);
            phoneCol.size = new Vector3(3.2f, 1.0f, 2.6f);

            var zoneComp = zoneGo.AddComponent<HomeInteractionZone>();
            zoneComp.Initialize(HomeZoneType.Phone, "Lounge & Phone", "📱 Open Phone & Media", zoneGo.transform);

            // Modern Coffee Table
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Coffee_Table";
            table.transform.SetParent(zoneGo.transform, false);
            table.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            table.transform.localScale = new Vector3(1.6f, 0.4f, 1.0f);
            table.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Table", theme.TableColor, 0.6f, 0.1f);

            // Smartphone / Tablet Prop on Table
            var phoneProp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            phoneProp.name = "Smartphone_Device";
            phoneProp.transform.SetParent(zoneGo.transform, false);
            phoneProp.transform.localPosition = new Vector3(0.15f, 0.46f, 0.05f);
            phoneProp.transform.localScale = new Vector3(0.14f, 0.015f, 0.26f);
            phoneProp.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Phone_Body", new Color(0.1f, 0.1f, 0.12f), 0.9f, 0.8f);

            // Glowing Screen
            var phoneScreen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            phoneScreen.name = "Smartphone_Screen";
            phoneScreen.transform.SetParent(phoneProp.transform, false);
            phoneScreen.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            phoneScreen.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            phoneScreen.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            var screenMat = CreateLitMaterial("Mat_Phone_Screen", new Color(0.2f, 0.6f, 1.0f), 0.9f, 0f);
            screenMat.EnableKeyword("_EMISSION");
            screenMat.SetColor("_EmissionColor", new Color(0.1f, 0.35f, 0.8f) * 1.5f);
            phoneScreen.GetComponent<MeshRenderer>().sharedMaterial = screenMat;

            // Sofa / Couch behind table
            var couch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            couch.name = "Sofa_Seat";
            couch.transform.SetParent(zoneGo.transform, false);
            couch.transform.localPosition = new Vector3(0f, 0.35f, -1.25f);
            couch.transform.localScale = new Vector3(2.6f, 0.45f, 0.9f);
            couch.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Sofa", theme.SofaColor, 0.05f, 0f);

            var couchBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            couchBack.name = "Sofa_Backrest";
            couchBack.transform.SetParent(zoneGo.transform, false);
            couchBack.transform.localPosition = new Vector3(0f, 0.7f, -1.6f);
            couchBack.transform.localScale = new Vector3(2.6f, 0.65f, 0.3f);
            couchBack.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Sofa_Back", theme.SofaColor, 0.05f, 0f);
        }

        // 4. Front Door Zone (Exit to Club / Training / Match)
        private static void BuildDoorZone(Transform parent, ApartmentTheme theme)
        {
            var zoneGo = new GameObject("Zone_Door");
            zoneGo.transform.SetParent(parent, false);
            zoneGo.transform.position = new Vector3(-4.8f, 0f, -4.5f);

            var doorCol = zoneGo.AddComponent<BoxCollider>();
            doorCol.center = new Vector3(0f, 1.2f, 0f);
            doorCol.size = new Vector3(1.8f, 2.4f, 1.4f);

            var zoneComp = zoneGo.AddComponent<HomeInteractionZone>();
            zoneComp.Initialize(HomeZoneType.Door, "Front Door", "🚪 Depart to Club / Career Hub", zoneGo.transform);

            // Door Frame
            var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Door_Frame";
            frame.transform.SetParent(zoneGo.transform, false);
            frame.transform.localPosition = new Vector3(0f, 1.25f, -0.65f);
            frame.transform.localScale = new Vector3(1.4f, 2.45f, 0.15f);
            frame.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Door_Frame", new Color(0.2f, 0.2f, 0.22f), 0.3f, 0.1f);

            // Door Panel
            var doorPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorPanel.name = "Door_Panel";
            doorPanel.transform.SetParent(zoneGo.transform, false);
            doorPanel.transform.localPosition = new Vector3(0f, 1.22f, -0.62f);
            doorPanel.transform.localScale = new Vector3(1.15f, 2.35f, 0.06f);
            doorPanel.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Door_Wood", theme.WoodColor * 0.8f, 0.4f, 0.05f);

            // Door Handle
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Door_Handle";
            handle.transform.SetParent(zoneGo.transform, false);
            handle.transform.localPosition = new Vector3(0.45f, 1.15f, -0.55f);
            handle.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            handle.transform.localScale = new Vector3(0.025f, 0.08f, 0.025f);
            handle.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Door_Brass", new Color(0.9f, 0.75f, 0.35f), 0.8f, 0.8f);

            // Welcome Doormat
            var doormat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doormat.name = "Doormat";
            doormat.transform.SetParent(zoneGo.transform, false);
            doormat.transform.localPosition = new Vector3(0f, 0.015f, -0.1f);
            doormat.transform.localScale = new Vector3(1.1f, 0.03f, 0.7f);
            doormat.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Doormat", new Color(0.35f, 0.28f, 0.22f), 0.05f, 0f);
        }

        // ── Decorations ───────────────────────────────────────────────────────
        private static void BuildDecorations(Transform parent, ApartmentTheme theme)
        {
            var decorRoot = new GameObject("Room_Decorations");
            decorRoot.transform.SetParent(parent, false);

            // Plush Lounge Rug
            var rug = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rug.name = "Lounge_Rug";
            rug.transform.SetParent(decorRoot.transform, false);
            rug.transform.localPosition = new Vector3(0f, 0.01f, -0.8f);
            rug.transform.localScale = new Vector3(3.2f, 0.02f, 2.2f);
            rug.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Rug", theme.RugColor, 0.05f, 0f);

            // Wall Art on Back Feature Wall
            var wallArt = GameObject.CreatePrimitive(PrimitiveType.Quad);
            wallArt.name = "Wall_Art";
            wallArt.transform.SetParent(decorRoot.transform, false);
            wallArt.transform.localPosition = new Vector3(0f, 2.2f, (kRoomDepth * 0.5f) - 0.14f);
            wallArt.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            wallArt.transform.localScale = new Vector3(2.4f, 1.4f, 1f);
            wallArt.GetComponent<MeshRenderer>().sharedMaterial = CreateLitMaterial("Mat_Wall_Art", theme.ArtColor, 0.4f, 0f);
        }

        // ── Theme Data ────────────────────────────────────────────────────────
        private sealed class ApartmentTheme
        {
            public Color FloorColor { get; set; }
            public float FloorSmoothness { get; set; }
            public float FloorMetallic { get; set; }
            public Color WallColor { get; set; }
            public Color AccentWallColor { get; set; }
            public Color WoodColor { get; set; }
            public Color LinenColor { get; set; }
            public Color SofaColor { get; set; }
            public Color TableColor { get; set; }
            public Color RugColor { get; set; }
            public Color ArtColor { get; set; }
            public Color AccentColor { get; set; }
            public Color SkylineColor { get; set; }
            public Color LightColor { get; set; }
            public float LightIntensity { get; set; }
        }

        private static ApartmentTheme GetTierTheme(LifestyleTier tier)
        {
            return tier switch
            {
                LifestyleTier.Modest => new ApartmentTheme
                {
                    FloorColor = new Color(0.65f, 0.62f, 0.58f), // Cool linoleum
                    FloorSmoothness = 0.2f,
                    FloorMetallic = 0f,
                    WallColor = new Color(0.88f, 0.88f, 0.86f),
                    AccentWallColor = new Color(0.55f, 0.60f, 0.65f),
                    WoodColor = new Color(0.55f, 0.42f, 0.30f),
                    LinenColor = new Color(0.78f, 0.82f, 0.85f),
                    SofaColor = new Color(0.35f, 0.38f, 0.42f),
                    TableColor = new Color(0.48f, 0.48f, 0.50f),
                    RugColor = new Color(0.45f, 0.48f, 0.52f),
                    ArtColor = new Color(0.25f, 0.45f, 0.65f),
                    AccentColor = new Color(0.2f, 0.5f, 0.8f),
                    SkylineColor = new Color(0.35f, 0.45f, 0.55f),
                    LightColor = new Color(1.0f, 0.95f, 0.90f),
                    LightIntensity = 1.0f
                },
                LifestyleTier.Comfortable => new ApartmentTheme
                {
                    FloorColor = new Color(0.62f, 0.45f, 0.28f), // Rich warm oak
                    FloorSmoothness = 0.45f,
                    FloorMetallic = 0.05f,
                    WallColor = new Color(0.92f, 0.91f, 0.89f),
                    AccentWallColor = new Color(0.22f, 0.30f, 0.38f), // Deep navy accent
                    WoodColor = new Color(0.48f, 0.34f, 0.22f),
                    LinenColor = new Color(0.92f, 0.90f, 0.88f),
                    SofaColor = new Color(0.25f, 0.27f, 0.30f),
                    TableColor = new Color(0.18f, 0.18f, 0.20f),
                    RugColor = new Color(0.70f, 0.68f, 0.65f),
                    ArtColor = new Color(0.85f, 0.55f, 0.25f),
                    AccentColor = new Color(0.95f, 0.55f, 0.15f),
                    SkylineColor = new Color(0.15f, 0.22f, 0.35f),
                    LightColor = new Color(1.0f, 0.94f, 0.86f),
                    LightIntensity = 1.15f
                },
                LifestyleTier.Luxurious => new ApartmentTheme
                {
                    FloorColor = new Color(0.82f, 0.80f, 0.78f), // Italian marble
                    FloorSmoothness = 0.85f,
                    FloorMetallic = 0.15f,
                    WallColor = new Color(0.95f, 0.95f, 0.96f),
                    AccentWallColor = new Color(0.15f, 0.18f, 0.24f), // Charcoal slate
                    WoodColor = new Color(0.22f, 0.16f, 0.12f), // Dark walnut
                    LinenColor = new Color(0.96f, 0.96f, 0.98f),
                    SofaColor = new Color(0.15f, 0.16f, 0.18f),
                    TableColor = new Color(0.90f, 0.90f, 0.92f), // White marble table
                    RugColor = new Color(0.80f, 0.78f, 0.74f),
                    ArtColor = new Color(0.95f, 0.75f, 0.30f), // Gold abstract art
                    AccentColor = new Color(0.95f, 0.72f, 0.20f),
                    SkylineColor = new Color(0.08f, 0.12f, 0.22f), // Glamorous night skyline
                    LightColor = new Color(1.0f, 0.92f, 0.82f),
                    LightIntensity = 1.25f
                },
                LifestyleTier.Extravagant => new ApartmentTheme
                {
                    FloorColor = new Color(0.15f, 0.15f, 0.17f), // Polished black granite
                    FloorSmoothness = 0.92f,
                    FloorMetallic = 0.25f,
                    WallColor = new Color(0.94f, 0.92f, 0.90f),
                    AccentWallColor = new Color(0.18f, 0.14f, 0.22f), // Royal purple slate
                    WoodColor = new Color(0.18f, 0.12f, 0.08f), // Ebony
                    LinenColor = new Color(0.98f, 0.98f, 1.0f),
                    SofaColor = new Color(0.12f, 0.12f, 0.14f),
                    TableColor = new Color(0.15f, 0.15f, 0.18f),
                    RugColor = new Color(0.65f, 0.58f, 0.50f),
                    ArtColor = new Color(0.98f, 0.80f, 0.25f),
                    AccentColor = new Color(0.98f, 0.75f, 0.15f),
                    SkylineColor = new Color(0.05f, 0.08f, 0.15f),
                    LightColor = new Color(1.0f, 0.90f, 0.80f),
                    LightIntensity = 1.35f
                },
                LifestyleTier.Superstar => new ApartmentTheme
                {
                    FloorColor = new Color(0.92f, 0.90f, 0.86f), // Gold-veined Calacatta marble
                    FloorSmoothness = 0.95f,
                    FloorMetallic = 0.3f,
                    WallColor = new Color(0.97f, 0.96f, 0.95f),
                    AccentWallColor = new Color(0.10f, 0.12f, 0.16f), // Obsidian
                    WoodColor = new Color(0.14f, 0.09f, 0.06f),
                    LinenColor = new Color(1.0f, 1.0f, 1.0f),
                    SofaColor = new Color(0.85f, 0.82f, 0.78f), // Premium cream leather
                    TableColor = new Color(0.95f, 0.93f, 0.90f),
                    RugColor = new Color(0.88f, 0.84f, 0.78f),
                    ArtColor = new Color(1.0f, 0.85f, 0.35f), // Pure gold accent art
                    AccentColor = new Color(1.0f, 0.80f, 0.20f),
                    SkylineColor = new Color(0.04f, 0.06f, 0.12f),
                    LightColor = new Color(1.0f, 0.92f, 0.82f),
                    LightIntensity = 1.45f
                },
                _ => GetTierTheme(LifestyleTier.Comfortable)
            };
        }

        private static Material CreateLitMaterial(string name, Color color, float smoothness, float metallic)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader)
            {
                name = name,
                color = color
            };
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            return mat;
        }
    }
}
