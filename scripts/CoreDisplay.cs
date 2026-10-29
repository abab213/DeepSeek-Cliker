using Godot;
using System;

public partial class CoreDisplay : Control
{
    private float _time;
    private float _pulse;
    private Texture2D? _whale;

    public event Action? Clicked;

    public CoreDisplay()
    {
        CustomMinimumSize = new Vector2(320, 320);
        MouseFilter = MouseFilterEnum.Stop;
        MouseDefaultCursorShape = CursorShape.PointingHand;
    }

    public void Pulse()
    {
        _pulse = 1f;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse &&
            mouse.ButtonIndex == MouseButton.Left &&
            mouse.Pressed)
        {
            Clicked?.Invoke();
            AcceptEvent();
        }
    }

    public override void _Ready()
    {
        _whale = GD.Load<Texture2D>("res://assets/art/deepseek_whale.svg");
    }

    public override void _Process(double delta)
    {
        _time += (float)delta;
        _pulse = Mathf.MoveToward(_pulse, 0f, (float)delta * 4.5f);
        QueueRedraw();
    }

    public override void _Draw()
    {
        var center = Size * 0.5f;
        float maxRadius = Mathf.Min(Size.X, Size.Y) * 0.5f - 8;
        float pulse = 0.5f + 0.5f * Mathf.Sin(_time * 1.6f);

        for (int i = 0; i < 34; i++)
        {
            float t = i / 33f;
            float radius = maxRadius * (0.65f + t * 0.55f);
            float alpha = 0.004f + (1f - t) * 0.010f + pulse * 0.002f;
            DrawCircle(center, radius, UiKit.Alpha(UiKit.Cyan, alpha));
        }

        float outer = maxRadius * 0.94f;
        DrawArc(center, outer, 0, Mathf.Tau, 128, UiKit.Alpha(UiKit.Cyan, 0.24f), 1.4f, true);
        DrawArc(center, outer - 9, 0, Mathf.Tau, 128, UiKit.Alpha(UiKit.Cyan, 0.08f), 1f, true);

        for (int i = 0; i < 72; i++)
        {
            float angle = i / 72f * Mathf.Tau;
            bool major = i % 6 == 0;
            float inner = outer - (major ? 12 : 5);
            var a = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * outer;
            var b = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * inner;
            DrawLine(a, b, UiKit.Alpha(major ? UiKit.Gold : UiKit.Cyan, major ? 0.55f : 0.22f), major ? 2f : 1f, true);
        }

        float dashRadius = outer - 30;
        for (int k = 0; k < 3; k++)
        {
            float start = _time * 0.18f + k * Mathf.Tau / 3f;
            DrawArc(center, dashRadius, start, start + 0.62f, 22, UiKit.Alpha(UiKit.Cyan, 0.62f), 2.2f, true);
            DrawArc(center, dashRadius, start + 1.15f, start + 1.45f, 12, UiKit.Alpha(UiKit.Gold, 0.85f), 2.4f, true);
            var dot = center + new Vector2(Mathf.Cos(start), Mathf.Sin(start)) * (dashRadius + 10);
            DrawCircle(dot, 2.4f, UiKit.Alpha(UiKit.Gold, 0.85f));
        }

        float glowRadius = outer - 55;
        for (int i = 0; i < 24; i++)
        {
            float t = i / 23f;
            float radius = glowRadius * (1f - t * 0.25f);
            DrawCircle(center, radius, UiKit.Alpha(UiKit.Cyan, 0.005f + t * 0.012f));
        }

        float discRadius = outer - 62;
        for (int i = 0; i < 56; i++)
        {
            float t = i / 55f;
            float radius = discRadius * (1f - t * 0.96f);
            var color = new Color("071322").Lerp(new Color("102b4b"), 1f - t);
            DrawCircle(center, radius, color);
        }

        DrawArc(center, discRadius, 0, Mathf.Tau, 128, UiKit.Alpha(UiKit.Cyan, 0.62f), 2.2f, true);
        DrawArc(center, discRadius - 7, 0, Mathf.Tau, 128, UiKit.Alpha(UiKit.Gold, 0.18f), 1f, true);
        DrawArc(center, discRadius + 3, -2.6f, -0.5f, 40, UiKit.Alpha(UiKit.Cyan, 0.8f), 4f, true);

        if (_pulse > 0)
        {
            float t = 1f - _pulse;
            float ring = discRadius * (0.98f + t * 0.28f);
            DrawArc(center, ring, 0, Mathf.Tau, 96, UiKit.Alpha(UiKit.Cyan, _pulse * 0.55f), 3f, true);
            DrawArc(center, ring - 6, 0, Mathf.Tau, 96, UiKit.Alpha(UiKit.Gold, _pulse * 0.28f), 1.5f, true);
        }

        var sheen = new Vector2(center.X - discRadius * 0.2f, center.Y - discRadius * 0.35f);
        for (int i = 0; i < 16; i++)
        {
            float t = i / 15f;
            DrawCircle(sheen, discRadius * (0.45f - t * 0.02f), UiKit.Alpha(UiKit.Cyan, 0.0035f));
        }

        if (_whale != null)
        {
            float whaleSize = 162 + pulse * 4;
            var rect = new Rect2(center.X - whaleSize * 0.5f, center.Y - whaleSize * 0.56f, whaleSize, whaleSize);
            DrawTextureRect(_whale, rect, false, Colors.White);
        }

        string title = "D E E P S E E K";
        var textSize = UiKit.DisplayFont.GetStringSize(title, HorizontalAlignment.Left, -1, 12);
        DrawString(
            UiKit.DisplayFont,
            new Vector2(center.X - textSize.X * 0.5f, center.Y + discRadius * 0.62f),
            title,
            HorizontalAlignment.Left,
            -1,
            12,
            UiKit.Alpha(UiKit.Cyan, 0.78f));

        DrawLine(
            new Vector2(center.X - 52, center.Y + discRadius * 0.7f),
            new Vector2(center.X + 52, center.Y + discRadius * 0.7f),
            UiKit.Alpha(UiKit.Cyan, 0.18f),
            1f,
            true);

        for (int i = 0; i < 4; i++)
        {
            float angle = Mathf.Pi * 0.5f * i + 0.08f;
            var p = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (outer + 8);
            DrawLine(p + new Vector2(-4, 0), p + new Vector2(4, 0), UiKit.Alpha(UiKit.Gold, 0.5f), 1f, true);
            DrawLine(p + new Vector2(0, -4), p + new Vector2(0, 4), UiKit.Alpha(UiKit.Gold, 0.5f), 1f, true);
        }
    }
}
