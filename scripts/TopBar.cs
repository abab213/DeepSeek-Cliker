using Godot;

public partial class TopBar : PanelContainer
{
    public TopBar()
    {
        CustomMinimumSize = new Vector2(0, 84);
        AddThemeStyleboxOverride(
            "panel",
            UiKit.Box(UiKit.Panel, UiKit.Line, 16, 1, 14, new Color(0, 0, 0, 0.34f)));

        AddChild(UiKit.Pad(BuildContent(), 20, 10, 16, 10));
    }

    private Control BuildContent()
    {
        var root = UiKit.HBox(18);
        root.SizeFlagsVertical = SizeFlags.ExpandFill;

        root.AddChild(BuildBrand());
        root.AddChild(MakeDivider());
        root.AddChild(BuildStatsColumn());
        root.AddChild(BuildActions());

        return root;
    }

    private Control BuildBrand()
    {
        var brand = UiKit.HBox(12);
        brand.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        brand.AddChild(UiKit.Icon("res://assets/ui/logo_mark.svg", 48));

        var text = UiKit.VBox(2);
        text.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var titleRow = UiKit.HBox(7);
        titleRow.AddChild(UiKit.Label("深潜防线", 22, UiKit.Text, UiKit.BoldFont));
        titleRow.AddChild(UiKit.Tag("2077", UiKit.Gold, 8));
        text.AddChild(titleRow);

        text.AddChild(UiKit.Label("D E E P S E E K  //  L A S T  D E F E N S E", 8, UiKit.Alpha(UiKit.Cyan, 0.82f), UiKit.DisplayFont));
        text.AddChild(UiKit.Label("人类最后的补给站", 9, UiKit.Faint, UiKit.BodyFont));

        brand.AddChild(text);
        return brand;
    }

    private Control BuildStatsColumn()
    {
        var column = UiKit.VBox(3);
        column.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        column.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        column.AddChild(BuildStats());
        column.AddChild(BuildSecondaryStats());
        return column;
    }

    private Control BuildStats()
    {
        var stats = UiKit.HBox(0);
        stats.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        stats.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        AddStat(stats, "res://assets/icons/icon_rice.svg", "白饭存量", "1,280", "碗", UiKit.Gold);
        AddSeparator(stats);
        AddStat(stats, "res://assets/icons/icon_rate.svg", "白饭产出", "3.5", "/秒", UiKit.Green);
        AddSeparator(stats);
        AddStat(stats, "res://assets/icons/icon_power.svg", "战斗力", "24", "级", UiKit.Cyan);
        AddSeparator(stats);
        AddStat(stats, "res://assets/icons/icon_skull.svg", "当前波次", "03", "/∞", UiKit.Text);
        AddSeparator(stats);
        AddStat(stats, "res://assets/icons/icon_star.svg", "防线积分", "360", "分", UiKit.Gold);

        return stats;
    }

    private Control BuildSecondaryStats()
    {
        var row = UiKit.HBox(12);
        row.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var defense = UiKit.HBox(5);
        defense.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        defense.AddChild(UiKit.Icon("res://assets/icons/icon_shield.svg", 10, UiKit.Green));
        defense.AddChild(UiKit.Label("防线 780/1000", 8, UiKit.Muted, UiKit.BodyFont));
        var bar = new ProgressBar
        {
            Value = 78,
            MaxValue = 100,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(82, 4),
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
        };
        bar.AddThemeStyleboxOverride("background", UiKit.Box(new Color("040a11"), new Color("040a11"), 2, 0));
        bar.AddThemeStyleboxOverride("fill", UiKit.Box(UiKit.Green, UiKit.Green, 2, 0));
        defense.AddChild(bar);
        defense.AddChild(UiKit.Display("78%", 9, UiKit.Green));
        row.AddChild(defense);
        row.AddChild(MakeDivider(11));
        row.AddChild(SecondaryItem("res://assets/icons/icon_skull.svg", UiKit.Alpha(UiKit.Red, 0.9f), "击杀 42", UiKit.Muted));
        row.AddChild(MakeDivider(11));
        row.AddChild(SecondaryItem("res://assets/icons/icon_fire.svg", UiKit.Alpha(UiKit.Green, 0.85f), "防线待命", UiKit.Alpha(UiKit.Green, 0.9f)));
        row.AddChild(UiKit.HSpace(0, true));
        var threat = UiKit.HBox(5);
        threat.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        var dot = new Panel { CustomMinimumSize = new Vector2(5, 5), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        dot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Green, UiKit.Green, 3, 0));
        threat.AddChild(dot);
        threat.AddChild(UiKit.Label("威胁等级 低", 8, UiKit.Muted, UiKit.BodyFont));
        row.AddChild(threat);
        return row;
    }

    private static Control SecondaryItem(string icon, Color tint, string text, Color color)
    {
        var item = UiKit.HBox(4);
        item.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        item.AddChild(UiKit.Icon(icon, 10, tint));
        item.AddChild(UiKit.Label(text, 8, color, UiKit.BodyFont));
        return item;
    }

    private Control BuildActions()
    {
        var actions = UiKit.HBox(8);
        actions.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var online = UiKit.HBox(6);
        online.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        var dot = new Panel { CustomMinimumSize = new Vector2(6, 6), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        dot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Green, UiKit.Green, 3, 0));
        online.AddChild(dot);
        online.AddChild(UiKit.Label("补给线稳定", 9, UiKit.Alpha(UiKit.Green, 0.85f), UiKit.BodyFont));

        online.CustomMinimumSize = new Vector2(82, 0);
        actions.AddChild(online);

        actions.AddChild(MakeDivider(28));
        actions.AddChild(IconButton("res://assets/icons/icon_pause.svg", "暂停"));
        actions.AddChild(IconButton("res://assets/icons/icon_settings.svg", "设置"));

        return actions;
    }

    private static void AddStat(BoxContainer parent, string icon, string caption, string value, string unit, Color color)
    {
        var chip = UiKit.HBox(9);
        chip.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        chip.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        chip.CustomMinimumSize = new Vector2(132, 0);

        var iconRect = UiKit.Icon(icon, 22, UiKit.Alpha(color, 0.92f));
        iconRect.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        chip.AddChild(iconRect);

        var text = UiKit.VBox(0);
        text.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        text.AddChild(UiKit.Label(caption, 10, UiKit.Muted, UiKit.BodyFont));

        var row = UiKit.HBox(4);
        row.AddChild(UiKit.Display(value, 21, color));
        var unitLabel = UiKit.Label(unit, 10, UiKit.Faint, UiKit.BodyFont);
        unitLabel.SizeFlagsVertical = SizeFlags.ShrinkEnd;
        row.AddChild(unitLabel);
        text.AddChild(row);

        chip.AddChild(text);
        parent.AddChild(chip);
    }

    private static void AddSeparator(BoxContainer parent)
    {
        parent.AddChild(MakeDivider(34));
    }

    private static Control MakeDivider(int height = 46)
    {
        var divider = UiKit.VLine(UiKit.Alpha(UiKit.Cyan, 0.11f), 1);
        divider.CustomMinimumSize = new Vector2(1, height);
        divider.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        return divider;
    }

    private static Button IconButton(string iconPath, string tooltip)
    {
        var button = new Button
        {
            CustomMinimumSize = new Vector2(38, 38),
            TooltipText = tooltip,
            FocusMode = FocusModeEnum.None,
            MouseDefaultCursorShape = CursorShape.PointingHand,
            ExpandIcon = true,
            Icon = GD.Load<Texture2D>(iconPath),
        };

        button.AddThemeConstantOverride("icon_max_width", 17);
        button.AddThemeStyleboxOverride("normal", UiKit.Box(UiKit.PanelDeep, UiKit.LineSoft, 9));
        button.AddThemeStyleboxOverride("hover", UiKit.Box(UiKit.PanelRaised, UiKit.Alpha(UiKit.Cyan, 0.5f), 9));
        button.AddThemeStyleboxOverride("pressed", UiKit.Box(UiKit.BgTop, UiKit.Cyan, 9));
        button.AddThemeColorOverride("icon_normal_color", UiKit.Muted);
        button.AddThemeColorOverride("icon_hover_color", UiKit.Cyan);
        button.AddThemeColorOverride("icon_pressed_color", UiKit.Cyan);
        button.AddThemeColorOverride("icon_disabled_color", UiKit.Faint);
        return button;
    }
}
