using Godot;

public partial class Backdrop : Control
{
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public override void _Draw()
    {
        var size = Size;
        if (size.X <= 0 || size.Y <= 0)
            return;

        const int bands = 72;
        for (int i = 0; i < bands; i++)
        {
            float t = i / (float)(bands - 1);
            var color = UiKit.BgTop.Lerp(UiKit.BgBottom, t);
            DrawRect(new Rect2(0, size.Y * i / bands, size.X, size.Y / bands + 1), color);
        }

        DrawGlow(new Vector2(size.X * 0.55f, size.Y * 0.46f), 430, UiKit.Cyan, 0.052f);
        DrawGlow(new Vector2(size.X * 0.72f, size.Y * 0.82f), 320, UiKit.Gold, 0.024f);
        DrawGlow(new Vector2(size.X * 0.1f, size.Y * 0.15f), 260, UiKit.Warm, 0.025f);

        var gridColor = UiKit.Alpha(UiKit.Cyan, 0.022f);
        for (float x = 0; x <= size.X; x += 80)
            DrawLine(new Vector2(x, 0), new Vector2(x, size.Y), gridColor, 1);
        for (float y = 0; y <= size.Y; y += 80)
            DrawLine(new Vector2(0, y), new Vector2(size.X, y), gridColor, 1);

        DrawLine(new Vector2(0, size.Y * 0.735f), new Vector2(size.X, size.Y * 0.735f), UiKit.Alpha(UiKit.Cyan, 0.05f), 1);

        var ghost = UiKit.DisplayFont;
        DrawString(
            ghost,
            new Vector2(size.X * 0.025f, size.Y * 0.955f),
            "2077",
            HorizontalAlignment.Left,
            -1,
            250,
            UiKit.Alpha(UiKit.Cyan, 0.028f));

        var rng = new RandomNumberGenerator { Seed = 2077 };
        for (int i = 0; i < 90; i++)
        {
            float x = rng.RandfRange(0, size.X);
            float y = rng.RandfRange(0, size.Y);
            float radius = rng.RandfRange(0.6f, 1.8f);
            DrawCircle(new Vector2(x, y), radius, UiKit.Alpha(UiKit.Cyan, rng.RandfRange(0.02f, 0.075f)));
        }

        DrawLine(new Vector2(28, 108), new Vector2(size.X - 28, 108), UiKit.Alpha(UiKit.Cyan, 0.07f), 1);
        DrawLine(new Vector2(28, size.Y - 24), new Vector2(size.X - 28, size.Y - 24), UiKit.Alpha(UiKit.Cyan, 0.04f), 1);
    }

    private void DrawGlow(Vector2 center, float radius, Color color, float strength)
    {
        for (int i = 0; i < 30; i++)
        {
            float t = i / 29f;
            float r = radius * (1f - t * 0.82f);
            DrawCircle(center, r, UiKit.Alpha(color, strength * t * t));
        }
    }
}
