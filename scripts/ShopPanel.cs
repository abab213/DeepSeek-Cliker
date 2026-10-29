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
        titleRow.AddChild(UiKit.Label("营地", 20, UiKit.Text, UiKit.BoldFont));
        titleRow.AddChild(UiKit.Tag("CAMP", UiKit.Gold, 8));
        titles.AddChild(titleRow);
        titles.AddChild(UiKit.Label("C A M P  //  商 店 · 安 全 区", 8, UiKit.Alpha(UiKit.Cyan, 0.7f), UiKit.DisplayFont));
        header.AddChild(titles);

        var online = UiKit.HBox(6);
        online.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        var dot = new Panel { CustomMinimumSize = new Vector2(6, 6), SizeFlagsVertical = SizeFlags.ShrinkCenter };
        dot.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Green, UiKit.Green, 3, 0));
        online.AddChild(dot);
        online.AddChild(UiKit.Label("营地在线", 9, UiKit.Alpha(UiKit.Green, 0.86f), UiKit.BodyFont));
        header.AddChild(online);
        root.AddChild(header);

        var accent = UiKit.HBox(4);
        accent.AddChild(new ColorRect { Color = UiKit.Cyan, CustomMinimumSize = new Vector2(30, 2) });
        accent.AddChild(new ColorRect { Color = UiKit.Alpha(UiKit.Gold, 0.7f), CustomMinimumSize = new Vector2(10, 2) });
        root.AddChild(UiKit.VSpace(9));
        root.AddChild(accent);
        root.AddChild(UiKit.VSpace(12));

        var body = UiKit.VBox(10);
        body.SizeFlagsVertical = SizeFlags.ExpandFill;
        body.AddChild(BuildShop());
        body.AddChild(BuildSafeZone());
        root.AddChild(body);

        return root;
    }

    private static Control BuildShop()
    {
        var shop = UiKit.VBox(0);
        shop.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        shop.SizeFlagsVertical = SizeFlags.ExpandFill;

        var head = UiKit.HBox(8);
        head.AddChild(UiKit.Icon("res://assets/icons/icon_building.svg", 14, UiKit.Cyan));
        head.AddChild(UiKit.Label("商店", 15, UiKit.Text, UiKit.BoldFont));
        head.AddChild(UiKit.Tag("SHOP", UiKit.Gold, 7));
        head.AddChild(UiKit.HSpace(0, true));
        head.AddChild(UiKit.Label("已解锁 6 项", 8, UiKit.Faint, UiKit.BodyFont));
        shop.AddChild(head);
        shop.AddChild(UiKit.VSpace(8));

        shop.AddChild(BuildTabs());
        shop.AddChild(UiKit.VSpace(8));

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
        };

        var list = UiKit.VBox(6);
        list.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        foreach (var item in Items)
            list.AddChild(BuildCard(item, item.Recommended));

        scroll.AddChild(list);
        shop.AddChild(scroll);

        shop.AddChild(UiKit.VSpace(6));
        shop.AddChild(UiKit.Label("建筑会持续产出白饭 · 价格随购买次数上涨", 8, UiKit.Faint, UiKit.BodyFont));
        return shop;
    }

    private static Control BuildSafeZone()
    {
        var safe = UiKit.VBox(6);
        safe.CustomMinimumSize = new Vector2(0, 104);
        safe.SizeFlagsVertical = SizeFlags.ShrinkEnd;

        var head = UiKit.HBox(7);
        head.AddChild(UiKit.Icon("res://assets/icons/icon_shield.svg", 13, UiKit.Alpha(UiKit.Green, 0.9f)));
        head.AddChild(UiKit.Label("安全区", 13, UiKit.Text, UiKit.BoldFont));
        head.AddChild(UiKit.Tag("空地", UiKit.Alpha(UiKit.Green, 0.72f), 7));
        head.AddChild(UiKit.HSpace(0, true));
        head.AddChild(UiKit.Label("S A F E  Z O N E", 7, UiKit.Alpha(UiKit.Cyan, 0.45f), UiKit.DisplayFont));
        safe.AddChild(head);

        var ground = new SafeGround
        {
            CustomMinimumSize = new Vector2(0, 70),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore,
        };

        var center = new CenterContainer();
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        center.MouseFilter = MouseFilterEnum.Ignore;

        var row = UiKit.HBox(10);
        row.Alignment = BoxContainer.AlignmentMode.Center;
        row.AddChild(UiKit.Icon("res://assets/icons/icon_shield.svg", 18, UiKit.Alpha(UiKit.Green, 0.28f)));
        row.AddChild(UiKit.Label("暂无设施", 10, UiKit.Alpha(UiKit.Text, 0.62f), UiKit.BoldFont));
        row.AddChild(UiKit.Label("· 等待部署", 8, UiKit.Faint, UiKit.BodyFont));
        center.AddChild(row);
        ground.AddChild(center);
        safe.AddChild(ground);
        return safe;
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
        var tabs = UiKit.HBox(6);
        tabs.AddChild(BuildTab("res://assets/icons/icon_building.svg", "建筑", "06", true));
        tabs.AddChild(BuildTab("res://assets/icons/icon_target.svg", "战斗装备", "00", false));
        tabs.AddChild(BuildTab("res://assets/icons/icon_flask.svg", "战术道具", "00", false));
        tabs.AddChild(UiKit.HSpace(0, true));
        return tabs;
    }

    private static Control BuildTab(string icon, string title, string count, bool active)
    {
        var group = UiKit.HBox(5);
        group.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        group.AddChild(UiKit.Icon(icon, 12, active ? UiKit.Cyan : UiKit.Alpha(UiKit.Faint, 0.9f)));
        group.AddChild(UiKit.Label(title, 9, active ? UiKit.Text : UiKit.Faint, active ? UiKit.BoldFont : UiKit.BodyFont));
        group.AddChild(UiKit.Display(count, 8, active ? UiKit.Alpha(UiKit.Cyan, 0.8f) : UiKit.Alpha(UiKit.Faint, 0.7f)));

        var panel = UiKit.PanelBox(
            UiKit.Pad(group, 8, 4, 8, 4),
            active
                ? UiKit.Box(UiKit.Alpha(UiKit.Cyan, 0.08f), UiKit.Alpha(UiKit.Cyan, 0.36f), 7)
                : UiKit.Box(new Color(0, 0, 0, 0), UiKit.Alpha(UiKit.Line, 0.55f), 7));
        panel.TooltipText = active ? $"{title}分类" : $"{title}分类暂未上架";
        return panel;
    }

    private static Control BuildCard(ShopItem item, bool recommended)
    {
        var border = recommended ? UiKit.Alpha(UiKit.Gold, 0.34f) : UiKit.LineSoft;
        var card = new PanelContainer
        {
            CustomMinimumSize = new Vector2(0, 72),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            Modulate = item.Affordable ? Colors.White : new Color(1, 1, 1, 0.62f),
            MouseFilter = MouseFilterEnum.Stop,
            TooltipText = $"{item.Name}：{item.Description}",
        };
        card.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Alpha(UiKit.PanelDeep, 0.86f), border, 10, 1));
        card.AddChild(UiKit.Pad(BuildCardRow(item, recommended), 10, 6, 10, 6));
        return card;
    }

    private static Control BuildCardRow(ShopItem item, bool recommended)
    {
        var row = UiKit.HBox(9);
        row.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var iconTile = new PanelContainer
        {
            CustomMinimumSize = new Vector2(46, 46),
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
        };
        iconTile.AddThemeStyleboxOverride(
            "panel",
            UiKit.Box(UiKit.Alpha(item.Accent, 0.055f), UiKit.Alpha(item.Accent, 0.18f), 9));
        var center = new CenterContainer();
        center.AddChild(UiKit.Icon(item.Icon, 34));
        iconTile.AddChild(center);
        row.AddChild(iconTile);

        var text = UiKit.VBox(2);
        text.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        text.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var titleLine = UiKit.HBox(6);
        titleLine.AddChild(UiKit.Label(item.Name, 12, UiKit.Text, UiKit.BoldFont));
        if (recommended)
            titleLine.AddChild(UiKit.Tag("推荐", UiKit.Gold, 7));
        titleLine.AddChild(UiKit.HSpace(0, true));
        var owned = UiKit.HBox(3);
        owned.AddChild(UiKit.Label("持有", 8, UiKit.Faint, UiKit.BodyFont));
        owned.AddChild(UiKit.Display("×" + item.Owned, 9, UiKit.Alpha(UiKit.Cyan, 0.8f)));
        titleLine.AddChild(owned);
        text.AddChild(titleLine);

        var desc = UiKit.Label(item.Description, 9, UiKit.Muted, UiKit.BodyFont);
        desc.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        desc.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        text.AddChild(desc);

        var stat = UiKit.HBox(4);
        stat.AddChild(UiKit.Icon(item.StatIcon, 10, UiKit.Alpha(item.Accent, 0.92f)));
        stat.AddChild(UiKit.Label(item.Stat, 9, UiKit.Alpha(item.Accent, 0.96f), UiKit.BodyFont));
        text.AddChild(stat);

        row.AddChild(text);

        var costBox = UiKit.VBox(3);
        costBox.CustomMinimumSize = new Vector2(90, 0);
        costBox.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var costLine = UiKit.HBox(3);
        costLine.Alignment = BoxContainer.AlignmentMode.End;
        costLine.AddChild(UiKit.Icon("res://assets/icons/icon_rice.svg", 10, UiKit.Alpha(UiKit.Gold, 0.9f)));
        costLine.AddChild(UiKit.Display(item.Cost, 12, item.Affordable ? UiKit.Gold : UiKit.Alpha(UiKit.Red, 0.75f)));
        costLine.AddChild(UiKit.Label("白饭", 8, UiKit.Faint, UiKit.BodyFont));
        costBox.AddChild(costLine);

        var button = item.Affordable
            ? UiKit.Button("购买", UiKit.Alpha(UiKit.Gold, 0.92f), UiKit.Gold, new Color("241a07"), 9)
            : UiKit.Button("白饭不足", UiKit.PanelDeep, UiKit.LineSoft, UiKit.Faint, 8);
        button.Disabled = !item.Affordable;
        button.CustomMinimumSize = new Vector2(0, 22);
        costBox.AddChild(button);

        row.AddChild(costBox);
        return row;
    }
}

public partial class SafeGround : Control
{
    public override void _Draw()
    {
        var w = Size.X;
        var h = Size.Y;
        var rect = new Rect2(1, 1, Mathf.Max(0, w - 2), Mathf.Max(0, h - 2));
        DrawRect(rect, new Color(0.015f, 0.035f, 0.05f, 0.52f));

        var grid = new Color(0.24f, 0.7f, 0.75f, 0.045f);
        for (float x = 0; x < w; x += 22)
            DrawLine(new Vector2(x, 0), new Vector2(x, h), grid, 1);
        for (float y = 0; y < h; y += 22)
            DrawLine(new Vector2(0, y), new Vector2(w, y), grid, 1);

        var line = new Color(0.28f, 0.85f, 0.85f, 0.28f);
        for (float x = 4; x < w; x += 14)
        {
            DrawLine(new Vector2(x, 1), new Vector2(Mathf.Min(x + 7, w), 1), line, 1);
            DrawLine(new Vector2(x, h - 1), new Vector2(Mathf.Min(x + 7, w), h - 1), line, 1);
        }
        for (float y = 4; y < h; y += 14)
        {
            DrawLine(new Vector2(1, y), new Vector2(1, Mathf.Min(y + 7, h)), line, 1);
            DrawLine(new Vector2(w - 1, y), new Vector2(w - 1, Mathf.Min(y + 7, h)), line, 1);
        }

        var corner = new Color(0.35f, 0.95f, 1f, 0.55f);
        DrawLine(new Vector2(0, 16), new Vector2(0, 0), corner, 2);
        DrawLine(new Vector2(0, 0), new Vector2(16, 0), corner, 2);
        DrawLine(new Vector2(w - 16, 0), new Vector2(w, 0), corner, 2);
        DrawLine(new Vector2(w, 0), new Vector2(w, 16), corner, 2);
        DrawLine(new Vector2(0, h - 16), new Vector2(0, h), corner, 2);
        DrawLine(new Vector2(0, h), new Vector2(16, h), corner, 2);
        DrawLine(new Vector2(w - 16, h), new Vector2(w, h), corner, 2);
        DrawLine(new Vector2(w, h), new Vector2(w, h - 16), corner, 2);
    }
}
