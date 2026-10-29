using Godot;
using System;

public partial class Main : Control
{
    private BattlePanel _battle = null!;
    private CorePanel _core = null!;
    private ShopPanel _shop = null!;
    private GameState _state = null!;

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
        layout.AddChild(new TopBar(_state));

        var content = new HBoxContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        content.AddThemeConstantOverride("separation", 20);
        layout.AddChild(content);

        _battle = new BattlePanel();
        _core = new CorePanel(_state);
        _shop = new ShopPanel(_state);

        content.AddChild(_battle);
        content.AddChild(_core);
        content.AddChild(_shop);

        AddChild(frame);

        if (HasArg("--demo"))
        {
            for (int i = 0; i < 12; i++)
                _state.Click();
            _state.TryBuy(0);
        }

        CaptureIfRequested();
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

        GD.Print($"Viewport: {GetViewport().GetVisibleRect().Size}");
        GD.Print($"Battle: {_battle.Size}  Core: {_core.Size}  Shop: {_shop.Size}");

        var image = GetViewport().GetTexture().GetImage();
        var error = image.SavePng(target);
        GD.Print($"Screenshot saved to {target}: {error}");
        GetTree().Quit();
    }
}
