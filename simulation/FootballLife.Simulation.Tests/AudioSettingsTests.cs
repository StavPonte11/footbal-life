using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class AudioSettingsTests
    {
        [Fact]
        public void DefaultSettings_HaveExpectedStandardVolumes()
        {
            var config = AudioSettingsConfig.Default;

            Assert.Equal(1.0f, config.MasterVolume);
            Assert.Equal(1.0f, config.SfxVolume);
            Assert.Equal(0.85f, config.AmbienceVolume);
            Assert.Equal(1.0f, config.UiVolume);
            Assert.Equal(0.70f, config.MusicVolume);

            Assert.False(config.MasterMuted);
            Assert.False(config.SfxMuted);
            Assert.False(config.AmbienceMuted);
            Assert.False(config.UiMuted);
            Assert.False(config.MusicMuted);
        }

        [Fact]
        public void GetEffectiveLinearVolume_CalculatesCombinedMultiplier()
        {
            var config = AudioSettingsConfig.Default
                .WithVolume(AudioBus.Master, 0.8f)
                .WithVolume(AudioBus.SFX, 0.5f);

            // 0.8 * 0.5 = 0.40
            Assert.Equal(0.40f, config.GetEffectiveLinearVolume(AudioBus.SFX), 2);
            Assert.Equal(0.80f, config.GetEffectiveLinearVolume(AudioBus.Master), 2);
        }

        [Fact]
        public void GetEffectiveLinearVolume_ReturnsZeroWhenMuted()
        {
            var config = AudioSettingsConfig.Default
                .WithMute(AudioBus.SFX, true);

            Assert.Equal(0.0f, config.GetEffectiveLinearVolume(AudioBus.SFX));
            Assert.True(config.GetEffectiveLinearVolume(AudioBus.UI) > 0f);

            var masterMuted = config.WithMute(AudioBus.Master, true);
            Assert.Equal(0.0f, masterMuted.GetEffectiveLinearVolume(AudioBus.SFX));
            Assert.Equal(0.0f, masterMuted.GetEffectiveLinearVolume(AudioBus.UI));
            Assert.Equal(0.0f, masterMuted.GetEffectiveLinearVolume(AudioBus.Music));
        }

        [Theory]
        [InlineData(1.0f, 0.0f)]
        [InlineData(0.1f, -20.0f)]
        [InlineData(0.01f, -40.0f)]
        [InlineData(0.0001f, -80.0f)]
        [InlineData(0.0f, -80.0f)]
        public void LinearToDecibels_ComputesAccurateLogarithmicAttenuation(float linear, float expectedDb)
        {
            float db = AudioSettingsConfig.LinearToDecibels(linear);
            Assert.Equal(expectedDb, db, 1);
        }

        [Theory]
        [InlineData(0.0f, 1.0f)]
        [InlineData(-20.0f, 0.1f)]
        [InlineData(-40.0f, 0.01f)]
        [InlineData(-80.0f, 0.0f)]
        public void DecibelsToLinear_ComputesAccurateLinearPower(float db, float expectedLinear)
        {
            float linear = AudioSettingsConfig.DecibelsToLinear(db);
            Assert.Equal(expectedLinear, linear, 2);
        }

        [Fact]
        public void WithVolume_ClampsBetweenZeroAndOne()
        {
            var config = AudioSettingsConfig.Default
                .WithVolume(AudioBus.Music, 1.5f);
            Assert.Equal(1.0f, config.MusicVolume);

            var negativeConfig = AudioSettingsConfig.Default
                .WithVolume(AudioBus.Music, -0.5f);
            Assert.Equal(0.0f, negativeConfig.MusicVolume);
        }
    }
}
