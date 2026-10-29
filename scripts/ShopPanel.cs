using Godot;
using System.Collections.Generic;

public sealed record ShopItem(
    string Name,
    string Description,
    string Icon,
    string Stat,
    string StatIcon,
    Color Accent,
    string Cost,
    string Owned,
    bool Affordable,
    bool Recommended = false);

public partial class ShopPanel : PanelContainer
{
    public ShopPanel()
    {
        CustomMinimumSize = new Vector2(624, 0);
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;
        AddThemeStyleboxOverride("panel", UiKit.PanelStyle());
        AddChild(UiKit.Pad(BuildContent(), 20, 18, 20, 16));
    }

    private Control BuildContent()
    {
        var root = UiKit.VBox(0);

        var header = UiKit.HBox(10);
        var titles = UiKit.VBox(1);
        titles.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var titleRow = UiKit.HBox(8);
        titleRow.AddChild(UiKit.Label("补给站", 20, UiKit.Text, UiKit.BoldFont));
        titleRow.AddChild(UiKit.Tag("SUPPLY", UiKit.Gold, 8));
        titles.AddChild(titleRow);
        titles.AddChild(UiKit.Label("S U P P L Y  D E P O T  //  建 筑 与 战 术 物 资", 8, UiKit.Alpha(UiKit.Cyan, 0.7f), UiKit.DisplayFont));
        header.AddChild(titles);

        var online = UiKit.HBox(6);
        online.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        var dot = new Panel { CustomMinimumSize = new Vector2(6, 6), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        dot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Green, UiKit.Green, 3, 0));
        online.AddChild(dot);
        online.AddChild(UiKit.Label("补给中", 9, UiKit.Alpha(UiKit.Green, 0.86f), UiKit.BodyFont));
        header.AddChild(online);
        root.AddChild(header);

        var accent = UiKit.HBox(4);
        var accentMain = new ColorRect { Color = UiKit.Cyan, CustomMinimumSize = new Vector2(30, 2) };
        var accentGold = new ColorRect { Color = UiKit.Alpha(UiKit.Gold, 0.7f), CustomMinimumSize = new Vector2(10, 2) };
        accent.AddChild(accentMain);
        accent.AddChild(accentGold);
        root.AddChild(UiKit.VSpace(9));
        root.AddChild(accent);
        root.AddChild(UiKit.VSpace(12));

        root.AddChild(BuildTabs());
        root.AddChild(UiKit.VSpace(12));

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
        };

        var list = UiKit.VBox(8);
        list.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        list.CustomMinimumSize = new Vector2(560, 0);
        foreach (var item in Items)
            list.AddChild(BuildCard(item, item.Recommended));

        scroll.AddChild(list);
        root.AddChild(scroll);

        root.AddChild(UiKit.VSpace(10));
        root.AddChild(UiKit.HLine(UiKit.Alpha(UiKit.Cyan, 0.1f), 1));
        root.AddChild(UiKit.VSpace(9));

        var footer = UiKit.HBox(6);
        footer.AddChild(UiKit.Label("建筑会持续产出白饭 · 价格随购买次数上涨", 9, UiKit.Faint, UiKit.BodyFont));
        footer.AddChild(UiKit.HSpace(0, true));
        var autoDot = new Panel { CustomMinimumSize = new Vector2(5, 5), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        autoDot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Green, UiKit.Green, 3, 0));
        footer.AddChild(autoDot);
        footer.AddChild(UiKit.Label("自动结算中", 9, UiKit.Alpha(UiKit.Green, 0.8f), UiKit.BodyFont));
        root.AddChild(footer);

        return root;
    }

    private static readonly List<ShopItem> Items = new()
    {
        new ShopItem(
            "电饭煲",
            "基础炊事设备，稳定产出白饭。",
            "res://assets/icons/shop_rice_cooker.svg",
            "+0.5 白饭 / 秒",
            "res://assets/icons/icon_rice.svg",
            UiKit.Green,
            "15",
            "3",
            true,
            true),
        new ShopItem(
            "自动售饭机",
            "无人值守，24 小时不打烊。",
            "res://assets/icons/shop_vending.svg",
            "+2.0 白饭 / 秒",
            "res://assets/icons/icon_rice.svg",
            UiKit.Green,
            "100",
            "1",
            true),
        new ShopItem(
            "铁盆哨塔",
            "铁盆号同款装甲，提供火力支援。",
            "res://assets/icons/shop_tower.svg",
            "+1.0 白饭/秒 · 战斗力 +5",
            "res://assets/icons/icon_power.svg",
            UiKit.Cyan,
            "500",
            "0",
            true),
        new ShopItem(
            "深潜食堂",
            "让整个防区都吃上热饭。",
            "res://assets/icons/shop_canteen.svg",
            "+8.0 白饭 / 秒",
            "res://assets/icons/icon_rice.svg",
            UiKit.Green,
            "2,400",
            "0",
            false),
        new ShopItem(
            "稻田温室",
            "在废土上种出希望。",
            "res://assets/icons/shop_greenhouse.svg",
            "+30 白饭 / 秒",
            "res://assets/icons/icon_rice.svg",
            UiKit.Green,
            "12,000",
            "0",
            false),
        new ShopItem(
            "服务器农场",
            "用算力换米饭，废土最奢侈的农场。",
            "res://assets/icons/shop_server.svg",
            "+120 白饭 / 秒",
            "res://assets/icons/icon_rice.svg",
            UiKit.Green,
            "130,000",
            "0",
            false),
    };

    private static Control BuildTabs()
    {
        var tabs = UiKit.HBox(8);

        var active = UiKit.HBox(7);
        active.AddChild(UiKit.Icon("res://assets/icons/icon_building.svg", 14, UiKit.Cyan));
        active.AddChild(UiKit.Label("建筑", 10, UiKit.Text, UiKit.BoldFont));
        active.AddChild(UiKit.Display("06", 9, UiKit.Alpha(UiKit.Cyan, 0.8f)));
        var activePanel = UiKit.PanelBox(
            UiKit.Pad(active, 11, 5, 11, 5),
            UiKit.Box(UiKit.Alpha(UiKit.Cyan, 0.08f), UiKit.Alpha(UiKit.Cyan, 0.36f), 8));
        tabs.AddChild(activePanel);

        var inactive = UiKit.HBox(7);
        inactive.AddChild(UiKit.Icon("res://assets/icons/icon_lock.svg", 12, UiKit.Faint));
        inactive.AddChild(UiKit.Label("战术道具", 10, UiKit.Faint, UiKit.BodyFont));
        inactive.AddChild(UiKit.Label("即将开放", 8, UiKit.Alpha(UiKit.Faint, 0.8f), UiKit.BodyFont));
        var inactivePanel = UiKit.PanelBox(
            UiKit.Pad(inactive, 11, 5, 11, 5),
            UiKit.Box(new Color(0, 0, 0, 0), UiKit.Alpha(UiKit.Line, 0.55f), 8));
        inactivePanel.TooltipText = "战术道具将在后续版本开放";
        tabs.AddChild(inactivePanel);

        tabs.AddChild(UiKit.HSpace(0, true));
        tabs.AddChild(UiKit.Label("已解锁 6 项", 9, UiKit.Faint, UiKit.BodyFont));
        return tabs;
    }

    private static Control BuildCard(ShopItem item, bool recommended)
    {
        var border = recommended ? UiKit.Alpha(UiKit.Gold, 0.34f) : UiKit.LineSoft;
        var card = new PanelContainer
        {
            CustomMinimumSize = new Vector2(0, 88),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            Modulate = item.Affordable ? Colors.White : new Color(1, 1, 1, 0.62f),
            MouseFilter = MouseFilterEnum.Stop,
            TooltipText = $"{item.Name}：{item.Description}",
        };
        card.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Alpha(UiKit.PanelDeep, 0.86f), border, 11, 1));
        card.AddChild(UiKit.Pad(BuildCardRow(item, recommended), 12, 8, 12, 8));
        return card;
    }

    private static Control BuildCardRow(ShopItem item, bool recommended)
    {
        var row = UiKit.HBox(12);
        row.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var iconTile = new PanelContainer
        {
            CustomMinimumSize = new Vector2(58, 58),
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
        };
        iconTile.AddThemeStyleboxOverride(
            "panel",
            UiKit.Box(UiKit.Alpha(item.Accent, 0.055f), UiKit.Alpha(item.Accent, 0.18f), 10));
        var center = new CenterContainer();
        center.AddChild(UiKit.Icon(item.Icon, 44));
        iconTile.AddChild(center);
        row.AddChild(iconTile);

        var text = UiKit.VBox(3);
        text.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        text.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var titleLine = UiKit.HBox(7);
        titleLine.AddChild(UiKit.Label(item.Name, 14, UiKit.Text, UiKit.BoldFont));
        if (recommended)
            titleLine.AddChild(UiKit.Tag("推荐", UiKit.Gold, 8));
        titleLine.AddChild(UiKit.HSpace(0, true));
        var owned = UiKit.HBox(4);
        owned.AddChild(UiKit.Label("持有", 9, UiKit.Faint, UiKit.BodyFont));
        owned.AddChild(UiKit.Display("×" + item.Owned, 10, UiKit.Alpha(UiKit.Cyan, 0.8f)));
        titleLine.AddChild(owned);
        text.AddChild(titleLine);

        var desc = UiKit.Label(item.Description, 10, UiKit.Muted, UiKit.BodyFont);
        desc.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        desc.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        text.AddChild(desc);

        var stat = UiKit.HBox(5);
        stat.AddChild(UiKit.Icon(item.StatIcon, 12, UiKit.Alpha(item.Accent, 0.92f)));
        stat.AddChild(UiKit.Label(item.Stat, 10, UiKit.Alpha(item.Accent, 0.96f), UiKit.BodyFont));
        text.AddChild(stat);

        row.AddChild(text);

        var costBox = UiKit.VBox(4);
        costBox.CustomMinimumSize = new Vector2(104, 0);
        costBox.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var costLine = UiKit.HBox(4);
        costLine.Alignment = BoxContainer.AlignmentMode.End;
        costLine.AddChild(UiKit.Icon("res://assets/icons/icon_rice.svg", 12, UiKit.Alpha(UiKit.Gold, 0.9f)));
        costLine.AddChild(UiKit.Display(item.Cost, 15, item.Affordable ? UiKit.Gold : UiKit.Alpha(UiKit.Red, 0.75f)));
        costLine.AddChild(UiKit.Label("白饭", 9, UiKit.Faint, UiKit.BodyFont));
        costBox.AddChild(costLine);

        var button = item.Affordable
            ? UiKit.Button("购买", UiKit.Alpha(UiKit.Gold, 0.92f), UiKit.Gold, new Color("241a07"), 10)
            : UiKit.Button("白饭不足", UiKit.PanelDeep, UiKit.LineSoft, UiKit.Faint, 9);
        button.Disabled = !item.Affordable;
        button.CustomMinimumSize = new Vector2(0, 26);
        costBox.AddChild(button);

        row.AddChild(costBox);
        return row;
    }
}
