#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FootballLife.Unity.Core.Gameplay
{
    public enum ActionClipType
    {
        PowerShot,
        FinesseCurl,
        Header,
        SlidingTackle,
        GKDiveLeft,
        GKDiveRight,
        GKJumpSave
    }

    public enum CelebrationClipType
    {
        KneeSlide,
        FistPump,
        CrowdWave
    }

    /// <summary>
    /// Factory creating mobile-optimized, clip-driven football animations:
    /// Locomotion (Idle, Jog, Sprint), Action clips (Power Shot, Finesse Curl, Header, Slide, GK Save),
    /// and Celebration clips (Knee Slide, Fist Pump, Crowd Wave).
    /// </summary>
    public static class MecanimAnimationFactory
    {
        private static readonly Dictionary<string, AnimationClip> _clipCache = new();

        public static AnimationClip GetOrCreateIdleClip()
        {
            const string key = "Clip_Locomotion_Idle";
            if (_clipCache.TryGetValue(key, out var clip) && clip != null) return clip;

            clip = new AnimationClip { name = key, wrapMode = WrapMode.Loop };
            float dur = 2.0f;

            // Torso breathing
            var torsoRotX = CreateSinCurve(dur, 0f, 2.5f, 2);
            SetCurveRot(clip, "Hips/Torso", torsoRotX, null, null);

            // Subtle arm idle sway
            var armRotZ = CreateSinCurve(dur, -8f, -12f, 2);
            SetCurveRot(clip, "Hips/Torso/Arm_L", null, null, armRotZ);
            var armRRotZ = CreateSinCurve(dur, 8f, 12f, 2);
            SetCurveRot(clip, "Hips/Torso/Arm_R", null, null, armRRotZ);

            _clipCache[key] = clip;
            return clip;
        }

        public static AnimationClip GetOrCreateJogClip()
        {
            const string key = "Clip_Locomotion_Jog";
            if (_clipCache.TryGetValue(key, out var clip) && clip != null) return clip;

            clip = new AnimationClip { name = key, wrapMode = WrapMode.Loop };
            float dur = 0.8f;

            // Alternating leg swing
            var legL = CreateSinCurve(dur, -30f, 30f, 1);
            var legR = CreateSinCurve(dur, 30f, -30f, 1);
            SetCurveRot(clip, "Hips/Leg_L", legL, null, null);
            SetCurveRot(clip, "Hips/Leg_R", legR, null, null);

            // Counter arm swing
            var armL = CreateSinCurve(dur, 25f, -25f, 1);
            var armR = CreateSinCurve(dur, -25f, 25f, 1);
            SetCurveRot(clip, "Hips/Torso/Arm_L", armL, null, null);
            SetCurveRot(clip, "Hips/Torso/Arm_R", armR, null, null);

            // Hip vertical bounce
            var hipY = CreateBounceCurve(dur, 0.85f, 0.90f, 2);
            SetCurvePos(clip, "Hips", null, hipY, null);

            _clipCache[key] = clip;
            return clip;
        }

        public static AnimationClip GetOrCreateSprintClip()
        {
            const string key = "Clip_Locomotion_Sprint";
            if (_clipCache.TryGetValue(key, out var clip) && clip != null) return clip;

            clip = new AnimationClip { name = key, wrapMode = WrapMode.Loop };
            float dur = 0.55f;

            // Exaggerated leg swing
            var legL = CreateSinCurve(dur, -55f, 55f, 1);
            var legR = CreateSinCurve(dur, 55f, -55f, 1);
            SetCurveRot(clip, "Hips/Leg_L", legL, null, null);
            SetCurveRot(clip, "Hips/Leg_R", legR, null, null);

            // Aggressive arm drive
            var armL = CreateSinCurve(dur, 50f, -50f, 1);
            var armR = CreateSinCurve(dur, -50f, 50f, 1);
            SetCurveRot(clip, "Hips/Torso/Arm_L", armL, null, null);
            SetCurveRot(clip, "Hips/Torso/Arm_R", armR, null, null);

            // Forward torso lean
            var torsoLean = CreateFlatCurve(dur, 14f);
            SetCurveRot(clip, "Hips/Torso", torsoLean, null, null);

            // High hip bounce
            var hipY = CreateBounceCurve(dur, 0.84f, 0.92f, 2);
            SetCurvePos(clip, "Hips", null, hipY, null);

            _clipCache[key] = clip;
            return clip;
        }

        public static AnimationClip GetOrCreateActionClip(ActionClipType action)
        {
            string key = $"Clip_Action_{action}";
            if (_clipCache.TryGetValue(key, out var clip) && clip != null) return clip;

            clip = new AnimationClip { name = key, wrapMode = WrapMode.Once };

            switch (action)
            {
                case ActionClipType.PowerShot:
                    BuildPowerShotClip(clip);
                    break;
                case ActionClipType.FinesseCurl:
                    BuildFinesseCurlClip(clip);
                    break;
                case ActionClipType.Header:
                    BuildHeaderClip(clip);
                    break;
                case ActionClipType.SlidingTackle:
                    BuildSlidingTackleClip(clip);
                    break;
                case ActionClipType.GKDiveLeft:
                    BuildGKDiveClip(clip, true);
                    break;
                case ActionClipType.GKDiveRight:
                    BuildGKDiveClip(clip, false);
                    break;
                case ActionClipType.GKJumpSave:
                    BuildGKJumpSaveClip(clip);
                    break;
            }

            _clipCache[key] = clip;
            return clip;
        }

        public static AnimationClip GetOrCreateCelebrationClip(CelebrationClipType celebration)
        {
            string key = $"Clip_Celebration_{celebration}";
            if (_clipCache.TryGetValue(key, out var clip) && clip != null) return clip;

            clip = new AnimationClip { name = key, wrapMode = WrapMode.Once };

            switch (celebration)
            {
                case CelebrationClipType.KneeSlide:
                    BuildKneeSlideClip(clip);
                    break;
                case CelebrationClipType.FistPump:
                    BuildFistPumpClip(clip);
                    break;
                case CelebrationClipType.CrowdWave:
                    BuildCrowdWaveClip(clip);
                    break;
            }

            _clipCache[key] = clip;
            return clip;
        }

        // ── Clip Builders ────────────────────────────────────────────────────
        private static void BuildPowerShotClip(AnimationClip clip)
        {
            float dur = 0.50f; // 500ms
            // Right leg: windup (-75° at 0.15s) -> strike (+65° at 0.35s) -> settle (+20° at 0.5s)
            var legR = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.15f, -75f),
                new Keyframe(0.32f, 65f),
                new Keyframe(0.50f, 15f)
            );
            SetCurveRot(clip, "Hips/Leg_R", legR, null, null);

            // Left leg: plant firmly
            var legL = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.15f, 10f),
                new Keyframe(0.32f, -10f),
                new Keyframe(0.50f, 0f)
            );
            SetCurveRot(clip, "Hips/Leg_L", legL, null, null);

            // Torso: windup back (-15°) -> snap forward (+20°)
            var torsoRot = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.15f, -18f),
                new Keyframe(0.32f, 22f),
                new Keyframe(0.50f, 5f)
            );
            SetCurveRot(clip, "Hips/Torso", torsoRot, null, null);
        }

        private static void BuildFinesseCurlClip(AnimationClip clip)
        {
            float dur = 0.55f;
            // Curled inside of boot strike with hip tilt
            var legR = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.18f, -55f),
                new Keyframe(0.35f, 45f),
                new Keyframe(0.55f, 10f)
            );
            var legRY = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.18f, -25f), // Outward hip rotation
                new Keyframe(0.35f, 35f),  // Inward wrap
                new Keyframe(0.55f, 0f)
            );
            SetCurveRot(clip, "Hips/Leg_R", legR, legRY, null);

            var torsoRotY = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.18f, -15f),
                new Keyframe(0.35f, 25f),
                new Keyframe(0.55f, 0f)
            );
            SetCurveRot(clip, "Hips/Torso", null, torsoRotY, null);
        }

        private static void BuildHeaderClip(AnimationClip clip)
        {
            float dur = 0.65f;
            // Vertical leap and forehead snap
            var hipY = new AnimationCurve(
                new Keyframe(0f, 0.85f),
                new Keyframe(0.15f, 0.70f), // Crouch
                new Keyframe(0.35f, 1.35f), // Peak leap
                new Keyframe(0.65f, 0.85f)  // Landing
            );
            SetCurvePos(clip, "Hips", null, hipY, null);

            // Head arch and thrust
            var headX = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.25f, -25f), // Arch back
                new Keyframe(0.38f, 35f),  // Forward header snap
                new Keyframe(0.65f, 0f)
            );
            SetCurveRot(clip, "Hips/Torso/Head", headX, null, null);
        }

        private static void BuildSlidingTackleClip(AnimationClip clip)
        {
            float dur = 0.80f;
            var hipY = new AnimationCurve(
                new Keyframe(0f, 0.85f),
                new Keyframe(0.20f, 0.40f), // Slide low
                new Keyframe(0.60f, 0.40f),
                new Keyframe(0.80f, 0.85f)  // Stand back up
            );
            SetCurvePos(clip, "Hips", null, hipY, null);

            // Right leg extended forward
            var legR = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.20f, 75f),
                new Keyframe(0.60f, 70f),
                new Keyframe(0.80f, 0f)
            );
            SetCurveRot(clip, "Hips/Leg_R", legR, null, null);
        }

        private static void BuildGKDiveClip(AnimationClip clip, bool isLeft)
        {
            float dur = 0.75f;
            float sign = isLeft ? -1f : 1f;

            // Hips dive laterally and downward
            var hipX = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.35f, sign * 1.6f),
                new Keyframe(0.75f, sign * 1.8f)
            );
            var hipY = new AnimationCurve(
                new Keyframe(0f, 0.85f),
                new Keyframe(0.30f, 0.60f),
                new Keyframe(0.75f, 0.25f)
            );
            SetCurvePos(clip, "Hips", hipX, hipY, null);

            // Torso lateral tilt
            var torsoZ = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.35f, -sign * 70f),
                new Keyframe(0.75f, -sign * 80f)
            );
            SetCurveRot(clip, "Hips/Torso", null, null, torsoZ);

            // Extended glove arm
            string armPath = isLeft ? "Hips/Torso/Arm_L" : "Hips/Torso/Arm_R";
            var armRot = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.30f, -sign * 120f),
                new Keyframe(0.75f, -sign * 110f)
            );
            SetCurveRot(clip, armPath, null, null, armRot);
        }

        private static void BuildGKJumpSaveClip(AnimationClip clip)
        {
            float dur = 0.70f;
            // High upward two-handed parry
            var hipY = new AnimationCurve(
                new Keyframe(0f, 0.85f),
                new Keyframe(0.12f, 0.65f), // Crouch
                new Keyframe(0.35f, 1.45f), // Apex tip over crossbar
                new Keyframe(0.70f, 0.85f)
            );
            SetCurvePos(clip, "Hips", null, hipY, null);

            // Both arms thrust up high
            var armX = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.35f, -160f),
                new Keyframe(0.70f, 0f)
            );
            SetCurveRot(clip, "Hips/Torso/Arm_L", armX, null, null);
            SetCurveRot(clip, "Hips/Torso/Arm_R", armX, null, null);
        }

        private static void BuildKneeSlideClip(AnimationClip clip)
        {
            float dur = 1.20f;
            // Low turf knee slide with arched back and pumped arms
            var hipY = new AnimationCurve(
                new Keyframe(0f, 0.85f),
                new Keyframe(0.20f, 0.38f),
                new Keyframe(0.90f, 0.38f),
                new Keyframe(1.20f, 0.85f)
            );
            SetCurvePos(clip, "Hips", null, hipY, null);

            var torsoX = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.20f, -25f), // Arched back
                new Keyframe(0.90f, -20f),
                new Keyframe(1.20f, 0f)
            );
            SetCurveRot(clip, "Hips/Torso", torsoX, null, null);

            // Bent knees
            var legX = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.20f, 65f),
                new Keyframe(0.90f, 65f),
                new Keyframe(1.20f, 0f)
            );
            SetCurveRot(clip, "Hips/Leg_L", legX, null, null);
            SetCurveRot(clip, "Hips/Leg_R", legX, null, null);
        }

        private static void BuildFistPumpClip(AnimationClip clip)
        {
            float dur = 1.0f;
            // Vertical leap with high energetic fist pump
            var hipY = new AnimationCurve(
                new Keyframe(0f, 0.85f),
                new Keyframe(0.20f, 0.65f),
                new Keyframe(0.45f, 1.25f),
                new Keyframe(0.75f, 0.85f)
            );
            SetCurvePos(clip, "Hips", null, hipY, null);

            // Right arm driving skyward
            var armR = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.20f, 30f),
                new Keyframe(0.45f, -160f),
                new Keyframe(0.70f, -120f),
                new Keyframe(0.90f, -160f),
                new Keyframe(1.0f, 0f)
            );
            SetCurveRot(clip, "Hips/Torso/Arm_R", armR, null, null);
        }

        private static void BuildCrowdWaveClip(AnimationClip clip)
        {
            float dur = 1.50f;
            // Both hands raised waving to fans
            var armLX = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.20f, -140f),
                new Keyframe(1.30f, -140f),
                new Keyframe(1.50f, 0f)
            );
            var armRX = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.20f, -140f),
                new Keyframe(1.30f, -140f),
                new Keyframe(1.50f, 0f)
            );
            var waveZ = CreateSinCurve(dur, -15f, 15f, 4);
            SetCurveRot(clip, "Hips/Torso/Arm_L", armLX, null, waveZ);
            SetCurveRot(clip, "Hips/Torso/Arm_R", armRX, null, waveZ);
        }

        // ── Curve Helpers ────────────────────────────────────────────────────
        private static AnimationCurve CreateSinCurve(float duration, float min, float max, int cycles)
        {
            var curve = new AnimationCurve();
            int points = Math.Max(8, cycles * 6);
            for (int i = 0; i <= points; i++)
            {
                float t = (float)i / points * duration;
                float phase = (float)i / points * (Mathf.PI * 2 * cycles);
                float val = Mathf.Lerp(min, max, (Mathf.Sin(phase) + 1f) * 0.5f);
                curve.AddKey(new Keyframe(t, val));
            }
            return curve;
        }

        private static AnimationCurve CreateBounceCurve(float duration, float min, float max, int cycles)
        {
            var curve = new AnimationCurve();
            int points = Math.Max(8, cycles * 6);
            for (int i = 0; i <= points; i++)
            {
                float t = (float)i / points * duration;
                float phase = (float)i / points * (Mathf.PI * 2 * cycles);
                float val = Mathf.Lerp(min, max, Mathf.Abs(Mathf.Cos(phase)));
                curve.AddKey(new Keyframe(t, val));
            }
            return curve;
        }

        private static AnimationCurve CreateFlatCurve(float duration, float val)
        {
            return new AnimationCurve(new Keyframe(0f, val), new Keyframe(duration, val));
        }

        private static void SetCurvePos(AnimationClip clip, string path, AnimationCurve? x, AnimationCurve? y, AnimationCurve? z)
        {
            if (x != null) clip.SetCurve(path, typeof(Transform), "localPosition.x", x);
            if (y != null) clip.SetCurve(path, typeof(Transform), "localPosition.y", y);
            if (z != null) clip.SetCurve(path, typeof(Transform), "localPosition.z", z);
        }

        private static void SetCurveRot(AnimationClip clip, string path, AnimationCurve? eulerX, AnimationCurve? eulerY, AnimationCurve? eulerZ)
        {
            if (eulerX != null) clip.SetCurve(path, typeof(Transform), "localEulerAngles.x", eulerX);
            if (eulerY != null) clip.SetCurve(path, typeof(Transform), "localEulerAngles.y", eulerY);
            if (eulerZ != null) clip.SetCurve(path, typeof(Transform), "localEulerAngles.z", eulerZ);
        }
    }
}
