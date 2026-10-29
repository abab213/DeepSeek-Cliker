using Godot;

public partial class CorePanel : PanelContainer
{
    public CorePanel()
    {
        CustomMinimumSize = new Vector2(380, 0);
        SizeFlagsVertical = SizeFlags.ExpandFill;
        AddThemeStyleboxOverride("panel", UiKit.PanelStyle(true));
        AddChild(UiKit.Pad(BuildContent(), 22, 18, 22, 18));
    }

    private Control BuildContent()
    {
        var root = UiKit.VBox(0);
        root.SizeFlagsVertical = SizeFlags.ExpandFill;

        var header = UiKit.HBox(10);
        var titles = UiKit.VBox(1);
        titles.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        var titleRow = UiKit.HBox(8);
        titleRow.AddChild(UiKit.Label("深潜核心", 20, UiKit.Text, UiKit.BoldFont));
        titleRow.AddChild(UiKit.Tag("CORE 07", UiKit.Cyan, 8));
        titles.AddChild(titleRow);
        titles.AddChild(UiKit.Label("D E E P S E E K  //  C O R E", 8, UiKit.Alpha(UiKit.Cyan, 0.72f), UiKit.DisplayFont));
        header.AddChild(titles);

        var online = UiKit.HBox(6);
        online.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        var dot = new Panel { CustomMinimumSize = new Vector2(6, 6), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        dot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Green, UiKit.Green, 3, 0));
        online.AddChild(dot);
        online.AddChild(UiKit.Label("在线", 9, UiKit.Green, UiKit.BodyFont));
        header.AddChild(online);
        root.AddChild(header);

        var accent = UiKit.HBox(4);
        accent.AddChild(UiKit.HLine(UiKit.Cyan, 2));
        accent.GetChild<ColorRect>(0).CustomMinimumSize = new Vector2(30, 2);
        var goldLine = new ColorRect
        {
            Color = UiKit.Alpha(UiKit.Gold, 0.7f),
            CustomMinimumSize = new Vector2(10, 2),
        };
        accent.AddChild(goldLine);
        root.AddChild(UiKit.VSpace(10));
        root.AddChild(accent);
        root.AddChild(UiKit.VSpace(10));

        var hint = UiKit.Label("点击核心，将算力转化为白饭", 11, UiKit.Muted, UiKit.BodyFont);
        hint.HorizontalAlignment = HorizontalAlignment.Center;
        root.AddChild(hint);
        root.AddChild(UiKit.VSpace(2));

        var coreCenter = new CenterContainer
        {
            CustomMinimumSize = new Vector2(0, 322),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        coreCenter.AddChild(new CoreDisplay());
        root.AddChild(coreCenter);

        var rewardCenter = new CenterContainer();
        var reward = UiKit.PanelBox(BuildReward(), UiKit.Box(UiKit.Alpha(UiKit.Gold, 0.1f), UiKit.Alpha(UiKit.Gold, 0.34f), 20));
        rewardCenter.AddChild(reward);
        root.AddChild(rewardCenter);

        root.AddChild(UiKit.VSpace(12));

        var stats = UiKit.HBox(10);
        var crit = InfoTile("res://assets/icons/icon_star.svg", "暴击率", "0%", UiKit.Gold);
        var combo = InfoTile("res://assets/icons/icon_fire.svg", "连击倍率", "×1.0", UiKit.Cyan);
        crit.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        combo.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        stats.AddChild(crit);
        stats.AddChild(combo);
        root.AddChild(stats);

        root.AddChild(UiKit.VSpace(14));
        root.AddChild(UiKit.PanelBox(
            BuildSituation(),
            UiKit.Box(UiKit.Alpha(UiKit.Cyan, 0.028f), UiKit.Alpha(UiKit.Cyan, 0.12f), 10)));
        root.AddChild(UiKit.VSpace(10));
        root.AddChild(UiKit.PanelBox(
            BuildNextGoal(),
            UiKit.Box(UiKit.Alpha(UiKit.Gold, 0.025f), UiKit.Alpha(UiKit.Gold, 0.15f), 10)));

        root.AddChild(UiKit.HSpace(0, true));

        var tip = UiKit.PanelBox(BuildTip(), UiKit.Box(UiKit.Alpha(UiKit.Cyan, 0.035f), UiKit.Alpha(UiKit.Cyan, 0.14f), 10));
        root.AddChild(tip);

        return root;
    }

    private Control BuildReward()
    {
        var row = UiKit.HBox(7);
        row.AddChild(UiKit.Icon("res://assets/icons/icon_rice.svg", 17, UiKit.Gold));
        row.AddChild(UiKit.Label("每次点击", 10, UiKit.Alpha(UiKit.Gold, 0.72f), UiKit.BodyFont));
        row.AddChild(UiKit.Display("+2", 18, UiKit.Gold));
        row.AddChild(UiKit.Label("白饭", 10, UiKit.Alpha(UiKit.Gold, 0.8f), UiKit.BodyFont));
        return UiKit.Pad(row, 14, 6, 14, 6);
    }

    private Control InfoTile(string icon, string caption, string value, Color color)
    {
        var row = UiKit.HBox(9);
        row.AddChild(UiKit.Icon(icon, 18, UiKit.Alpha(color, 0.9f)));

        var text = UiKit.VBox(0);
        text.AddChild(UiKit.Label(caption, 10, UiKit.Muted, UiKit.BodyFont));
        text.AddChild(UiKit.Display(value, 16, color));
        row.AddChild(text);

        var panel = UiKit.PanelBox(
            UiKit.Pad(row, 11, 8, 11, 8),
            UiKit.Box(UiKit.PanelDeep, UiKit.LineSoft, 10));
        return panel;
    }

    private Control BuildSituation()
    {
        var root = UiKit.VBox(4);

        var header = UiKit.HBox(6);
        header.AddChild(UiKit.Label("战况", 10, UiKit.Alpha(UiKit.Cyan, 0.78f), UiKit.BoldFont));
        header.AddChild(UiKit.Label("S I T U A T I O N", 7, UiKit.Alpha(UiKit.Cyan, 0.42f), UiKit.DisplayFont));
        header.AddChild(UiKit.HSpace(0, true));
        var liveDot = new Panel { CustomMinimumSize = new Vector2(5, 5), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        liveDot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Green, UiKit.Green, 3, 0));
        header.AddChild(liveDot);
        header.AddChild(UiKit.Label("实时", 8, UiKit.Alpha(UiKit.Green, 0.72f), UiKit.BodyFont));
        root.AddChild(header);
        root.AddChild(UiKit.VSpace(2));

        root.AddChild(SituationRow(UiKit.Red, "第 03 波 · 尸潮推进中", "剩余 12", "dot"));
        root.AddChild(UiKit.HLine(UiKit.Alpha(UiKit.Cyan, 0.06f), 1));
        root.AddChild(SituationRow(UiKit.Warm, "铁盆号 · 自动射击中", "击杀 42", "fire"));
        root.AddChild(UiKit.HLine(UiKit.Alpha(UiKit.Cyan, 0.06f), 1));
        root.AddChild(SituationRow(UiKit.Gold, "下一波", "00:42", "target"));

        return UiKit.Pad(root, 12, 9, 12, 9);
    }

    private Control SituationRow(Color color, string text, string value, string icon)
    {
        var row = UiKit.HBox(6);
        if (icon == "dot")
        {
            var dot = new Panel { CustomMinimumSize = new Vector2(5, 5), SizeFlagsVertical = SizeFlags.ShrinkCenter };
            dot.AddThemeStyleboxOverride("panel", UiKit.Box(color, color, 3, 0));
            row.AddChild(dot);
        }
        else
        {
            row.AddChild(UiKit.Icon($"res://assets/icons/icon_{icon}.svg", 11, UiKit.Alpha(color, 0.9f)));
        }

        row.AddChild(UiKit.Label(text, 10, UiKit.Alpha(UiKit.Text, 0.72f), UiKit.BodyFont));
        row.AddChild(UiKit.HSpace(0, true));
        row.AddChild(UiKit.Display(value, icon == "target" ? 11 : 10, color));
        return row;
    }

    private Control BuildNextGoal()
    {
        var root = UiKit.VBox(5);
        var top = UiKit.HBox(7);
        top.AddChild(UiKit.Icon("res://assets/icons/icon_target.svg", 13, UiKit.Gold));
        top.AddChild(UiKit.Label("下一目标", 9, UiKit.Muted, UiKit.BodyFont));
        top.AddChild(UiKit.Label("深潜食堂", 10, UiKit.Text, UiKit.BoldFont));
        top.AddChild(UiKit.HSpace(0, true));
        top.AddChild(UiKit.Display("1,280 / 2,400", 9, UiKit.Alpha(UiKit.Gold, 0.9f)));
        root.AddChild(top);

        var bar = new ProgressBar
        {
            Value = 53,
            MaxValue = 100,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(0, 4),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        bar.AddThemeStyleboxOverride("background", UiKit.Box(new Color("040a11"), new Color("040a11"), 2, 0));
        bar.AddThemeStyleboxOverride("fill", UiKit.Box(UiKit.Gold, UiKit.Gold, 2, 0));
        root.AddChild(bar);

        return UiKit.Pad(root, 11, 8, 11, 8);
    }

    private Control BuildTip()
    {
        var row = UiKit.HBox(7);
        row.AddChild(UiKit.Icon("res://assets/icons/icon_target.svg", 15, UiKit.Cyan));
        var label = UiKit.Label("点击核心 · 建筑会持续产出白饭", 10, UiKit.Muted, UiKit.BodyFont);
        label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row.AddChild(label);
        return UiKit.Pad(row, 11, 8, 11, 8);
    }
}
