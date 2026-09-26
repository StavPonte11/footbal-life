using System;

namespace FootballLife.Domain
{
    public enum SkinToneType
    {
        Fair,
        Olive,
        Tan,
        DeepBronze,
        MelaninDark
    }

    public enum HairStyleType
    {
        ShortCrop,
        BuzzFade,
        TexturedAfro,
        SlickBack,
        LongTied,
        Undercut
    }

    public enum HairColorType
    {
        JetBlack,
        DarkBrown,
        LightBrown,
        GoldenBlonde,
        AuburnRed,
        PlatinumSilver
    }

    public enum FacialHairType
    {
        CleanShaven,
        LightStubble,
        TrimmedBeard,
        Goatee
    }

    public enum BootStyleType
    {
        ClassicBlack,
        NeonSpeed,
        CleanWhite,
        CrimsonStrike
    }

    public readonly record struct VisualColor(float R, float G, float B)
    {
        public static VisualColor FairSkin => new VisualColor(0.96f, 0.84f, 0.74f);
        public static VisualColor OliveSkin => new VisualColor(0.85f, 0.70f, 0.54f);
        public static VisualColor TanSkin => new VisualColor(0.78f, 0.53f, 0.33f);
        public static VisualColor DeepBronzeSkin => new VisualColor(0.55f, 0.33f, 0.14f);
        public static VisualColor MelaninDarkSkin => new VisualColor(0.24f, 0.15f, 0.10f);

        public static VisualColor JetBlackHair => new VisualColor(0.10f, 0.10f, 0.10f);
        public static VisualColor DarkBrownHair => new VisualColor(0.23f, 0.13f, 0.10f);
        public static VisualColor LightBrownHair => new VisualColor(0.42f, 0.27f, 0.14f);
        public static VisualColor GoldenBlondeHair => new VisualColor(0.83f, 0.69f, 0.22f);
        public static VisualColor AuburnRedHair => new VisualColor(0.57f, 0.15f, 0.14f);
        public static VisualColor PlatinumSilverHair => new VisualColor(0.86f, 0.86f, 0.86f);
    }

    /// <summary>
    /// Pure C# domain model defining a footballer's visual identity, ethnicity,
    /// hairstyle, facial features, and footwear styling.
    /// </summary>
    public sealed record PlayerVisualProfile
    {
        public SkinToneType SkinTone { get; init; } = SkinToneType.Tan;
        public HairStyleType HairStyle { get; init; } = HairStyleType.ShortCrop;
        public HairColorType HairColor { get; init; } = HairColorType.DarkBrown;
        public FacialHairType FacialHair { get; init; } = FacialHairType.CleanShaven;
        public BootStyleType BootStyle { get; init; } = BootStyleType.NeonSpeed;
        public float HeightMeters { get; init; } = 1.82f;

        public VisualColor GetSkinColor() => SkinTone switch
        {
            SkinToneType.Fair => VisualColor.FairSkin,
            SkinToneType.Olive => VisualColor.OliveSkin,
            SkinToneType.Tan => VisualColor.TanSkin,
            SkinToneType.DeepBronze => VisualColor.DeepBronzeSkin,
            SkinToneType.MelaninDark => VisualColor.MelaninDarkSkin,
            _ => VisualColor.TanSkin
        };

        public VisualColor GetHairColor() => HairColor switch
        {
            HairColorType.JetBlack => VisualColor.JetBlackHair,
            HairColorType.DarkBrown => VisualColor.DarkBrownHair,
            HairColorType.LightBrown => VisualColor.LightBrownHair,
            HairColorType.GoldenBlonde => VisualColor.GoldenBlondeHair,
            HairColorType.AuburnRed => VisualColor.AuburnRedHair,
            HairColorType.PlatinumSilver => VisualColor.PlatinumSilverHair,
            _ => VisualColor.DarkBrownHair
        };

        /// <summary>
        /// Generates an authentic, stable visual appearance deterministically derived
        /// from the player's name and squad number.
        /// </summary>
        public static PlayerVisualProfile CreateDeterministic(string name, int squadNumber, Position position)
        {
            int hash = 17;
            unchecked
            {
                if (!string.IsNullOrEmpty(name))
                {
                    foreach (char c in name)
                    {
                        hash = hash * 31 + c;
                    }
                }
                hash = hash * 31 + squadNumber;
            }
            uint seed = (uint)Math.Abs(hash);

            var skinTones = (SkinToneType[])Enum.GetValues(typeof(SkinToneType));
            var hairStyles = (HairStyleType[])Enum.GetValues(typeof(HairStyleType));
            var hairColors = (HairColorType[])Enum.GetValues(typeof(HairColorType));
            var facialHairs = (FacialHairType[])Enum.GetValues(typeof(FacialHairType));
            var bootStyles = (BootStyleType[])Enum.GetValues(typeof(BootStyleType));

            var skin = skinTones[seed % (uint)skinTones.Length];
            var hair = hairStyles[(seed / 5) % (uint)hairStyles.Length];
            var color = hairColors[(seed / 11) % (uint)hairColors.Length];
            var facial = facialHairs[(seed / 17) % (uint)facialHairs.Length];
            var boot = bootStyles[(seed / 23) % (uint)bootStyles.Length];

            float height = position == Position.GK ? 1.90f : 1.80f + ((seed % 10) * 0.01f);

            return new PlayerVisualProfile
            {
                SkinTone = skin,
                HairStyle = hair,
                HairColor = color,
                FacialHair = facial,
                BootStyle = boot,
                HeightMeters = (float)Math.Round(height, 2)
            };
        }
    }
}
