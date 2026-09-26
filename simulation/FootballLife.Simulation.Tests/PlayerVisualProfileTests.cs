using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public class PlayerVisualProfileTests
    {
        [Fact]
        public void CreateDeterministic_ReturnsConsistentProfileForSameInput()
        {
            var p1 = PlayerVisualProfile.CreateDeterministic("Marcus Vance", 9, Position.ST);
            var p2 = PlayerVisualProfile.CreateDeterministic("Marcus Vance", 9, Position.ST);

            Assert.Equal(p1.SkinTone, p2.SkinTone);
            Assert.Equal(p1.HairStyle, p2.HairStyle);
            Assert.Equal(p1.HairColor, p2.HairColor);
            Assert.Equal(p1.FacialHair, p2.FacialHair);
            Assert.Equal(p1.BootStyle, p2.BootStyle);
            Assert.Equal(p1.HeightMeters, p2.HeightMeters);
        }

        [Fact]
        public void CreateDeterministic_GeneratesDifferentProfilesForDifferentPlayers()
        {
            var p1 = PlayerVisualProfile.CreateDeterministic("Marcus Vance", 9, Position.ST);
            var p2 = PlayerVisualProfile.CreateDeterministic("Oliver Kahn", 1, Position.GK);

            Assert.True(p1.SkinTone != p2.SkinTone ||
                        p1.HairStyle != p2.HairStyle ||
                        p1.HairColor != p2.HairColor ||
                        p1.BootStyle != p2.BootStyle);
        }

        [Fact]
        public void Goalkeeper_IsTallerThanTypicalOutfield()
        {
            var gk = PlayerVisualProfile.CreateDeterministic("Oliver Kahn", 1, Position.GK);
            Assert.True(gk.HeightMeters >= 1.88f);
        }

        [Fact]
        public void GetSkinColor_ReturnsValidNonZeroRGB()
        {
            var profile = new PlayerVisualProfile { SkinTone = SkinToneType.Olive };
            var color = profile.GetSkinColor();

            Assert.True(color.R > 0f && color.R <= 1f);
            Assert.True(color.G > 0f && color.G <= 1f);
            Assert.True(color.B > 0f && color.B <= 1f);
        }

        [Fact]
        public void GetHairColor_ReturnsValidRGB()
        {
            var profile = new PlayerVisualProfile { HairColor = HairColorType.GoldenBlonde };
            var color = profile.GetHairColor();

            Assert.True(color.R > color.B); // blonde has high red/yellow
        }
    }
}
