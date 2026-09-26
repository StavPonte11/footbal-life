using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Audio bus channels for volume mixing and muting.
    /// </summary>
    public enum AudioBus
    {
        Master,
        SFX,
        Ambience,
        UI,
        Music
    }

    /// <summary>
    /// Pure domain model representing the player's audio mixing preferences,
    /// volume levels [0.0..1.0], mute states, and decibel logarithmic conversions.
    /// </summary>
    public record AudioSettingsConfig
    {
        public float MasterVolume { get; init; } = 1.0f;
        public float SfxVolume { get; init; } = 1.0f;
        public float AmbienceVolume { get; init; } = 0.85f;
        public float UiVolume { get; init; } = 1.0f;
        public float MusicVolume { get; init; } = 0.70f;

        public bool MasterMuted { get; init; } = false;
        public bool SfxMuted { get; init; } = false;
        public bool AmbienceMuted { get; init; } = false;
        public bool UiMuted { get; init; } = false;
        public bool MusicMuted { get; init; } = false;

        public static AudioSettingsConfig Default => new AudioSettingsConfig();

        public float GetVolume(AudioBus bus) => bus switch
        {
            AudioBus.Master => MasterVolume,
            AudioBus.SFX => SfxVolume,
            AudioBus.Ambience => AmbienceVolume,
            AudioBus.UI => UiVolume,
            AudioBus.Music => MusicVolume,
            _ => 1.0f
        };

        public bool IsMuted(AudioBus bus) => bus switch
        {
            AudioBus.Master => MasterMuted,
            AudioBus.SFX => SfxMuted,
            AudioBus.Ambience => AmbienceMuted,
            AudioBus.UI => UiMuted,
            AudioBus.Music => MusicMuted,
            _ => false
        };

        /// <summary>
        /// Calculates the final linear volume [0.0..1.0] taking master volume and individual mute states into account.
        /// </summary>
        public float GetEffectiveLinearVolume(AudioBus bus)
        {
            if (MasterMuted || IsMuted(bus))
                return 0.0f;

            float busVol = Math.Clamp(GetVolume(bus), 0.0f, 1.0f);
            float masterVol = Math.Clamp(MasterVolume, 0.0f, 1.0f);

            return (bus == AudioBus.Master) ? masterVol : (busVol * masterVol);
        }

        /// <summary>
        /// Converts a linear volume [0.0..1.0] to decibels [-80.0dB..0.0dB] for AudioMixer attenuation.
        /// </summary>
        public static float LinearToDecibels(float linearVolume)
        {
            if (linearVolume <= 0.0001f)
                return -80.0f;

            float clamped = Math.Clamp(linearVolume, 0.0001f, 1.0f);
            return (float)(20.0 * Math.Log10(clamped));
        }

        /// <summary>
        /// Converts decibels [-80.0dB..0.0dB] to a linear volume [0.0..1.0].
        /// </summary>
        public static float DecibelsToLinear(float decibels)
        {
            if (decibels <= -80.0f)
                return 0.0f;

            float clampedDb = Math.Clamp(decibels, -80.0f, 0.0f);
            return (float)Math.Pow(10.0, clampedDb / 20.0);
        }

        public AudioSettingsConfig WithVolume(AudioBus bus, float volume)
        {
            float clamped = Math.Clamp(volume, 0.0f, 1.0f);
            return bus switch
            {
                AudioBus.Master => this with { MasterVolume = clamped },
                AudioBus.SFX => this with { SfxVolume = clamped },
                AudioBus.Ambience => this with { AmbienceVolume = clamped },
                AudioBus.UI => this with { UiVolume = clamped },
                AudioBus.Music => this with { MusicVolume = clamped },
                _ => this
            };
        }

        public AudioSettingsConfig WithMute(AudioBus bus, bool muted) => bus switch
        {
            AudioBus.Master => this with { MasterMuted = muted },
            AudioBus.SFX => this with { SfxMuted = muted },
            AudioBus.Ambience => this with { AmbienceMuted = muted },
            AudioBus.UI => this with { UiMuted = muted },
            AudioBus.Music => this with { MusicMuted = muted },
            _ => this
        };
    }
}
