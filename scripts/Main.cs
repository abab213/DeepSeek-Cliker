using Godot;
using System;

public partial class Main : Control
{
    private BattlePanel _battle = null!;
    private CorePanel _core = null!;
    private ShopPanel _shop = null!;
    private GameState _state = null!;
    private MenuOverlay _menu = null!;
    private double _autosaveTimer;

    public override void _Ready()
    {
        Theme = new Theme
        {
            DefaultFont = UiKit.BodyFont,
            DefaultFontSize = 14,
        };

        AddChild(new Backdrop());

        var frame = new MarginContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
        };
        frame.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        frame.AddThemeConstantOverride("margin_left", 28);
        frame.AddThemeConstantOverride("margin_top", 24);
        frame.AddThemeConstantOverride("margin_right", 28);
        frame.AddThemeConstantOverride("margin_bottom", 24);

        var layout = new VBoxContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
        };
        layout.AddThemeConstantOverride("separation", 16);
        frame.AddChild(layout);

        _state = new GameState();
        var save = SaveService.Load();
        if (save != null)
            _state.ApplySave(save);

        _menu = new MenuOverlay(_state);

        var top = new TopBar(_state);
        top.PausePressed += _menu.OpenPause;
        top.SettingsPressed += _menu.OpenSettings;
        layout.AddChild(top);

        var content = new HBoxContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        content.AddThemeConstantOverride("separation", 20);
        layout.AddChild(content);

        _battle = new BattlePanel(_state);
        _core = new CorePanel(_state);
        _shop = new ShopPanel(_state);

        content.AddChild(_battle);
        content.AddChild(_core);
        content.AddChild(_shop);

        AddChild(frame);
        AddChild(_menu);
        AudioManager.Init(this);
        AudioManager.SetEnabled(_state.AudioEnabled);
        AudioManager.SetMusicEnabled(_state.MusicEnabled);
        AudioManager.SetSfxVolume(_state.SfxVolume);
        AudioManager.SetMusicVolume(_state.MusicVolume);
        _state.Clicked += (_, _) => AudioManager.Play("click");
        _state.PurchaseMade += () => AudioManager.Play("buy");
        _state.TacticalUsed += () => AudioManager.Play("tactical");
        _state.GameEnded += () =>
        {
            AudioManager.StopMusic();
            AudioManager.Play("gameover");
        };

        if (HasArg("--demo"))
        {
            for (int i = 0; i < 12; i++)
                _state.Click();
            _state.TryBuy(0);
        }

        if (HasArg("--gameover"))
            _state.DamageDefense(5000);

        if (HasArg("--shop-equipment"))
            _shop.ShowCategory(ShopCategory.Equipment);
        if (HasArg("--shop-tactical"))
            _shop.ShowCategory(ShopCategory.Tactical);

        if (HasArg("--test-equipment"))
        {
            _shop.ShowCategory(ShopCategory.Equipment);
            _state.BuyEquipment(0);
            _state.BuyEquipment(3);
        }

        if (HasArg("--test-tactical"))
        {
            _shop.ShowCategory(ShopCategory.Tactical);
            _state.UseTactical(1);
            _state.UseTactical(2);
        }

        if (HasArg("--test-facilities"))
        {
            _state.BuyFacility(0);
            _state.BuyFacility(1);
        }

        if (HasArg("--test-shortcuts"))
        {
            _shop.ActivateShopShortcut(0);
            _shop.ActivateFacilityShortcut(0);
        }

        if (HasArg("--boss-test"))
            _state.SetWave(10, false, 0);

        if (HasArg("--pause"))
            _menu.OpenPause();
        if (HasArg("--settings"))
            _menu.OpenSettings();
        if (HasArg("--savetest"))
            SaveService.Save(_state);

        CaptureIfRequested();
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (_state.Paused || _state.GameOver)
            return;
        if (@event is not InputEventKey key || !key.Pressed || key.Echo)
            return;

        var code = (int)key.Keycode;
        var first = (int)Key.Key1;
        if (code >= first && code <= (int)Key.Key6)
        {
            _shop.ActivateShopShortcut(code - first);
            GetViewport().SetInputAsHandled();
            return;
        }

        switch (key.Keycode)
        {
            case Key.A:
                _shop.ShowCategory(ShopCategory.Building);
                GetViewport().SetInputAsHandled();
                break;
            case Key.S:
                _shop.ShowCategory(ShopCategory.Equipment);
                GetViewport().SetInputAsHandled();
                break;
            case Key.D:
                _shop.ShowCategory(ShopCategory.Tactical);
                GetViewport().SetInputAsHandled();
                break;
            case Key.Q:
                _shop.ActivateFacilityShortcut(0);
                GetViewport().SetInputAsHandled();
                break;
            case Key.W:
                _shop.ActivateFacilityShortcut(1);
                GetViewport().SetInputAsHandled();
                break;
            case Key.E:
                _shop.ActivateFacilityShortcut(2);
                GetViewport().SetInputAsHandled();
                break;
            case Key.R:
                _shop.ActivateFacilityShortcut(3);
                GetViewport().SetInputAsHandled();
                break;
            case Key.T:
                _shop.ActivateFacilityShortcut(4);
                GetViewport().SetInputAsHandled();
                break;
            case Key.Y:
                _shop.ActivateFacilityShortcut(5);
                GetViewport().SetInputAsHandled();
                break;
        }
    }

    private static bool HasArg(string value)
    {
        foreach (var arg in OS.GetCmdlineUserArgs())
            if (arg == value)
                return true;
        return false;
    }

    public override void _Process(double delta)
    {
        _state.Tick(delta);

        if (_state.Paused || _state.GameOver || !_state.AutoSave)
            return;

        _autosaveTimer += delta;
        if (_autosaveTimer >= 30)
        {
            _autosaveTimer = 0;
            SaveService.Save(_state);
        }
    }

    private async void CaptureIfRequested()
    {
        string? target = null;
        var args = OS.GetCmdlineUserArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--screenshot")
            {
                target = args[i + 1];
                break;
            }
        }

        if (string.IsNullOrEmpty(target))
            return;

        for (int i = 0; i < 6; i++)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        if (HasArg("--combat"))
        {
            for (int i = 0; i < 760; i++)
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        GD.Print($"Viewport: {GetViewport().GetVisibleRect().Size}");
        GD.Print($"Battle: {_battle.Size}  Core: {_core.Size}  Shop: {_shop.Size}");

        var image = GetViewport().GetTexture().GetImage();
        var error = image.SavePng(target);
        GD.Print($"Screenshot saved to {target}: {error}");
        GetTree().Quit();
    }
}
