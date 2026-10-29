using Godot;

public partial class BattlePanel : PanelContainer
{
    private const float StageWidth = 500f;
    private const float StageHeight = 752f;

    private readonly GameState _state;
    private CombatField _combat = null!;
    private Label _statusTop = null!;
    private Label _waveLabel = null!;
    private Label _nextLabel = null!;
    private Label _spawnLabel = null!;
    private Label _spawnEn = null!;
    private Control _gameOver = null!;
    private Label _gameOverStats = null!;

    public BattlePanel(GameState state)
    {
        _state = state;
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
        _state.Changed += RefreshHud;
        _state.GameEnded += ShowGameOver;
        RefreshHud();
    }

    private void BuildStage(Control stage)
    {
        AddTexture(stage, "res://assets/art/road_battlefield.svg", Vector2.Zero, new Vector2(StageWidth, StageHeight));

        AddTexture(stage, "res://assets/art/mounted_gun.svg", new Vector2(195, 548), new Vector2(100, 155));
        AddTexture(stage, "res://assets/art/characters/fish_head.webp", new Vector2(209, 640), new Vector2(82, 96));
        AddTexture(stage, "res://assets/art/characters/fish_tail.svg", new Vector2(298, 688), new Vector2(52, 44));

        _combat = new CombatField(_state);
        _combat.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        stage.AddChild(_combat);

        var fx = new StreetFx();
        fx.SetAnchorsPreset(LayoutPreset.FullRect);
        fx.OffsetRight = 0;
        fx.OffsetBottom = 0;
        stage.AddChild(fx);

        BuildTopHud(stage);
        BuildGameOver(stage);
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

    private void BuildTopHud(Control stage)
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
        _statusTop = UiKit.Label("S T A N D B Y", 7, UiKit.Alpha(UiKit.Green, 0.7f), UiKit.DisplayFont);
        topRow.AddChild(_statusTop);

        var location = UiKit.HBox(5);
        location.AddChild(UiKit.Label("望京高架 · 防线 03", 10, UiKit.Alpha(UiKit.Cyan, 0.8f), UiKit.BodyFont));
        location.AddChild(UiKit.HSpace(0, true));
        location.AddChild(UiKit.Label("铁盆号", 9, UiKit.Alpha(UiKit.Text, 0.7f), UiKit.BodyFont));

        var waveRow = UiKit.HBox(6);
        waveRow.AddChild(UiKit.Icon("res://assets/icons/icon_skull.svg", 13, UiKit.Red));
        _waveLabel = UiKit.Label("第 01 波", 10, UiKit.Text, UiKit.BoldFont);
        waveRow.AddChild(_waveLabel);
        waveRow.AddChild(UiKit.HSpace(0, true));
        _nextLabel = UiKit.Label("下一波 00:05", 9, UiKit.Alpha(UiKit.Green, 0.9f), UiKit.BodyFont);
        waveRow.AddChild(_nextLabel);

        var spawnRow = UiKit.HBox(6);
        var calmDot = new Panel { CustomMinimumSize = new Vector2(5, 5), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        calmDot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Green, UiKit.Green, 3, 0));
        spawnRow.AddChild(calmDot);
        _spawnLabel = UiKit.Label("街道静默", 9, UiKit.Alpha(UiKit.Green, 0.9f), UiKit.BoldFont);
        spawnRow.AddChild(_spawnLabel);
        spawnRow.AddChild(UiKit.HSpace(0, true));
        _spawnEn = UiKit.Label("S I L E N T", 7, UiKit.Alpha(UiKit.Cyan, 0.55f), UiKit.DisplayFont);
        spawnRow.AddChild(_spawnEn);

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

    private void RefreshHud()
    {
        if (_waveLabel == null)
            return;

        if (_state.GameOver)
        {
            _statusTop.Text = "L O S T";
            _statusTop.AddThemeColorOverride("font_color", UiKit.Red);
            _nextLabel.Text = "—";
            _nextLabel.AddThemeColorOverride("font_color", UiKit.Red);
            _spawnLabel.Text = "防线失守";
            _spawnLabel.AddThemeColorOverride("font_color", UiKit.Red);
            _spawnEn.Text = "L I N E  L O S T";
            _spawnEn.AddThemeColorOverride("font_color", UiKit.Red);
            return;
        }

        var active = _state.WaveActive;
        _statusTop.Text = active ? "E N G A G E D" : "S T A N D B Y";
        _statusTop.AddThemeColorOverride("font_color", active ? UiKit.Red : UiKit.Alpha(UiKit.Green, 0.7f));
        _waveLabel.Text = $"第 {_state.Wave:00} 波";
        _nextLabel.Text = active ? "清剿中" : $"下一波 {_state.WaveTimer:00}";
        _nextLabel.AddThemeColorOverride("font_color", active ? UiKit.Red : UiKit.Alpha(UiKit.Green, 0.9f));
        _spawnLabel.Text = active ? "尸潮推进" : "街道静默";
        _spawnLabel.AddThemeColorOverride("font_color", active ? UiKit.Red : UiKit.Alpha(UiKit.Green, 0.9f));
        _spawnEn.Text = active ? "I N C O M I N G" : "S I L E N T";
        _spawnEn.AddThemeColorOverride("font_color", active ? UiKit.Alpha(UiKit.Red, 0.8f) : UiKit.Alpha(UiKit.Cyan, 0.55f));
    }

    private void BuildGameOver(Control stage)
    {
        var overlay = new Control
        {
            Visible = false,
            MouseFilter = MouseFilterEnum.Stop,
        };
        overlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        stage.AddChild(overlay);

        var dim = new ColorRect
        {
            Color = new Color(0.01f, 0.02f, 0.04f, 0.78f),
            MouseFilter = MouseFilterEnum.Stop,
        };
        dim.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        overlay.AddChild(dim);

        var center = new CenterContainer();
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        overlay.AddChild(center);

        var box = UiKit.VBox(9);
        box.Alignment = BoxContainer.AlignmentMode.Center;
        box.AddChild(UiKit.Label("防线失守", 28, UiKit.Red, UiKit.BoldFont));
        box.AddChild(UiKit.Label("D E F E N S E  L I N E  L O S T", 8, UiKit.Alpha(UiKit.Red, 0.65f), UiKit.DisplayFont));
        _gameOverStats = UiKit.Label("", 10, UiKit.Muted, UiKit.BodyFont);
        box.AddChild(_gameOverStats);

        var restart = UiKit.Button("重新部署", UiKit.Red, UiKit.Alpha(UiKit.Red, 0.7f), UiKit.Text, 10);
        restart.CustomMinimumSize = new Vector2(140, 34);
        restart.Pressed += () => GetTree().ReloadCurrentScene();
        box.AddChild(restart);

        var panel = UiKit.PanelBox(
            UiKit.Pad(box, 28, 22, 28, 22),
            UiKit.Box(UiKit.Alpha(UiKit.BgTop, 0.94f), UiKit.Alpha(UiKit.Red, 0.5f), 14, 1, 12, new Color(0, 0, 0, 0.5f)));
        center.AddChild(panel);
        _gameOver = overlay;
    }

    private void ShowGameOver()
    {
        if (_gameOver == null)
            return;
        _gameOverStats.Text = $"第 {_state.Wave:00} 波 · 击杀 {_state.Kills} · 积分 {_state.Score:N0}";
        _gameOver.Visible = true;
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
