#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Utility for creating Unity Humanoid Avatars programmatically from rigged pawn hierarchies,
    /// enabling Mecanim Animator Controller & Locomotion BlendTree evaluation.
    /// </summary>
    public static class HumanoidAvatarUtility
    {
        public static Avatar CreateHumanoidAvatar(GameObject rootGo)
        {
            var humanBones = new List<HumanBone>();
            var skeletonBones = new List<SkeletonBone>();

            // Map all transforms in hierarchy to skeleton
            AddSkeletonBonesRecursive(rootGo.transform, skeletonBones);

            // Map standard Humanoid anatomical bones
            void MapBone(string humanName, string bonePath)
            {
                var t = rootGo.transform.Find(bonePath);
                if (t != null)
                {
                    var hb = new HumanBone
                    {
                        humanName = humanName,
                        boneName = t.name,
                        limit = new HumanLimit { useDefaultValues = true }
                    };
                    humanBones.Add(hb);
                }
            }

            MapBone("Hips", "Hips");
            MapBone("Spine", "Hips/Torso");
            MapBone("Chest", "Hips/Torso");
            MapBone("Head", "Hips/Torso/Head");
            MapBone("LeftUpperArm", "Hips/Torso/Arm_L");
            MapBone("RightUpperArm", "Hips/Torso/Arm_R");
            MapBone("LeftUpperLeg", "Hips/Leg_L");
            MapBone("RightUpperLeg", "Hips/Leg_R");
            MapBone("LeftFoot", "Hips/Leg_L/Foot_L");
            MapBone("RightFoot", "Hips/Leg_R/Foot_R");

            var humanDesc = new HumanDescription
            {
                human = humanBones.ToArray(),
                skeleton = skeletonBones.ToArray(),
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0.1f,
                hasTranslationDoF = false
            };

            var avatar = AvatarBuilder.BuildHumanAvatar(rootGo, humanDesc);
            avatar.name = $"{rootGo.name}_HumanoidAvatar";
            return avatar;
        }

        private static void AddSkeletonBonesRecursive(Transform t, List<SkeletonBone> list)
        {
            list.Add(new SkeletonBone
            {
                name = t.name,
                position = t.localPosition,
                rotation = t.localRotation,
                scale = t.localScale
            });

            for (int i = 0; i < t.childCount; i++)
            {
                AddSkeletonBonesRecursive(t.GetChild(i), list);
            }
        }
    }
}
