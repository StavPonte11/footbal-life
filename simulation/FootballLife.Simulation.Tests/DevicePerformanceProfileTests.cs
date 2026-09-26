using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class DevicePerformanceProfileTests
    {
        [Theory]
        [InlineData(2048, 4, 512, false, DeviceTier.Low)]      // Low-end budget phone (e.g. Galaxy A03, Moto E)
        [InlineData(3000, 4, 512, true, DeviceTier.Low)]       // Under 3GB RAM
        [InlineData(4096, 4, 1024, true, DeviceTier.Low)]      // Quad-core CPU constraint
        [InlineData(4096, 6, 1536, true, DeviceTier.Medium)]   // Solid mid-ranger (e.g. Galaxy A53, Pixel 6a)
        [InlineData(5800, 8, 2048, true, DeviceTier.Medium)]   // High CPU but sub-6GB RAM
        [InlineData(6144, 8, 2048, true, DeviceTier.High)]     // Flagship (e.g. Galaxy S22/S23, iPhone 14/15)
        [InlineData(12288, 8, 4096, true, DeviceTier.High)]    // High-end flagship (12GB RAM)
        public void DeviceTierClassifier_ClassifiesHardwareAccurately(
            int systemRamMb,
            int processorCount,
            int graphicsMemoryMb,
            bool supportsComputeShaders,
            DeviceTier expectedTier)
        {
            var tier = DeviceTierClassifier.Classify(
                systemMemoryMb: systemRamMb,
                processorCount: processorCount,
                graphicsMemoryMb: graphicsMemoryMb,
                supportsComputeShaders: supportsComputeShaders
            );

            Assert.Equal(expectedTier, tier);
        }

        [Fact]
        public void DevicePerformanceProfile_LowTier_SatisfiesBudgetConstraints()
        {
            var profile = DevicePerformanceProfile.GetProfile(DeviceTier.Low);

            Assert.Equal(DeviceTier.Low, profile.Tier);
            Assert.Equal(QualityTier.Low, profile.QualityTier);
            Assert.Equal(30, profile.TargetFpsGameplay);
            Assert.Equal(30, profile.TargetFpsUI);
            Assert.True(profile.RenderScale <= 0.85f);
            Assert.True(profile.CrowdDensityFactor <= 0.50f);
            Assert.False(profile.EnableShadowCascades);
            Assert.True(profile.TargetMaxRamMb <= 150);
        }

        [Fact]
        public void DevicePerformanceProfile_MidTier_SatisfiesBudgetConstraints()
        {
            var profile = DevicePerformanceProfile.GetProfile(DeviceTier.Medium);

            Assert.Equal(DeviceTier.Medium, profile.Tier);
            Assert.Equal(QualityTier.Medium, profile.QualityTier);
            Assert.Equal(60, profile.TargetFpsGameplay);
            Assert.Equal(30, profile.TargetFpsUI);
            Assert.Equal(1.0f, profile.RenderScale);
            Assert.True(profile.CrowdDensityFactor >= 0.60f && profile.CrowdDensityFactor <= 0.80f);
            Assert.True(profile.EnableShadowCascades);
            Assert.True(profile.TargetMaxRamMb <= 180);
        }

        [Fact]
        public void DevicePerformanceProfile_HighTier_MaxVisualsWithin200Mb()
        {
            var profile = DevicePerformanceProfile.GetProfile(DeviceTier.High);

            Assert.Equal(DeviceTier.High, profile.Tier);
            Assert.Equal(QualityTier.High, profile.QualityTier);
            Assert.Equal(60, profile.TargetFpsGameplay);
            Assert.Equal(30, profile.TargetFpsUI);
            Assert.Equal(1.0f, profile.RenderScale);
            Assert.Equal(1.0f, profile.CrowdDensityFactor);
            Assert.True(profile.EnableShadowCascades);
            Assert.Equal(PerformanceBudget.MaxTotalRamMb, profile.TargetMaxRamMb);
        }
    }
}
