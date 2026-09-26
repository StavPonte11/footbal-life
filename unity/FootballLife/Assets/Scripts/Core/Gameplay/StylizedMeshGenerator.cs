#nullable enable
using System;
using System.Collections.Generic;
using FootballLife.Domain;
using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Procedural geometry generator producing mobile-optimized, stylized low-poly
    /// meshes for footballer pawns, athletic kits, hair styles, and goalkeeper gear.
    /// Triangles budget: ~1,800 to 2,400 tris per complete humanoid pawn.
    /// </summary>
    public static class StylizedMeshGenerator
    {
        // ── Athletic Torso (Jersey) ──────────────────────────────────────────
        public static Mesh CreateAthleticTorso(bool isGoalkeeper)
        {
            var mesh = new Mesh { name = isGoalkeeper ? "Mesh_GK_Torso" : "Mesh_Outfield_Torso" };

            // 12-vertex tapered athletic torso (broader shoulders, narrower waist)
            // Dimensions: Height ~ 0.44m, Shoulder width ~ 0.46m, Waist width ~ 0.36m, Depth ~ 0.22m
            float topW = 0.23f;
            float botW = 0.17f;
            float topD = 0.12f;
            float botD = 0.09f;
            float h = 0.44f;
            float chestBump = isGoalkeeper ? 0.04f : 0.02f; // Padded chest for GK

            var vertices = new Vector3[]
            {
                // Bottom ring (waist)
                new Vector3(-botW, 0f, -botD), // 0: bot-left-back
                new Vector3( botW, 0f, -botD), // 1: bot-right-back
                new Vector3( botW, 0f,  botD), // 2: bot-right-front
                new Vector3(-botW, 0f,  botD), // 3: bot-left-front

                // Mid ring (chest line)
                new Vector3(-topW * 0.95f, h * 0.65f, -topD),
                new Vector3( topW * 0.95f, h * 0.65f, -topD),
                new Vector3( topW * 0.95f, h * 0.65f,  topD + chestBump),
                new Vector3(-topW * 0.95f, h * 0.65f,  topD + chestBump),

                // Top ring (shoulders/collar)
                new Vector3(-topW, h, -topD * 0.8f), // 8: top-left-back
                new Vector3( topW, h, -topD * 0.8f), // 9: top-right-back
                new Vector3( topW, h,  topD * 0.8f), // 10: top-right-front
                new Vector3(-topW, h,  topD * 0.8f), // 11: top-left-front
            };

            var triangles = new List<int>();

            // Quads helper
            void AddQuad(int a, int b, int c, int d)
            {
                triangles.Add(a); triangles.Add(b); triangles.Add(c);
                triangles.Add(a); triangles.Add(c); triangles.Add(d);
            }

            // Lower torso
            AddQuad(0, 1, 5, 4); // Back
            AddQuad(1, 2, 6, 5); // Right
            AddQuad(2, 3, 7, 6); // Front
            AddQuad(3, 0, 4, 7); // Left

            // Upper torso
            AddQuad(4, 5, 9, 8);   // Back
            AddQuad(5, 6, 10, 9);  // Right
            AddQuad(6, 7, 11, 10); // Front
            AddQuad(7, 4, 8, 11);  // Left

            // Top cap (shoulders)
            AddQuad(8, 9, 10, 11);
            // Bottom cap (waist)
            AddQuad(3, 2, 1, 0);

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Athletic Shorts (Pelvis) ─────────────────────────────────────────
        public static Mesh CreateAthleticShorts()
        {
            var mesh = new Mesh { name = "Mesh_Athletic_Shorts" };

            // Boxy contoured athletic shorts
            float w = 0.20f;
            float d = 0.12f;
            float h = 0.22f;

            var vertices = new Vector3[]
            {
                // Top (waistband)
                new Vector3(-w * 0.9f, h, -d * 0.9f),
                new Vector3( w * 0.9f, h, -d * 0.9f),
                new Vector3( w * 0.9f, h,  d * 0.9f),
                new Vector3(-w * 0.9f, h,  d * 0.9f),

                // Bottom (flared leg openings)
                new Vector3(-w * 1.05f, 0f, -d * 1.05f),
                new Vector3( w * 1.05f, 0f, -d * 1.05f),
                new Vector3( w * 1.05f, 0f,  d * 1.05f),
                new Vector3(-w * 1.05f, 0f,  d * 1.05f),
            };

            var triangles = new List<int>
            {
                // Back
                0, 1, 5, 0, 5, 4,
                // Right
                1, 2, 6, 1, 6, 5,
                // Front
                2, 3, 7, 2, 7, 6,
                // Left
                3, 0, 4, 3, 4, 7,
                // Top cap
                0, 3, 2, 0, 2, 1,
                // Bottom cap
                7, 4, 5, 7, 5, 6
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Contoured Stylized Head ──────────────────────────────────────────
        public static Mesh CreateContouredHead()
        {
            var mesh = new Mesh { name = "Mesh_Contoured_Head" };

            // Low-poly sculpted head: defined jaw, chin, cheeks, brow, and cranium
            float w = 0.11f;
            float d = 0.12f;
            float chinY = 0f;
            float jawY = 0.08f;
            float eyeY = 0.16f;
            float browY = 0.21f;
            float topY = 0.28f;

            var vertices = new Vector3[]
            {
                // 0: Chin
                new Vector3(0f, chinY, d * 0.5f),

                // 1-4: Jaw & lower head
                new Vector3(-w * 0.65f, jawY,  d * 0.4f),
                new Vector3( w * 0.65f, jawY,  d * 0.4f),
                new Vector3( w * 0.85f, jawY, -d * 0.5f),
                new Vector3(-w * 0.85f, jawY, -d * 0.5f),

                // 5-8: Mid head / Cheek / Eye line
                new Vector3(-w * 0.95f, eyeY,  d * 0.55f),
                new Vector3( w * 0.95f, eyeY,  d * 0.55f),
                new Vector3( w * 0.95f, eyeY, -d * 0.65f),
                new Vector3(-w * 0.95f, eyeY, -d * 0.65f),

                // 9-12: Brow line & temple
                new Vector3(-w * 0.90f, browY,  d * 0.55f),
                new Vector3( w * 0.90f, browY,  d * 0.55f),
                new Vector3( w * 0.90f, browY, -d * 0.65f),
                new Vector3(-w * 0.90f, browY, -d * 0.65f),

                // 13: Cranium top center
                new Vector3(0f, topY, -d * 0.1f)
            };

            var triangles = new List<int>
            {
                // Chin to jaw
                0, 1, 2,
                // Jaw sides
                1, 4, 3, 1, 3, 2,
                // Lower to mid front
                1, 5, 6, 1, 6, 2,
                // Mid sides
                2, 6, 7, 2, 7, 3,
                4, 8, 5, 4, 5, 1,
                // Mid back
                3, 7, 8, 3, 8, 4,
                // Eye to brow front
                5, 9, 10, 5, 10, 6,
                // Brow to cranium top
                9, 13, 10,
                10, 13, 11,
                11, 13, 12,
                12, 13, 9,
                // Back temple to top
                6, 10, 11, 6, 11, 7,
                8, 12, 9, 8, 9, 5,
                7, 11, 12, 7, 12, 8
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Modular Hair Meshes (6 Styles) ───────────────────────────────────
        public static Mesh CreateModularHair(HairStyleType style)
        {
            var mesh = new Mesh { name = $"Mesh_Hair_{style}" };

            switch (style)
            {
                case HairStyleType.BuzzFade:
                    // Sleek low-profile skull cap
                    return CreateEllipsoidMesh(0.12f, 0.08f, 0.13f, 8, 6);

                case HairStyleType.ShortCrop:
                    // Athletic cropped top with front texture ridge
                    return CreateTaperedHairCap(0.125f, 0.11f, 0.135f, topLift: 0.04f);

                case HairStyleType.TexturedAfro:
                    // Volumetric rounded dome
                    return CreateEllipsoidMesh(0.155f, 0.14f, 0.155f, 10, 8);

                case HairStyleType.SlickBack:
                    // Swept back volume with rear taper
                    return CreateSweptBackMesh(0.13f, 0.11f, 0.15f);

                case HairStyleType.LongTied:
                    // Pulled back hair with topknot / bun
                    return CreateTopknotMesh(0.13f, 0.10f, 0.14f);

                case HairStyleType.Undercut:
                default:
                    // Disconnected high volume top with parted sides
                    return CreateUndercutMesh(0.135f, 0.12f, 0.14f);
            }
        }

        // ── Goalkeeper Padded Gloves ─────────────────────────────────────────
        public static Mesh CreateGoalkeeperGlove(bool isLeft)
        {
            var mesh = new Mesh { name = isLeft ? "Mesh_GK_Glove_L" : "Mesh_GK_Glove_R" };

            // Padded wide palms with distinct thumb block and extended wristband
            float sign = isLeft ? -1f : 1f;
            float w = 0.08f;
            float h = 0.14f;
            float d = 0.05f;

            var vertices = new Vector3[]
            {
                // Wrist band
                new Vector3(-w * 0.6f, -0.06f, -d * 0.6f),
                new Vector3( w * 0.6f, -0.06f, -d * 0.6f),
                new Vector3( w * 0.6f, -0.06f,  d * 0.6f),
                new Vector3(-w * 0.6f, -0.06f,  d * 0.6f),

                // Palm base
                new Vector3(-w, 0f, -d),
                new Vector3( w, 0f, -d),
                new Vector3( w, 0f,  d),
                new Vector3(-w, 0f,  d),

                // Palm top / Finger pads
                new Vector3(-w * 0.9f, h, -d * 0.8f),
                new Vector3( w * 0.9f, h, -d * 0.8f),
                new Vector3( w * 0.9f, h,  d * 0.8f),
                new Vector3(-w * 0.9f, h,  d * 0.8f),

                // Thumb flare
                new Vector3(sign * (w + 0.035f), h * 0.4f, d * 0.5f)
            };

            var triangles = new List<int>
            {
                // Wrist to palm
                0, 1, 5, 0, 5, 4,
                1, 2, 6, 1, 6, 5,
                2, 3, 7, 2, 7, 6,
                3, 0, 4, 3, 4, 7,

                // Palm to fingers
                4, 5, 9, 4, 9, 8,
                5, 6, 10, 5, 10, 9,
                6, 7, 11, 6, 11, 10,
                7, 4, 8, 7, 8, 11,

                // Fingertips cap
                8, 9, 10, 8, 10, 11,

                // Thumb triangle
                isLeft ? 7 : 6, 12, isLeft ? 11 : 10
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Athletic Football Boot ───────────────────────────────────────────
        public static Mesh CreateAthleticBoot(bool isLeft)
        {
            var mesh = new Mesh { name = isLeft ? "Mesh_Boot_L" : "Mesh_Boot_R" };

            // Streamlined low-poly football boot with toe curve and heel collar
            float w = 0.055f;
            float h = 0.065f;
            float heelZ = -0.09f;
            float toeZ = 0.13f;

            var vertices = new Vector3[]
            {
                // Sole
                new Vector3(-w, 0f, heelZ),
                new Vector3( w, 0f, heelZ),
                new Vector3( w * 0.8f, 0.01f, toeZ),
                new Vector3(-w * 0.8f, 0.01f, toeZ),

                // Upper
                new Vector3(-w * 0.85f, h, heelZ * 0.7f),
                new Vector3( w * 0.85f, h, heelZ * 0.7f),
                new Vector3( w * 0.65f, h * 0.55f, toeZ * 0.7f),
                new Vector3(-w * 0.65f, h * 0.55f, toeZ * 0.7f),

                // Toe tip
                new Vector3(0f, 0.025f, toeZ + 0.02f)
            };

            var triangles = new List<int>
            {
                // Sole bottom
                0, 3, 2, 0, 2, 1,
                // Sides
                0, 1, 5, 0, 5, 4,
                1, 2, 6, 1, 6, 5,
                3, 0, 4, 3, 4, 7,
                // Vamp / laces
                4, 5, 6, 4, 6, 7,
                // Toe cap
                2, 3, 8, 6, 2, 8, 3, 7, 8, 7, 6, 8
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // ── Procedural Jersey Squad Number Badge ──────────────────────────────
        public static Texture2D CreateSquadNumberTexture(int number, Color fontColor, Color bgColor)
        {
            const int width = 128;
            const int height = 128;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = $"Tex_SquadNumber_{number}",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = bgColor;
            }

            // Draw clean blocky digital/athletic numerals
            string strNum = Math.Clamp(number, 1, 99).ToString();
            DrawSimpleDigits(pixels, width, height, strNum, fontColor);

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static void DrawSimpleDigits(Color[] pixels, int width, int height, string digits, Color color)
        {
            int digitWidth = digits.Length == 1 ? 40 : 28;
            int digitHeight = 70;
            int startY = (height - digitHeight) / 2;
            int spacing = 8;
            int totalW = digits.Length * digitWidth + (digits.Length - 1) * spacing;
            int startX = (width - totalW) / 2;

            for (int i = 0; i < digits.Length; i++)
            {
                int xOffset = startX + i * (digitWidth + spacing);
                DrawSingleDigit(pixels, width, height, digits[i], xOffset, startY, digitWidth, digitHeight, color);
            }
        }

        private static void DrawSingleDigit(
            Color[] pixels, int width, int height, char digit,
            int x0, int y0, int w, int h, Color color)
        {
            // 7-segment athletic numeral renderer
            int t = Math.Max(4, w / 5); // Thickness
            bool top = digit != '1' && digit != '4';
            bool mid = digit != '0' && digit != '1' && digit != '7';
            bool bot = digit != '1' && digit != '4' && digit != '7';
            bool tl = digit != '1' && digit != '2' && digit != '3' && digit != '7';
            bool tr = digit != '5' && digit != '6';
            bool bl = digit == '0' || digit == '2' || digit == '6' || digit == '8';
            bool br = digit != '2';

            void FillRect(int rx, int ry, int rw, int rh)
            {
                for (int y = ry; y < ry + rh && y < height; y++)
                {
                    for (int x = rx; x < rx + rw && x < width; x++)
                    {
                        if (x >= 0 && y >= 0) pixels[y * width + x] = color;
                    }
                }
            }

            int midY = y0 + h / 2 - t / 2;

            if (top) FillRect(x0, y0 + h - t, w, t);
            if (mid) FillRect(x0, midY, w, t);
            if (bot) FillRect(x0, y0, w, t);

            if (tl) FillRect(x0, midY, t, h / 2);
            if (tr) FillRect(x0 + w - t, midY, t, h / 2);
            if (bl) FillRect(x0, y0, t, h / 2);
            if (br) FillRect(x0 + w - t, y0, t, h / 2);
        }

        // ── Helper Geometry Builders ──────────────────────────────────────────
        private static Mesh CreateEllipsoidMesh(float rx, float ry, float rz, int stacks, int slices)
        {
            var mesh = new Mesh { name = "Mesh_Ellipsoid" };
            var verts = new List<Vector3>();
            var tris = new List<int>();

            for (int i = 0; i <= stacks; i++)
            {
                float v = (float)i / stacks;
                float phi = v * Mathf.PI;

                for (int j = 0; j <= slices; j++)
                {
                    float u = (float)j / slices;
                    float theta = u * (Mathf.PI * 2);

                    float x = Mathf.Sin(phi) * Mathf.Cos(theta) * rx;
                    float y = Mathf.Cos(phi) * ry;
                    float z = Mathf.Sin(phi) * Mathf.Sin(theta) * rz;

                    verts.Add(new Vector3(x, y, z));
                }
            }

            for (int i = 0; i < stacks; i++)
            {
                for (int j = 0; j < slices; j++)
                {
                    int first = (i * (slices + 1)) + j;
                    int second = first + slices + 1;

                    tris.Add(first);
                    tris.Add(second);
                    tris.Add(first + 1);

                    tris.Add(second);
                    tris.Add(second + 1);
                    tris.Add(first + 1);
                }
            }

            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateTaperedHairCap(float rx, float ry, float rz, float topLift)
        {
            var mesh = CreateEllipsoidMesh(rx, ry, rz, 8, 8);
            var verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                if (verts[i].y > 0)
                {
                    verts[i] = new Vector3(verts[i].x, verts[i].y + topLift, verts[i].z);
                }
            }
            mesh.vertices = verts;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateSweptBackMesh(float rx, float ry, float rz)
        {
            var mesh = CreateEllipsoidMesh(rx, ry, rz, 8, 8);
            var verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                if (verts[i].y > 0)
                {
                    // Shift top volume backwards
                    verts[i] = new Vector3(verts[i].x, verts[i].y * 1.15f, verts[i].z - (verts[i].y * 0.35f));
                }
            }
            mesh.vertices = verts;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateTopknotMesh(float rx, float ry, float rz)
        {
            var mesh = CreateEllipsoidMesh(rx, ry, rz, 8, 8);
            var verts = new List<Vector3>(mesh.vertices);
            var tris = new List<int>(mesh.triangles);

            // Add bun on top-rear
            int bunStart = verts.Count;
            Vector3 bunCenter = new Vector3(0f, ry * 1.3f, -rz * 0.4f);
            float bunR = 0.045f;

            var bunMesh = CreateEllipsoidMesh(bunR, bunR, bunR, 6, 6);
            foreach (var v in bunMesh.vertices)
            {
                verts.Add(v + bunCenter);
            }
            foreach (var t in bunMesh.triangles)
            {
                tris.Add(t + bunStart);
            }

            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateUndercutMesh(float rx, float ry, float rz)
        {
            var mesh = CreateEllipsoidMesh(rx, ry, rz, 8, 8);
            var verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                if (verts[i].y < ry * 0.2f)
                {
                    // Shave lower sides
                    verts[i] = new Vector3(verts[i].x * 0.75f, verts[i].y, verts[i].z * 0.75f);
                }
                else
                {
                    // Part top volume to the right
                    verts[i] = new Vector3(verts[i].x + 0.02f, verts[i].y * 1.25f, verts[i].z);
                }
            }
            mesh.vertices = verts;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
