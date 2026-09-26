using System;
using UnityEngine;

namespace FootballLife.Unity.Core.Audio
{
    /// <summary>
    /// Generates high-fidelity procedural PCM AudioClips in-engine at runtime.
    /// Guarantees that Football Life has 100% complete, rich audio feedback across
    /// all platforms and test runners without requiring external multi-megabyte sound libraries.
    /// </summary>
    public static class ProceduralAudioClipFactory
    {
        private const int SampleRate = 44100;

        /// <summary>
        /// Generates a punchy ball kick sound.
        /// </summary>
        /// <param name="isHard">True for power shots / headers, false for passes / soft taps.</param>
        public static AudioClip CreateKickClip(bool isHard = true)
        {
            float duration = isHard ? 0.24f : 0.16f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            float startFreq = isHard ? 170f : 120f;
            float endFreq = isHard ? 42f : 55f;
            float phase = 0f;

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;

                // Non-linear downward pitch bend (thump)
                float currentFreq = Mathf.Lerp(startFreq, endFreq, Mathf.Sqrt(progress));
                phase += 2f * Mathf.PI * currentFreq / SampleRate;

                // Sine wave fundamental
                float wave = Mathf.Sin(phase);

                // Initial burst of noise on impact (first 8ms)
                float noise = 0f;
                if (t < 0.012f)
                {
                    float noiseEnv = 1f - (t / 0.012f);
                    noise = ((float)UnityEngine.Random.value * 2f - 1f) * noiseEnv * 0.45f;
                }

                // Envelope: instantaneous attack, exponential decay
                float envelope = Mathf.Exp(-progress * 6.5f);
                samples[i] = Mathf.Clamp((wave + noise) * envelope, -1f, 1f);
            }

            var clip = AudioClip.Create(isHard ? "Procedural_Kick_Hard" : "Procedural_Kick_Soft", numSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a sharp metallic post/crossbar strike sound.
        /// </summary>
        public static AudioClip CreateWoodworkClip()
        {
            float duration = 0.45f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            // Inharmonic metallic modal frequencies
            float f1 = 860f;
            float f2 = 1740f;
            float f3 = 2620f;

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;

                float wave1 = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.55f;
                float wave2 = Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.30f;
                float wave3 = Mathf.Sin(2f * Mathf.PI * f3 * t) * 0.15f;

                // Sharp initial impact click
                float impact = (t < 0.005f) ? (1f - (t / 0.005f)) * 0.4f : 0f;

                float decay = Mathf.Exp(-progress * 5.2f);
                samples[i] = Mathf.Clamp((wave1 + wave2 + wave3 + impact) * decay, -1f, 1f);
            }

            var clip = AudioClip.Create("Procedural_Woodwork", numSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a satisfying swoosh/rustle of the goal net being struck.
        /// </summary>
        public static AudioClip CreateNetRippleClip()
        {
            float duration = 0.38f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            float lastNoise = 0f;
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;

                // Low-pass filtered noise to simulate cord friction
                float rawNoise = (float)UnityEngine.Random.value * 2f - 1f;
                lastNoise = Mathf.Lerp(lastNoise, rawNoise, 0.25f);

                // Attack and release envelope
                float env = 0f;
                if (progress < 0.20f)
                    env = progress / 0.20f;
                else
                    env = Mathf.Exp(-(progress - 0.20f) * 4.5f);

                samples[i] = Mathf.Clamp(lastNoise * env * 0.85f, -1f, 1f);
            }

            var clip = AudioClip.Create("Procedural_Net_Ripple", numSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates authentic referee whistle blasts.
        /// </summary>
        public static AudioClip CreateWhistleClip(WhistleType type)
        {
            float duration = type switch
            {
                WhistleType.SituationStart => 0.36f,
                WhistleType.GoalScored => 0.85f,
                WhistleType.FullTime => 1.30f,
                _ => 0.40f
            };

            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            float f1 = 2820f;
            float f2 = 3010f; // Trill beat frequency (~190Hz difference)

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;

                // Tremolo flutter
                float tremolo = 0.85f + (0.15f * Mathf.Sin(2f * Mathf.PI * 14f * t));

                float tone = (Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.5f) +
                             (Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.5f);

                // Envelope based on whistle pattern
                float env = 0f;
                if (type == WhistleType.FullTime)
                {
                    // Pattern: Pip, Pip, Peep (short, short, long)
                    if (t < 0.20f) env = Mathf.Sin(Mathf.PI * (t / 0.20f));
                    else if (t < 0.30f) env = 0f;
                    else if (t < 0.50f) env = Mathf.Sin(Mathf.PI * ((t - 0.30f) / 0.20f));
                    else if (t < 0.60f) env = 0f;
                    else env = Mathf.Sin(Mathf.PI * Mathf.Clamp01((t - 0.60f) / 0.70f));
                }
                else
                {
                    // Single continuous blast with soft attack & decay
                    float attack = 0.04f;
                    float release = 0.08f;
                    if (t < attack) env = t / attack;
                    else if (t > duration - release) env = (duration - t) / release;
                    else env = 1.0f;
                }

                // Add slight turbulent air hiss
                float air = ((float)UnityEngine.Random.value * 2f - 1f) * 0.08f;
                samples[i] = Mathf.Clamp((tone * tremolo + air) * env * 0.80f, -1f, 1f);
            }

            var clip = AudioClip.Create($"Procedural_Whistle_{type}", numSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates continuous ambient stadium crowd murmur. Seamless loop.
        /// </summary>
        public static AudioClip CreateCrowdMurmurClip()
        {
            float duration = 4.0f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            float n1 = 0f, n2 = 0f;
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;

                // Multiple layered filtered noise bands
                float raw = (float)UnityEngine.Random.value * 2f - 1f;
                n1 = Mathf.Lerp(n1, raw, 0.04f); // Low rumble (80-200Hz)
                n2 = Mathf.Lerp(n2, raw, 0.12f); // Mid chatter (200-800Hz)

                // Slow breathing modulation (simulates collective crowd chant waves)
                float breath = 0.75f + (0.25f * Mathf.Sin(2f * Mathf.PI * 0.33f * t));

                // Seamless loop crossfade at boundaries
                float loopFade = 1.0f;
                float fadeDur = 0.25f;
                if (t < fadeDur) loopFade = t / fadeDur;
                else if (t > duration - fadeDur) loopFade = (duration - t) / fadeDur;

                samples[i] = Mathf.Clamp((n1 * 0.7f + n2 * 0.3f) * breath * loopFade * 0.70f, -1f, 1f);
            }

            var clip = AudioClip.Create("Procedural_Crowd_Murmur", numSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates an explosive stadium goal celebration roar.
        /// </summary>
        public static AudioClip CreateCrowdRoarClip()
        {
            float duration = 3.0f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            float n1 = 0f, n2 = 0f;
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;

                float raw = (float)UnityEngine.Random.value * 2f - 1f;
                n1 = Mathf.Lerp(n1, raw, 0.08f);
                n2 = Mathf.Lerp(n2, raw, 0.28f);

                // Quick explosive attack (0.2s), sustained cheer (1.0s), gradual decay
                float env = 0f;
                if (t < 0.25f)
                    env = Mathf.SmoothStep(0f, 1f, t / 0.25f);
                else if (t < 1.4f)
                    env = 1.0f;
                else
                    env = Mathf.Exp(-(t - 1.4f) * 1.8f);

                // Cheering pitch modulation
                float cheerTone = Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.08f;

                samples[i] = Mathf.Clamp((n1 * 0.55f + n2 * 0.45f + cheerTone) * env * 0.95f, -1f, 1f);
            }

            var clip = AudioClip.Create("Procedural_Crowd_Roar", numSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a sharp collective crowd gasp on a near-miss or diving save.
        /// </summary>
        public static AudioClip CreateCrowdGaspClip()
        {
            float duration = 0.75f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            float n = 0f;
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;

                float raw = (float)UnityEngine.Random.value * 2f - 1f;
                n = Mathf.Lerp(n, raw, 0.18f);

                // Fast inhalation curve
                float env = Mathf.Sin(Mathf.PI * Mathf.Pow(progress, 0.7f));
                samples[i] = Mathf.Clamp(n * env * 0.80f, -1f, 1f);
            }

            var clip = AudioClip.Create("Procedural_Crowd_Gasp", numSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a subtle, crisp UI button tap click.
        /// </summary>
        public static AudioClip CreateUIClickClip()
        {
            float duration = 0.035f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;

                float tone = Mathf.Sin(2f * Mathf.PI * 1250f * t);
                float env = Mathf.Exp(-progress * 18.0f);
                samples[i] = tone * env * 0.50f;
            }

            var clip = AudioClip.Create("Procedural_UI_Click", numSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a smooth navigation tab switch sound.
        /// </summary>
        public static AudioClip CreateUITabSwitchClip()
        {
            float duration = 0.06f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;

                float freq = Mathf.Lerp(650f, 1300f, progress);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t);
                float env = Mathf.Sin(Mathf.PI * progress);
                samples[i] = tone * env * 0.40f;
            }

            var clip = AudioClip.Create("Procedural_UI_TabSwitch", numSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a celebratory ascending chime on weekly wage deposits or financial rewards.
        /// </summary>
        public static AudioClip CreateUIWageChimeClip()
        {
            float duration = 0.85f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            // Ascending major chord frequencies: C6, E6, G6, C7
            float[] notes = { 1046.50f, 1318.51f, 1567.98f, 2093.00f };
            float[] startTimes = { 0.00f, 0.12f, 0.24f, 0.36f };

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float sum = 0f;

                for (int n = 0; n < notes.Length; n++)
                {
                    if (t >= startTimes[n])
                    {
                        float localT = t - startTimes[n];
                        float localEnv = Mathf.Exp(-localT * 6.5f);
                        // Fundamental + pleasant 2nd harmonic
                        float wave = Mathf.Sin(2f * Mathf.PI * notes[n] * localT) +
                                     (Mathf.Sin(2f * Mathf.PI * notes[n] * 2f * localT) * 0.25f);
                        sum += wave * localEnv * 0.25f;
                    }
                }

                samples[i] = Mathf.Clamp(sum, -1f, 1f);
            }

            var clip = AudioClip.Create("Procedural_UI_WageChime", numSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates an attention alert chime for dilemmas or contract proposals.
        /// </summary>
        public static AudioClip CreateUIAlertClip()
        {
            float duration = 0.50f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            // Two-tone bell: A5 (880Hz) to D6 (1174.6Hz)
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                float sum = 0f;

                // First chime (0s)
                float env1 = Mathf.Exp(-t * 9f);
                sum += Mathf.Sin(2f * Mathf.PI * 880f * t) * env1 * 0.4f;

                // Second chime (0.16s)
                if (t >= 0.16f)
                {
                    float t2 = t - 0.16f;
                    float env2 = Mathf.Exp(-t2 * 7f);
                    sum += Mathf.Sin(2f * Mathf.PI * 1174.66f * t2) * env2 * 0.5f;
                }

                samples[i] = Mathf.Clamp(sum, -1f, 1f);
            }

            var clip = AudioClip.Create("Procedural_UI_Alert", numSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a relaxed, mellow lofi electronic ambient music loop for the Career Hub.
        /// Seamless 8-second loop with gentle chord progression.
        /// </summary>
        public static AudioClip CreateMenuMusicClip()
        {
            float duration = 8.0f;
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            // Chord root frequencies (Em7 -> Cmaj7 -> G -> D)
            // 2 seconds per chord
            float[][] chordFrequencies = new float[][]
            {
                new float[] { 164.81f, 196.00f, 246.94f, 293.66f }, // E3, G3, B3, D4 (Em7)
                new float[] { 130.81f, 164.81f, 196.00f, 246.94f }, // C3, E3, G3, B3 (Cmaj7)
                new float[] { 196.00f, 246.94f, 293.66f, 392.00f }, // G3, B3, D4, G4 (G)
                new float[] { 146.83f, 220.00f, 293.66f, 369.99f }  // D3, A3, D4, F#4 (D)
            };

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / SampleRate;
                int chordIdx = Mathf.Clamp((int)(t / 2.0f), 0, 3);
                float chordT = t % 2.0f;

                // Soft bell/electric piano envelope per chord
                float chordEnv = Mathf.Sin(Mathf.PI * Mathf.Clamp01(chordT / 2.0f));

                float chordMix = 0f;
                float[] freqs = chordFrequencies[chordIdx];
                for (int f = 0; f < freqs.Length; f++)
                {
                    // Soft sine wave
                    float sine = Mathf.Sin(2f * Mathf.PI * freqs[f] * t);
                    chordMix += sine * 0.12f;
                }

                // Sub bass on chord root
                float subBass = Mathf.Sin(2f * Mathf.PI * (freqs[0] * 0.5f) * t) * 0.18f;

                // Gentle rhythmic lofi heartbeat pulse on quarter notes (every 0.5s)
                float pulseT = t % 0.5f;
                float pulseEnv = Mathf.Exp(-pulseT * 16.0f);
                float pulse = Mathf.Sin(2f * Mathf.PI * 65f * pulseT) * pulseEnv * 0.15f;

                // Seamless loop fade
                float loopFade = 1.0f;
                float fadeDur = 0.15f;
                if (t < fadeDur) loopFade = t / fadeDur;
                else if (t > duration - fadeDur) loopFade = (duration - t) / fadeDur;

                samples[i] = Mathf.Clamp(((chordMix * chordEnv) + subBass + pulse) * loopFade * 0.55f, -1f, 1f);
            }

            var clip = AudioClip.Create("Procedural_Menu_Music", numSamples, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
