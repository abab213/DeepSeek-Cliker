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
        AddTexture(stage, "res://assets/art/characters/iron_basin_hero_full.jpg", new Vector2(0, 40), new Vector2(StageWidth, StageHeight));

        var vignette = UiKit.HorizontalGradient(UiKit.Alpha(UiKit.BgTop, 0.82f), UiKit.Alpha(UiKit.BgTop, 0f));
        vignette.Position = Vector2.Zero;
        vignette.Size = new Vector2(310, StageHeight);
        stage.AddChild(vignette);

        AddTexture(stage, "res://assets/art/lane_holo.svg", Vector2.Zero, new Vector2(StageWidth, StageHeight));

        AddTexture(stage, "res://assets/art/zombie_front_walker.svg", new Vector2(210, 226), new Vector2(58, 92), 0.56f);
        AddTexture(stage, "res://assets/art/zombie_front_runner.svg", new Vector2(142, 254), new Vector2(52, 86), 0.68f);
        AddTexture(stage, "res://assets/art/zombie_front_runner.svg", new Vector2(108, 338), new Vector2(104, 172), 0.94f);
        AddTexture(stage, "res://assets/art/zombie_front_brute.svg", new Vector2(198, 468), new Vector2(150, 220), 1f);

        BuildScrims(stage);

        AddTexture(stage, "res://assets/art/defense_gun.svg", new Vector2(-120, 45), new Vector2(StageWidth, StageHeight));
        AddTexture(stage, "res://assets/art/muzzle_flash.svg", new Vector2(68, 485), new Vector2(124, 92), 0.96f);

        var fx = new BattleFx();
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
        topRow.AddChild(UiKit.Label("AUTO", 7, UiKit.Alpha(UiKit.Green, 0.72f), UiKit.DisplayFont));

        var location = UiKit.HBox(5);
        location.AddChild(UiKit.Label("望京高架 · 防线 03", 10, UiKit.Alpha(UiKit.Cyan, 0.8f), UiKit.BodyFont));
        location.AddChild(UiKit.HSpace(0, true));
        location.AddChild(UiKit.Label("铁盆号", 9, UiKit.Alpha(UiKit.Text, 0.7f), UiKit.BodyFont));

        var waveRow = UiKit.HBox(6);
        waveRow.AddChild(UiKit.Icon("res://assets/icons/icon_skull.svg", 13, UiKit.Red));
        waveRow.AddChild(UiKit.Label("第 03 波", 10, UiKit.Text, UiKit.BoldFont));
        waveRow.AddChild(UiKit.HSpace(0, true));
        waveRow.AddChild(UiKit.Label("剩余 12", 9, UiKit.Alpha(UiKit.Red, 0.9f), UiKit.BodyFont));

        var spawnRow = UiKit.HBox(6);
        spawnRow.AddChild(UiKit.Label("▼ ▼ ▼", 7, UiKit.Alpha(UiKit.Red, 0.9f), UiKit.DisplayFont));
        spawnRow.AddChild(UiKit.Label("尸潮入口", 9, UiKit.Red, UiKit.BoldFont));
        spawnRow.AddChild(UiKit.HSpace(0, true));
        spawnRow.AddChild(UiKit.Label("S P A W N", 7, UiKit.Alpha(UiKit.Red, 0.68f), UiKit.DisplayFont));

        column.AddChild(topRow);
        column.AddChild(location);
        column.AddChild(UiKit.PanelBox(
            UiKit.Pad(waveRow, 8, 4, 8, 4),
            UiKit.Box(UiKit.Alpha(UiKit.Red, 0.08f), UiKit.Alpha(UiKit.Red, 0.34f), 6)));
        column.AddChild(UiKit.PanelBox(
            UiKit.Pad(spawnRow, 8, 3, 8, 3),
            UiKit.Box(new Color(0, 0, 0, 0), UiKit.Alpha(UiKit.Red, 0.2f), 6)));

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
        auto.AddChild(UiKit.Icon("res://assets/icons/icon_fire.svg", 12, UiKit.Warm));
        auto.AddChild(UiKit.Label("自动射击", 10, UiKit.Alpha(UiKit.Warm, 0.92f), UiKit.BodyFont));
        bottom.AddChild(auto);

        bottom.AddChild(UiKit.HSpace(0, true));

        var threat = UiKit.HBox(5);
        var threatDot = new Panel { CustomMinimumSize = new Vector2(5, 5), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        threatDot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Red, UiKit.Red, 3, 0));
        threat.AddChild(threatDot);
        threat.AddChild(UiKit.Label("威胁等级 中", 10, UiKit.Muted, UiKit.BodyFont));
        bottom.AddChild(threat);

        root.AddChild(bottom);
        return UiKit.Pad(root, 14, 10, 14, 10);
    }
}

public partial class BattleFx : Control
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
        var muzzle = new Vector2(130, 533);
        var nearTarget = new Vector2(274, 535);
        var midTarget = new Vector2(160, 425);
        var farTarget = new Vector2(238, 300);
        float flicker = 0.55f + 0.45f * Mathf.Sin(_time * 24f);

        DrawCircle(muzzle, 36, UiKit.Alpha(UiKit.Cyan, 0.06f));
        DrawLine(muzzle, nearTarget, UiKit.Alpha(UiKit.Gold, 0.2f), 7f, true);
        DrawLine(muzzle, nearTarget, UiKit.Alpha(UiKit.Cyan, 0.82f), 2f, true);
        DrawLine(muzzle, midTarget, UiKit.Alpha(UiKit.Gold, 0.16f), 4f, true);
        DrawLine(muzzle, farTarget, UiKit.Alpha(UiKit.Gold, 0.1f), 2f, true);
        DrawLine(muzzle + new Vector2(-2, 0), nearTarget + new Vector2(0, -4), UiKit.Alpha(UiKit.White, 0.75f * flicker), 1f, true);
        DrawLine(muzzle, midTarget, UiKit.Alpha(UiKit.White, 0.3f * flicker), 1f, true);

        DrawImpact(nearTarget, 1f);
        DrawImpact(midTarget, 0.7f);
        DrawImpact(farTarget, 0.4f);

        var head = new Vector2(274, 495);
        DrawArc(head, 30, -0.9f, 0.9f, 18, UiKit.Alpha(UiKit.Cyan, 0.4f), 1.3f, true);
        DrawArc(head, 30, Mathf.Pi - 0.9f, Mathf.Pi + 0.9f, 18, UiKit.Alpha(UiKit.Cyan, 0.4f), 1.3f, true);
        DrawLine(head + new Vector2(0, -44), head + new Vector2(0, -34), UiKit.Alpha(UiKit.Cyan, 0.5f), 1.2f, true);
        DrawLine(head + new Vector2(0, 34), head + new Vector2(0, 44), UiKit.Alpha(UiKit.Cyan, 0.5f), 1.2f, true);
        DrawLine(head + new Vector2(-44, 0), head + new Vector2(-34, 0), UiKit.Alpha(UiKit.Cyan, 0.5f), 1.2f, true);
        DrawLine(head + new Vector2(34, 0), head + new Vector2(44, 0), UiKit.Alpha(UiKit.Cyan, 0.5f), 1.2f, true);

        var rng = new RandomNumberGenerator { Seed = 2077 };
        for (int i = 0; i < 24; i++)
        {
            float x = rng.RandfRange(16, 484);
            float baseY = rng.RandfRange(90, 640);
            float speed = rng.RandfRange(26f, 52f);
            float y = ((baseY + _time * speed + i * 37f) % 580f) + 60f;
            float len = rng.RandfRange(10f, 26f);
            DrawLine(new Vector2(x, y), new Vector2(x - len * 0.28f, y + len), UiKit.Alpha(UiKit.Cyan, 0.12f), 1f, true);
        }

        float warn = 0.5f + 0.5f * Mathf.Sin(_time * 2.8f);
        DrawLine(new Vector2(154, 186), new Vector2(302, 186), UiKit.Alpha(UiKit.Red, 0.12f + warn * 0.1f), 2f, true);
        DrawLine(new Vector2(176, 198), new Vector2(280, 198), UiKit.Alpha(UiKit.Red, 0.06f + warn * 0.06f), 1f, true);

        DrawLine(new Vector2(14, 604), new Vector2(330, 604), UiKit.Alpha(UiKit.Cyan, 0.1f + warn * 0.06f), 1f, true);
        DrawLine(new Vector2(14, 610), new Vector2(330, 610), UiKit.Alpha(UiKit.Cyan, 0.04f), 1f, true);
    }

    private void DrawImpact(Vector2 point, float strength)
    {
        DrawCircle(point, 4.5f * strength, UiKit.Alpha(UiKit.Gold, 0.5f * strength));
        for (int i = 0; i < 4; i++)
        {
            float angle = i * Mathf.Pi * 0.5f + 0.35f;
            var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            DrawLine(point, point + dir * (9f + 4f * strength), UiKit.Alpha(UiKit.Gold, 0.7f * strength), 1.2f, true);
        }
    }
}
