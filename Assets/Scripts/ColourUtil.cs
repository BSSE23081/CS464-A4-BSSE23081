using UnityEngine;

// Lecture 08, slides 11 and 22: colour is chosen by value first, and checked with a number.
public static class ColourUtil
{
    // WCAG relative luminance. Unity's Color is gamma (sRGB) space, so Color.linear does the 2.4 curve for us.
    public static float RelativeLuminance(Color c)
    {
        Color l = c.linear;
        return 0.2126f * l.r + 0.7152f * l.g + 0.0722f * l.b;
    }

    // (L_light + 0.05) / (L_dark + 0.05): 1 means identical, 21 is black on white. AA text needs 4.5.
    public static float Contrast(Color a, Color b)
    {
        float la = RelativeLuminance(a), lb = RelativeLuminance(b);
        return (Mathf.Max(la, lb) + 0.05f) / (Mathf.Min(la, lb) + 0.05f);
    }

    // Hue in DEGREES here (Unity's own HSVToRGB wants 0 to 1).
    public static Color Hsv(float hueDegrees, float saturation, float value)
    {
        return Color.HSVToRGB(Mathf.Repeat(hueDegrees, 360f) / 360f, saturation, value);
    }
}

// The game's palette: ONE base hue (258 degrees, indigo) and a TRIADIC harmony (+120, +240):
// indigo for the interface, orange-red (18) for danger, green (138) for things you want.
// The roles are separated by VALUE, so they still stand apart in grayscale:
//   dark   panel, floors, walls                  (relative luminance 0.01 to 0.05)
//   hazard orange                                 0.16
//   pickup green                                  0.38
//   light  player, text                           0.85
public static class Palette
{
    public const float BaseHue = 258f;
    public const float HazardHue = 18f;     // BaseHue + 120
    public const float PickupHue = 138f;    // BaseHue + 240

    public static readonly Color Panel = ColourUtil.Hsv(BaseHue, 0.42f, 0.15f);      // dark   #1B1626
    public static readonly Color Button = ColourUtil.Hsv(BaseHue, 0.30f, 0.82f);     // mid    #A592D1
    public static readonly Color Text = ColourUtil.Hsv(BaseHue, 0.05f, 0.97f);       // light  #EFEBF7
    public static readonly Color Hazard = ColourUtil.Hsv(HazardHue, 0.85f, 0.74f);   // accent #BD4C1C
    public static readonly Color Pickup = ColourUtil.Hsv(PickupHue, 0.50f, 0.72f);   // accent #5CB877
    public static readonly Color Player = ColourUtil.Hsv(BaseHue, 0.06f, 0.98f);     // light  #EFEBFA

    // Button states. A dark label (the panel colour) sits on all of them.
    public static readonly Color ButtonHighlighted = ColourUtil.Hsv(BaseHue, 0.20f, 0.92f);
    public static readonly Color ButtonSelected = ColourUtil.Hsv(BaseHue, 0.08f, 0.99f);
    public static readonly Color ButtonPressed = ColourUtil.Hsv(BaseHue, 0.34f, 0.72f);
    public static readonly Color ButtonDisabled = ColourUtil.Hsv(BaseHue, 0.20f, 0.35f);

    public static Color Floor(float hueDegrees) { return ColourUtil.Hsv(hueDegrees, 0.35f, 0.26f); }
    public static Color Wall(float hueDegrees) { return ColourUtil.Hsv(hueDegrees, 0.40f, 0.16f); }
}
