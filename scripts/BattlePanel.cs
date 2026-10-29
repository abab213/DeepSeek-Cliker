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
        AddTexture(stage, "res://assets/art/battle_bg.svg", Vector2.Zero, new Vector2(StageWidth, StageHeight));

        AddTexture(stage, "res://assets/art/zombie_brute.svg", new Vector2(-80, 380), new Vector2(260, 380), 0.62f);
        AddTexture(stage, "res://assets/art/zombie_walker.svg", new Vector2(55, 438), new Vector2(148, 242), 1f);
        AddTexture(stage, "res://assets/art/gunner.svg", new Vector2(164, 286), new Vector2(340, 404));
        AddTexture(stage, "res://assets/art/muzzle_flash.svg", new Vector2(130, 508), new Vector2(120, 88), 0.95f);
        AddTexture(stage, "res://assets/art/battle_fg.svg", new Vector2(0, -32), new Vector2(StageWidth, StageHeight + 32));

        var fx = new BattleFx();
        fx.SetAnchorsPreset(LayoutPreset.FullRect);
        fx.OffsetRight = 0;
        fx.OffsetBottom = 0;
        stage.AddChild(fx);

        BuildScrims(stage);
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
        var top = UiKit.VerticalGradient(UiKit.Alpha(UiKit.BgTop, 0.94f), UiKit.Alpha(UiKit.BgTop, 0f));
        top.AnchorLeft = 0;
        top.AnchorTop = 0;
        top.AnchorRight = 1;
        top.AnchorBottom = 0;
        top.OffsetLeft = 0;
        top.OffsetTop = 0;
        top.OffsetRight = 0;
        top.OffsetBottom = 224;
        stage.AddChild(top);

        var bottom = UiKit.VerticalGradient(UiKit.Alpha(UiKit.BgTop, 0f), UiKit.Alpha(UiKit.BgTop, 0.96f));
        bottom.AnchorLeft = 0;
        bottom.AnchorTop = 1;
        bottom.AnchorRight = 1;
        bottom.AnchorBottom = 1;
        bottom.OffsetLeft = 0;
        bottom.OffsetTop = -300;
        bottom.OffsetRight = 0;
        bottom.OffsetBottom = 0;
        stage.AddChild(bottom);
    }

    private static void BuildTopHud(Control stage)
    {
        var hud = new MarginContainer
        {
            AnchorLeft = 0,
            AnchorTop = 0,
            AnchorRight = 1,
            AnchorBottom = 0,
            OffsetLeft = 0,
            OffsetTop = 0,
            OffsetRight = 0,
            OffsetBottom = 150,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        hud.AddThemeConstantOverride("margin_left", 15);
        hud.AddThemeConstantOverride("margin_top", 14);
        hud.AddThemeConstantOverride("margin_right", 15);
        hud.AddThemeConstantOverride("margin_bottom", 0);

        var column = UiKit.VBox(8);
        var topRow = UiKit.HBox(8);

        var live = UiKit.HBox(6);
        live.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        var dot = new Panel { CustomMinimumSize = new Vector2(7, 7), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        dot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Red, UiKit.Red, 4, 0));
        live.AddChild(dot);
        live.AddChild(UiKit.Label("LIVE", 8, UiKit.Red, UiKit.DisplayFont));
        var divider = UiKit.VLine(UiKit.Alpha(UiKit.Red, 0.3f), 1);
        divider.CustomMinimumSize = new Vector2(1, 12);
        divider.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        live.AddChild(divider);
        live.AddChild(UiKit.Label("SECTOR 07", 8, UiKit.Alpha(UiKit.Cyan, 0.8f), UiKit.DisplayFont));
        topRow.AddChild(live);

        topRow.AddChild(UiKit.HSpace(0, true));

        var waveRow = UiKit.HBox(7);
        waveRow.AddChild(UiKit.Icon("res://assets/icons/icon_skull.svg", 14, UiKit.Red));
        waveRow.AddChild(UiKit.Label("第 03 波", 11, UiKit.Text, UiKit.BoldFont));
        var waveDivider = UiKit.VLine(UiKit.Alpha(UiKit.Red, 0.25f), 1);
        waveDivider.CustomMinimumSize = new Vector2(1, 12);
        waveDivider.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        waveRow.AddChild(waveDivider);
        waveRow.AddChild(UiKit.Label("剩余 12", 10, UiKit.Alpha(UiKit.Red, 0.9f), UiKit.BodyFont));
        var wavePanel = UiKit.PanelBox(
            UiKit.Pad(waveRow, 9, 5, 9, 5),
            UiKit.Box(UiKit.Alpha(UiKit.Red, 0.1f), UiKit.Alpha(UiKit.Red, 0.42f), 8));
        topRow.AddChild(wavePanel);

        column.AddChild(topRow);

        var locationRow = UiKit.HBox(7);
        locationRow.AddChild(UiKit.Label("望京废墟 · 地下补给站", 10, UiKit.Alpha(UiKit.Cyan, 0.78f), UiKit.BodyFont));
        locationRow.AddChild(UiKit.HSpace(0, true));
        locationRow.AddChild(UiKit.Label("机枪手 铁盆号", 10, UiKit.Alpha(UiKit.Cyan, 0.82f), UiKit.BodyFont));
        column.AddChild(locationRow);

        hud.AddChild(column);
        stage.AddChild(hud);
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
            OffsetTop = -178,
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
        return UiKit.Pad(root, 14, 11, 14, 11);
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
        var muzzle = new Vector2(197, 542);
        var target = new Vector2(128, 566);
        var secondary = new Vector2(92, 532);
        float flicker = 0.55f + 0.45f * Mathf.Sin(_time * 23f);

        DrawLine(muzzle, target, UiKit.Alpha(UiKit.Gold, 0.2f), 7f, true);
        DrawLine(muzzle, target, UiKit.Alpha(UiKit.Cyan, 0.75f), 2f, true);
        DrawLine(muzzle, secondary, UiKit.Alpha(UiKit.Gold, 0.18f), 3f, true);
        DrawLine(muzzle + new Vector2(0, 2), target + new Vector2(0, -4), UiKit.Alpha(UiKit.White, 0.7f * flicker), 1f, true);

        DrawCircle(target, 3.5f, UiKit.Alpha(UiKit.Gold, 0.55f));
        DrawLine(target + new Vector2(-7, -7), target + new Vector2(-3, -3), UiKit.Alpha(UiKit.Gold, 0.8f), 1.5f, true);
        DrawLine(target + new Vector2(7, -7), target + new Vector2(3, -3), UiKit.Alpha(UiKit.Gold, 0.8f), 1.5f, true);
        DrawLine(target + new Vector2(-7, 7), target + new Vector2(-3, 3), UiKit.Alpha(UiKit.Gold, 0.8f), 1.5f, true);
        DrawLine(target + new Vector2(7, 7), target + new Vector2(3, 3), UiKit.Alpha(UiKit.Gold, 0.8f), 1.5f, true);

        var head = new Vector2(146, 503);
        DrawArc(head, 24, -0.7f, 0.7f, 16, UiKit.Alpha(UiKit.Cyan, 0.35f), 1.2f, true);
        DrawArc(head, 24, Mathf.Pi - 0.7f, Mathf.Pi + 0.7f, 16, UiKit.Alpha(UiKit.Cyan, 0.35f), 1.2f, true);
        DrawLine(head + new Vector2(0, -34), head + new Vector2(0, -26), UiKit.Alpha(UiKit.Cyan, 0.45f), 1.2f, true);
        DrawLine(head + new Vector2(0, 26), head + new Vector2(0, 34), UiKit.Alpha(UiKit.Cyan, 0.45f), 1.2f, true);

        var rng = new RandomNumberGenerator { Seed = 77 };
        for (int i = 0; i < 16; i++)
        {
            float x = rng.RandfRange(20, 480);
            float baseY = rng.RandfRange(300, 700);
            float y = baseY - (_time * rng.RandfRange(3f, 9f) + i * 12f) % 80f;
            DrawCircle(new Vector2(x, y), rng.RandfRange(0.6f, 1.4f), UiKit.Alpha(UiKit.Cyan, 0.12f));
        }

        float warn = 0.5f + 0.5f * Mathf.Sin(_time * 3f);
        DrawLine(new Vector2(12, 360), new Vector2(12, 420), UiKit.Alpha(UiKit.Red, 0.18f + warn * 0.12f), 2f, true);
    }
}
