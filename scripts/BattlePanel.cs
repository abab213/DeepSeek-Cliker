using Godot;

public partial class BattlePanel : PanelContainer
{
    private const float StageWidth = 500f;
    private const float StageHeight = 752f;

    public BattlePanel()
    {
        CustomMinimumSize = new Vector2(StageWidth, 0);
        SizeFlagsVertical = SizeFlags.ExpandFill;
        AddThemeStyleboxOverride(
            "panel",
            UiKit.Box(UiKit.Panel, UiKit.Line, 16, 1, 16, new Color(0, 0, 0, 0.42f)));

        var stage = new Control
        {
            CustomMinimumSize = new Vector2(StageWidth - 2, StageHeight - 2),
            ClipContents = true,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(stage);
        BuildStage(stage);
    }

    private void BuildStage(Control stage)
    {
        AddTexture(stage, "res://assets/art/road_battlefield.svg", Vector2.Zero, new Vector2(StageWidth, StageHeight));

        AddTexture(stage, "res://assets/art/mounted_gun.svg", new Vector2(195, 448), new Vector2(100, 155));
        AddTexture(stage, "res://assets/art/characters/fish_head.webp", new Vector2(209, 540), new Vector2(82, 96));
        AddTexture(stage, "res://assets/art/characters/fish_tail.svg", new Vector2(298, 588), new Vector2(52, 44));

        var fx = new StreetFx();
        fx.SetAnchorsPreset(LayoutPreset.FullRect);
        fx.OffsetRight = 0;
        fx.OffsetBottom = 0;
        stage.AddChild(fx);

        BuildTopHud(stage);
        BuildBottomHud(stage);
    }

    private static void AddTexture(Control parent, string path, Vector2 position, Vector2 size, float alpha = 1f)
    {
        var texture = new TextureRect
        {
            Texture = GD.Load<Texture2D>(path),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = MouseFilterEnum.Ignore,
            Modulate = new Color(1, 1, 1, alpha),
        };
        texture.Position = position;
        texture.Size = size;
        parent.AddChild(texture);
    }

    private static void BuildScrims(Control stage)
    {
        var bottom = UiKit.VerticalGradient(UiKit.Alpha(UiKit.BgTop, 0f), UiKit.Alpha(UiKit.BgTop, 0.96f));
        bottom.AnchorLeft = 0;
        bottom.AnchorTop = 1;
        bottom.AnchorRight = 1;
        bottom.AnchorBottom = 1;
        bottom.OffsetLeft = 0;
        bottom.OffsetTop = -250;
        bottom.OffsetRight = 0;
        bottom.OffsetBottom = 0;
        stage.AddChild(bottom);
    }

    private static void BuildTopHud(Control stage)
    {
        var panel = new PanelContainer
        {
            Position = new Vector2(14, 14),
            CustomMinimumSize = new Vector2(252, 0),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        panel.AddThemeStyleboxOverride(
            "panel",
            UiKit.Box(UiKit.Alpha(UiKit.BgTop, 0.92f), UiKit.Alpha(UiKit.Cyan, 0.26f), 10, 1, 12, new Color(0, 0, 0, 0.5f)));

        var column = UiKit.VBox(6);

        var topRow = UiKit.HBox(6);
        var dot = new Panel { CustomMinimumSize = new Vector2(6, 6), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        dot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Red, UiKit.Red, 3, 0));
        topRow.AddChild(dot);
        topRow.AddChild(UiKit.Label("LIVE", 8, UiKit.Red, UiKit.DisplayFont));
        var divider = UiKit.VLine(UiKit.Alpha(UiKit.Red, 0.3f), 1);
        divider.CustomMinimumSize = new Vector2(1, 11);
        divider.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        topRow.AddChild(divider);
        topRow.AddChild(UiKit.Label("S E C T O R  0 7", 7, UiKit.Alpha(UiKit.Cyan, 0.8f), UiKit.DisplayFont));
        topRow.AddChild(UiKit.HSpace(0, true));
        topRow.AddChild(UiKit.Label("S T A N D B Y", 7, UiKit.Alpha(UiKit.Green, 0.7f), UiKit.DisplayFont));

        var location = UiKit.HBox(5);
        location.AddChild(UiKit.Label("望京高架 · 防线 03", 10, UiKit.Alpha(UiKit.Cyan, 0.8f), UiKit.BodyFont));
        location.AddChild(UiKit.HSpace(0, true));
        location.AddChild(UiKit.Label("铁盆号", 9, UiKit.Alpha(UiKit.Text, 0.7f), UiKit.BodyFont));

        var waveRow = UiKit.HBox(6);
        waveRow.AddChild(UiKit.Icon("res://assets/icons/icon_skull.svg", 13, UiKit.Red));
        waveRow.AddChild(UiKit.Label("第 03 波", 10, UiKit.Text, UiKit.BoldFont));
        waveRow.AddChild(UiKit.HSpace(0, true));
        waveRow.AddChild(UiKit.Label("下一波 00:42", 9, UiKit.Alpha(UiKit.Red, 0.9f), UiKit.BodyFont));

        var spawnRow = UiKit.HBox(6);
        var calmDot = new Panel { CustomMinimumSize = new Vector2(5, 5), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        calmDot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Green, UiKit.Green, 3, 0));
        spawnRow.AddChild(calmDot);
        spawnRow.AddChild(UiKit.Label("街道静默", 9, UiKit.Alpha(UiKit.Green, 0.9f), UiKit.BoldFont));
        spawnRow.AddChild(UiKit.HSpace(0, true));
        spawnRow.AddChild(UiKit.Label("S I L E N T", 7, UiKit.Alpha(UiKit.Cyan, 0.55f), UiKit.DisplayFont));

        column.AddChild(topRow);
        column.AddChild(location);
        column.AddChild(UiKit.PanelBox(
            UiKit.Pad(waveRow, 8, 4, 8, 4),
            UiKit.Box(UiKit.Alpha(UiKit.Red, 0.08f), UiKit.Alpha(UiKit.Red, 0.34f), 6)));
        column.AddChild(UiKit.PanelBox(
            UiKit.Pad(spawnRow, 8, 3, 8, 3),
            UiKit.Box(new Color(0, 0, 0, 0), UiKit.Alpha(UiKit.Green, 0.18f), 6)));

        panel.AddChild(UiKit.Pad(column, 11, 9, 11, 9));
        stage.AddChild(panel);
    }

    private static void BuildBottomHud(Control stage)
    {
        var hud = new MarginContainer
        {
            AnchorLeft = 0,
            AnchorTop = 1,
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = 0,
            OffsetTop = -110,
            OffsetRight = 0,
            OffsetBottom = -14,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        hud.AddThemeConstantOverride("margin_left", 14);
        hud.AddThemeConstantOverride("margin_right", 14);
        hud.AddThemeConstantOverride("margin_top", 0);
        hud.AddThemeConstantOverride("margin_bottom", 0);

        var panel = UiKit.PanelBox(
            BuildDefenseHud(),
            UiKit.Box(UiKit.Alpha(UiKit.BgTop, 0.86f), UiKit.Alpha(UiKit.Cyan, 0.2f), 12, 1, 12, new Color(0, 0, 0, 0.4f)));
        panel.SizeFlagsVertical = SizeFlags.ShrinkEnd;
        hud.AddChild(panel);
        stage.AddChild(hud);
    }

    private static Control BuildDefenseHud()
    {
        var root = UiKit.VBox(8);

        var top = UiKit.HBox(7);
        top.AddChild(UiKit.Icon("res://assets/icons/icon_shield.svg", 15, UiKit.Green));
        top.AddChild(UiKit.Label("防线耐久", 10, UiKit.Muted, UiKit.BodyFont));
        top.AddChild(UiKit.HSpace(0, true));
        top.AddChild(UiKit.Display("780 / 1000", 10, UiKit.Alpha(UiKit.Text, 0.82f)));
        top.AddChild(UiKit.Display("78%", 15, UiKit.Green));
        root.AddChild(top);

        var bar = new ProgressBar
        {
            Value = 78,
            MaxValue = 100,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(0, 7),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        bar.AddThemeStyleboxOverride("background", UiKit.Box(new Color("040a11"), new Color("040a11"), 4, 0));
        bar.AddThemeStyleboxOverride("fill", UiKit.Box(UiKit.Green, UiKit.Green, 4, 0));
        root.AddChild(bar);

        var bottom = UiKit.HBox(8);
        var kills = UiKit.HBox(5);
        kills.AddChild(UiKit.Icon("res://assets/icons/icon_skull.svg", 12, UiKit.Alpha(UiKit.Red, 0.9f)));
        kills.AddChild(UiKit.Label("击杀 42", 10, UiKit.Muted, UiKit.BodyFont));
        bottom.AddChild(kills);

        bottom.AddChild(UiKit.HSpace(0, true));

        var auto = UiKit.HBox(5);
        auto.AddChild(UiKit.Icon("res://assets/icons/icon_fire.svg", 12, UiKit.Alpha(UiKit.Green, 0.85f)));
        auto.AddChild(UiKit.Label("防线待命", 10, UiKit.Alpha(UiKit.Green, 0.9f), UiKit.BodyFont));
        bottom.AddChild(auto);

        bottom.AddChild(UiKit.HSpace(0, true));

        var threat = UiKit.HBox(5);
        var threatDot = new Panel { CustomMinimumSize = new Vector2(5, 5), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        threatDot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Green, UiKit.Green, 3, 0));
        threat.AddChild(threatDot);
        threat.AddChild(UiKit.Label("威胁等级 低", 10, UiKit.Muted, UiKit.BodyFont));
        bottom.AddChild(threat);

        root.AddChild(bottom);
        return UiKit.Pad(root, 14, 10, 14, 10);
    }
}

public partial class StreetFx : Control
{
    private float _time;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public override void _Process(double delta)
    {
        _time += (float)delta;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var rng = new RandomNumberGenerator { Seed = 2077 };
        for (int i = 0; i < 26; i++)
        {
            float x = rng.RandfRange(4, 496);
            float baseY = rng.RandfRange(80, 700);
            float speed = rng.RandfRange(22f, 46f);
            float y = ((baseY + _time * speed + i * 43f) % 600f) + 30f;
            float len = rng.RandfRange(9f, 22f);
            DrawLine(new Vector2(x, y), new Vector2(x - len * 0.24f, y + len), UiKit.Alpha(UiKit.Cyan, 0.1f), 1f, true);
        }

        var rng2 = new RandomNumberGenerator { Seed = 77 };
        for (int i = 0; i < 12; i++)
        {
            float x = rng2.RandfRange(20, 480);
            float baseY = rng2.RandfRange(120, 560);
            float y = baseY - ((_time * rng2.RandfRange(4f, 10f) + i * 17f) % 100f);
            DrawCircle(new Vector2(x, y), rng2.RandfRange(0.6f, 1.4f), UiKit.Alpha(UiKit.Warm, 0.16f));
        }

        float pulse = 0.5f + 0.5f * Mathf.Sin(_time * 1.8f);
        DrawLine(new Vector2(16, 632), new Vector2(484, 632), UiKit.Alpha(UiKit.Cyan, 0.05f + pulse * 0.03f), 1f, true);
    }
}
