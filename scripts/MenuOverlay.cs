using Godot;

public partial class MenuOverlay : Control
{
    private readonly GameState _state;
    private Control _pausePanel = null!;
    private Control _settingsPanel = null!;
    private Label _pauseStats = null!;
    private Label _pauseStatus = null!;
    private Label _settingsStatus = null!;
    private Button _autoButton = null!;
    private Button _effectsButton = null!;
    private Button _audioButton = null!;
    private Button _musicButton = null!;
    private Button _clearButton = null!;
    private bool _confirmClear;

    public MenuOverlay(GameState state)
    {
        _state = state;
        Visible = false;
        MouseFilter = MouseFilterEnum.Stop;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var dim = new ColorRect
        {
            Color = new Color(0.01f, 0.02f, 0.04f, 0.8f),
            MouseFilter = MouseFilterEnum.Stop,
        };
        dim.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(dim);

        var center = new CenterContainer();
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(center);

        _pausePanel = BuildPausePanel();
        _settingsPanel = BuildSettingsPanel();
        _settingsPanel.Visible = false;
        center.AddChild(_pausePanel);
        center.AddChild(_settingsPanel);
    }

    public void OpenPause()
    {
        _state.Pause();
        Visible = true;
        _pausePanel.Visible = true;
        _settingsPanel.Visible = false;
        _confirmClear = false;
        _clearButton.Text = "清除存档并重开";
        RefreshPauseStats();
        _pauseStatus.Text = "战场已冻结，进度不会继续推进";
        _pauseStatus.AddThemeColorOverride("font_color", UiKit.Faint);
    }

    public void OpenSettings()
    {
        _state.Pause();
        Visible = true;
        _pausePanel.Visible = false;
        _settingsPanel.Visible = true;
        _confirmClear = false;
        _clearButton.Text = "清除存档并重开";
        RefreshSettings();
        _settingsStatus.Text = "设置随存档一起保存";
        _settingsStatus.AddThemeColorOverride("font_color", UiKit.Faint);
    }

    public void Resume()
    {
        Visible = false;
        _state.Resume();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible || @event is not InputEventKey key || !key.Pressed || key.Echo)
            return;

        if (key.Keycode == Key.Escape)
        {
            if (_settingsPanel.Visible)
                ShowPause();
            else
                Resume();
            AcceptEvent();
        }
    }

    private Control BuildPausePanel()
    {
        var box = UiKit.VBox(10);

        var titleRow = UiKit.HBox(9);
        titleRow.AddChild(UiKit.Label("已暂停", 26, UiKit.Text, UiKit.BoldFont));
        titleRow.AddChild(UiKit.Tag("PAUSED", UiKit.Gold, 8));
        box.AddChild(titleRow);
        box.AddChild(UiKit.Label("P A U S E D  //  战 场 冻 结", 8, UiKit.Alpha(UiKit.Cyan, 0.6f), UiKit.DisplayFont));

        var accent = UiKit.HBox(4);
        accent.AddChild(new ColorRect { Color = UiKit.Cyan, CustomMinimumSize = new Vector2(30, 2) });
        accent.AddChild(new ColorRect { Color = UiKit.Alpha(UiKit.Gold, 0.7f), CustomMinimumSize = new Vector2(10, 2) });
        box.AddChild(UiKit.VSpace(4));
        box.AddChild(accent);

        _pauseStats = UiKit.Label("", 10, UiKit.Muted, UiKit.BodyFont);
        box.AddChild(_pauseStats);
        box.AddChild(UiKit.VSpace(6));

        box.AddChild(PrimaryButton("继续游戏", () => Resume()));
        box.AddChild(SecondaryButton("保存进度", () => SaveNow()));
        box.AddChild(SecondaryButton("设置", () => ShowSettings()));

        _pauseStatus = UiKit.Label("", 9, UiKit.Faint, UiKit.BodyFont);
        box.AddChild(_pauseStatus);

        return UiKit.PanelBox(
            UiKit.Pad(box, 32, 26, 32, 24),
            UiKit.Box(UiKit.Alpha(UiKit.BgTop, 0.97f), UiKit.Alpha(UiKit.Cyan, 0.45f), 16, 1, 18, new Color(0, 0, 0, 0.6f)));
    }

    private Control BuildSettingsPanel()
    {
        var box = UiKit.VBox(10);

        var titleRow = UiKit.HBox(9);
        titleRow.AddChild(UiKit.Label("设置", 26, UiKit.Text, UiKit.BoldFont));
        titleRow.AddChild(UiKit.Tag("SETTINGS", UiKit.Cyan, 8));
        box.AddChild(titleRow);
        box.AddChild(UiKit.Label("S E T T I N G S  //  营 地 配 置", 8, UiKit.Alpha(UiKit.Cyan, 0.6f), UiKit.DisplayFont));
        box.AddChild(UiKit.VSpace(4));

        box.AddChild(SettingRow("自动保存", "每 30 秒自动写入进度", out _autoButton, ToggleAutoSave));
        box.AddChild(SettingRow("战斗特效", "曳光弹、粒子和命中闪光", out _effectsButton, ToggleEffects));
        box.AddChild(SettingRow("音效", "点击、射击、购买与警报", out _audioButton, ToggleAudio));
        box.AddChild(SettingRow("背景音乐", "废土氛围循环 BGM", out _musicButton, ToggleMusic));

        box.AddChild(UiKit.HLine(UiKit.Alpha(UiKit.Cyan, 0.1f), 1));
        box.AddChild(SecondaryButton("保存进度", () => SaveNow()));
        _clearButton = SecondaryButton("清除存档并重开", ClearAndRestart);
        box.AddChild(_clearButton);
        box.AddChild(SecondaryButton("返回", () => ShowPause()));

        _settingsStatus = UiKit.Label("", 9, UiKit.Faint, UiKit.BodyFont);
        box.AddChild(_settingsStatus);

        return UiKit.PanelBox(
            UiKit.Pad(box, 32, 26, 32, 24),
            UiKit.Box(UiKit.Alpha(UiKit.BgTop, 0.97f), UiKit.Alpha(UiKit.Cyan, 0.45f), 16, 1, 18, new Color(0, 0, 0, 0.6f)));
    }

    private Control SettingRow(string title, string hint, out Button toggle, System.Action onToggle)
    {
        var row = UiKit.HBox(10);
        row.CustomMinimumSize = new Vector2(340, 34);
        var text = UiKit.VBox(1);
        text.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        text.AddChild(UiKit.Label(title, 11, UiKit.Text, UiKit.BodyFont));
        text.AddChild(UiKit.Label(hint, 8, UiKit.Faint, UiKit.BodyFont));
        row.AddChild(text);

        toggle = new Button
        {
            CustomMinimumSize = new Vector2(64, 26),
            FocusMode = FocusModeEnum.None,
            MouseDefaultCursorShape = CursorShape.PointingHand,
        };
        toggle.AddThemeFontOverride("font", UiKit.BoldFont);
        toggle.AddThemeFontSizeOverride("font_size", 9);
        toggle.Pressed += onToggle;
        row.AddChild(toggle);
        return row;
    }

    private void RefreshPauseStats()
    {
        _pauseStats.Text = $"第 {_state.Wave:00} 波 · 击杀 {_state.Kills} · 积分 {_state.Score:N0} · 白饭 {_state.Rice:N0}";
    }

    private void RefreshSettings()
    {
        ApplyToggle(_autoButton, _state.AutoSave);
        ApplyToggle(_effectsButton, _state.EffectsEnabled);
        ApplyToggle(_audioButton, _state.AudioEnabled);
        ApplyToggle(_musicButton, _state.MusicEnabled);
    }

    private void ToggleAutoSave()
    {
        _state.AutoSave = !_state.AutoSave;
        RefreshSettings();
        _settingsStatus.Text = _state.AutoSave ? "自动保存已开启" : "自动保存已关闭";
        _settingsStatus.AddThemeColorOverride("font_color", UiKit.Alpha(UiKit.Green, 0.85f));
    }

    private void ToggleEffects()
    {
        _state.EffectsEnabled = !_state.EffectsEnabled;
        RefreshSettings();
        _settingsStatus.Text = _state.EffectsEnabled ? "战斗特效已开启" : "战斗特效已关闭";
        _settingsStatus.AddThemeColorOverride("font_color", UiKit.Alpha(UiKit.Green, 0.85f));
    }

    private void ToggleAudio()
    {
        _state.AudioEnabled = !_state.AudioEnabled;
        AudioManager.SetEnabled(_state.AudioEnabled);
        RefreshSettings();
        _settingsStatus.Text = _state.AudioEnabled ? "音效已开启" : "音效已关闭";
        _settingsStatus.AddThemeColorOverride("font_color", UiKit.Alpha(UiKit.Green, 0.85f));
    }

    private void ToggleMusic()
    {
        _state.MusicEnabled = !_state.MusicEnabled;
        AudioManager.SetMusicEnabled(_state.MusicEnabled);
        RefreshSettings();
        _settingsStatus.Text = _state.MusicEnabled ? "背景音乐已开启" : "背景音乐已关闭";
        _settingsStatus.AddThemeColorOverride("font_color", UiKit.Alpha(UiKit.Green, 0.85f));
    }

    private void SaveNow()
    {
        if (_state.GameOver)
        {
            SetStatus("防线失守，当前进度不可保存", false);
            return;
        }

        var ok = SaveService.Save(_state);
        SetStatus(ok
            ? $"已保存 · 第 {_state.Wave:00} 波 · 白饭 {_state.Rice:N0}"
            : $"保存失败：{SaveService.LastError}", ok);
    }

    private void ClearAndRestart()
    {
        if (!_confirmClear)
        {
            _confirmClear = true;
            _clearButton.Text = "再次点击确认清除";
            _settingsStatus.Text = "将删除本地存档并重新开始";
            _settingsStatus.AddThemeColorOverride("font_color", UiKit.Alpha(UiKit.Red, 0.85f));
            return;
        }

        SaveService.Clear();
        if (SaveService.HasSave())
        {
            SetStatus($"清除失败：{SaveService.LastError}", false);
            return;
        }

        GetTree().ReloadCurrentScene();
    }

    private void ShowPause()
    {
        _pausePanel.Visible = true;
        _settingsPanel.Visible = false;
        RefreshPauseStats();
    }

    private void ShowSettings()
    {
        _pausePanel.Visible = false;
        _settingsPanel.Visible = true;
        _confirmClear = false;
        _clearButton.Text = "清除存档并重开";
        RefreshSettings();
        _settingsStatus.Text = "设置随存档一起保存";
        _settingsStatus.AddThemeColorOverride("font_color", UiKit.Faint);
    }

    private void SetStatus(string message, bool ok)
    {
        var status = _settingsPanel.Visible ? _settingsStatus : _pauseStatus;
        status.Text = message;
        status.AddThemeColorOverride("font_color", ok ? UiKit.Alpha(UiKit.Green, 0.9f) : UiKit.Alpha(UiKit.Red, 0.85f));
    }

    private static void ApplyToggle(Button button, bool enabled)
    {
        button.Text = enabled ? "开" : "关";
        button.AddThemeStyleboxOverride("normal", UiKit.Box(
            enabled ? UiKit.Alpha(UiKit.Green, 0.12f) : UiKit.PanelDeep,
            enabled ? UiKit.Alpha(UiKit.Green, 0.5f) : UiKit.LineSoft, 7));
        button.AddThemeStyleboxOverride("hover", UiKit.Box(
            enabled ? UiKit.Alpha(UiKit.Green, 0.18f) : UiKit.PanelRaised,
            enabled ? UiKit.Green : UiKit.Alpha(UiKit.Cyan, 0.4f), 7));
        button.AddThemeStyleboxOverride("pressed", UiKit.Box(
            enabled ? UiKit.Alpha(UiKit.Green, 0.2f) : UiKit.PanelDeep,
            enabled ? UiKit.Green : UiKit.Cyan, 7));
        button.AddThemeColorOverride("font_color", enabled ? UiKit.Green : UiKit.Muted);
        button.AddThemeColorOverride("font_hover_color", enabled ? UiKit.Green : UiKit.Cyan);
        button.AddThemeColorOverride("font_pressed_color", enabled ? UiKit.Green : UiKit.Cyan);
    }

    private static Button PrimaryButton(string text, System.Action action)
    {
        var button = UiKit.Button(text, UiKit.Alpha(UiKit.Gold, 0.94f), UiKit.Gold, new Color("241a07"), 11);
        button.CustomMinimumSize = new Vector2(300, 38);
        button.Pressed += action;
        return button;
    }

    private static Button SecondaryButton(string text, System.Action action)
    {
        var button = UiKit.Button(text, UiKit.PanelDeep, UiKit.LineSoft, UiKit.Muted, 10);
        button.CustomMinimumSize = new Vector2(300, 34);
        button.AddThemeColorOverride("font_hover_color", UiKit.Cyan);
        button.Pressed += action;
        return button;
    }
}
