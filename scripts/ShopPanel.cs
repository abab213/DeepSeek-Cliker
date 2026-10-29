using Godot;
using System.Collections.Generic;

public partial class ShopPanel : PanelContainer
{
    private readonly GameState _state;
    private readonly List<CardView> _cards = new();
    private readonly Dictionary<ShopCategory, Button> _tabs = new();

    private ShopCategory _category = ShopCategory.Building;
    private VBoxContainer _list = null!;
    private Label _unlocked = null!;
    private Label _footer = null!;

    public ShopPanel(GameState state)
    {
        _state = state;
        CustomMinimumSize = new Vector2(624, 0);
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;
        AddThemeStyleboxOverride("panel", UiKit.PanelStyle());
        AddChild(UiKit.Pad(BuildContent(), 20, 18, 20, 16));
        UpdateTabs();
        _state.Changed += Refresh;
        Refresh();
    }

    public void ShowCategory(ShopCategory category)
    {
        _category = category;
        BuildCards();
        UpdateTabs();
        Refresh();
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

    private Control BuildShop()
    {
        var shop = UiKit.VBox(0);
        shop.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        shop.SizeFlagsVertical = SizeFlags.ExpandFill;

        var head = UiKit.HBox(8);
        head.AddChild(UiKit.Icon("res://assets/icons/icon_building.svg", 14, UiKit.Cyan));
        head.AddChild(UiKit.Label("商店", 15, UiKit.Text, UiKit.BoldFont));
        head.AddChild(UiKit.Tag("SHOP", UiKit.Gold, 7));
        head.AddChild(UiKit.HSpace(0, true));
        _unlocked = UiKit.Label("", 8, UiKit.Faint, UiKit.BodyFont);
        head.AddChild(_unlocked);
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

        _list = UiKit.VBox(6);
        _list.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(_list);
        shop.AddChild(scroll);

        shop.AddChild(UiKit.VSpace(6));
        _footer = UiKit.Label("", 8, UiKit.Faint, UiKit.BodyFont);
        shop.AddChild(_footer);

        BuildCards();
        return shop;
    }

    private Control BuildSafeZone()
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

    private Control BuildTabs()
    {
        var tabs = UiKit.HBox(6);
        tabs.AddChild(BuildTab(ShopCategory.Building, "建筑", "res://assets/icons/icon_building.svg", _state.Buildings.Count));
        tabs.AddChild(BuildTab(ShopCategory.Equipment, "战斗装备", "res://assets/icons/icon_target.svg", _state.Equipment.Count));
        tabs.AddChild(BuildTab(ShopCategory.Tactical, "战术道具", "res://assets/icons/icon_flask.svg", _state.Tacticals.Count));
        tabs.AddChild(UiKit.HSpace(0, true));
        return tabs;
    }

    private Button BuildTab(ShopCategory category, string title, string icon, int count)
    {
        var button = new Button
        {
            Text = $"{title} {count:00}",
            Icon = GD.Load<Texture2D>(icon),
            Alignment = HorizontalAlignment.Left,
            CustomMinimumSize = new Vector2(0, 26),
            FocusMode = FocusModeEnum.None,
            MouseDefaultCursorShape = CursorShape.PointingHand,
            TooltipText = $"{title}分类",
        };
        button.AddThemeConstantOverride("icon_max_width", 12);
        button.AddThemeConstantOverride("h_separation", 5);
        button.AddThemeFontOverride("font", UiKit.BoldFont);
        button.AddThemeFontSizeOverride("font_size", 9);
        button.Pressed += () => ShowCategory(category);
        _tabs[category] = button;
        return button;
    }

    private void UpdateTabs()
    {
        foreach (var (category, button) in _tabs)
        {
            var active = category == _category;
            button.AddThemeStyleboxOverride("normal", TabStyle(active));
            button.AddThemeStyleboxOverride("hover", TabStyle(active, true));
            button.AddThemeStyleboxOverride("pressed", TabStyle(true));
            button.AddThemeColorOverride("font_color", active ? UiKit.Text : UiKit.Faint);
            button.AddThemeColorOverride("font_hover_color", active ? UiKit.Text : UiKit.Cyan);
            button.AddThemeColorOverride("icon_normal_color", active ? UiKit.Cyan : UiKit.Alpha(UiKit.Faint, 0.8f));
            button.AddThemeColorOverride("icon_hover_color", UiKit.Cyan);
        }
    }

    private static StyleBoxFlat TabStyle(bool active, bool hover = false)
    {
        var style = UiKit.Box(
            active ? UiKit.Alpha(UiKit.Cyan, 0.12f) : hover ? UiKit.PanelRaised : new Color(0, 0, 0, 0),
            active ? UiKit.Alpha(UiKit.Cyan, 0.45f) : UiKit.Alpha(UiKit.Line, 0.55f),
            7, 1);
        style.ContentMarginLeft = 8;
        style.ContentMarginRight = 8;
        style.ContentMarginTop = 4;
        style.ContentMarginBottom = 4;
        return style;
    }

    private IEnumerable<ShopEntry> CurrentEntries()
    {
        if (_category == ShopCategory.Building)
            return _state.Buildings;
        if (_category == ShopCategory.Equipment)
            return _state.Equipment;
        return _state.Tacticals;
    }

    private void BuildCards()
    {
        if (_list == null)
            return;

        _cards.Clear();
        foreach (var child in _list.GetChildren())
        {
            _list.RemoveChild(child);
            child.QueueFree();
        }

        int index = 0;
        foreach (var entry in CurrentEntries())
        {
            var card = BuildCard(entry, index++);
            _cards.Add(card);
            _list.AddChild(card.Card);
        }
    }

    private CardView BuildCard(ShopEntry entry, int index)
    {
        var border = entry is Building { Recommended: true } ? UiKit.Alpha(UiKit.Gold, 0.34f) : UiKit.LineSoft;
        var card = new PanelContainer
        {
            CustomMinimumSize = new Vector2(0, 72),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Stop,
            TooltipText = $"{entry.Name}：{entry.Description}",
        };
        card.AddThemeStyleboxOverride("panel", UiKit.Box(UiKit.Alpha(UiKit.PanelDeep, 0.86f), border, 10, 1));

        var row = UiKit.HBox(9);
        row.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var iconTile = new PanelContainer
        {
            CustomMinimumSize = new Vector2(46, 46),
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
        };
        iconTile.AddThemeStyleboxOverride(
            "panel",
            UiKit.Box(UiKit.Alpha(entry.Accent, 0.055f), UiKit.Alpha(entry.Accent, 0.18f), 9));
        var center = new CenterContainer();
        center.AddChild(UiKit.Icon(entry.Icon, 34));
        iconTile.AddChild(center);
        row.AddChild(iconTile);

        var text = UiKit.VBox(2);
        text.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        text.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var titleLine = UiKit.HBox(6);
        titleLine.AddChild(UiKit.Label(entry.Name, 12, UiKit.Text, UiKit.BoldFont));
        if (entry is Building { Recommended: true })
            titleLine.AddChild(UiKit.Tag("推荐", UiKit.Gold, 7));
        titleLine.AddChild(UiKit.HSpace(0, true));

        var stateBox = UiKit.HBox(3);
        var stateCaption = UiKit.Label(entry.Category == ShopCategory.Tactical ? "状态" : "持有", 8, UiKit.Faint, UiKit.BodyFont);
        stateBox.AddChild(stateCaption);
        var stateLabel = UiKit.Display(entry.Category == ShopCategory.Tactical ? "可用" : "×" + entry.Owned, 9, UiKit.Alpha(UiKit.Cyan, 0.8f));
        stateBox.AddChild(stateLabel);
        titleLine.AddChild(stateBox);
        text.AddChild(titleLine);

        var desc = UiKit.Label(entry.Description, 9, UiKit.Muted, UiKit.BodyFont);
        desc.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        desc.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        text.AddChild(desc);

        var stat = UiKit.HBox(4);
        stat.AddChild(UiKit.Icon(entry.StatIcon, 10, UiKit.Alpha(entry.Accent, 0.92f)));
        stat.AddChild(UiKit.Label(entry.Stat, 9, UiKit.Alpha(entry.Accent, 0.96f), UiKit.BodyFont));
        text.AddChild(stat);
        row.AddChild(text);

        var costBox = UiKit.VBox(3);
        costBox.CustomMinimumSize = new Vector2(90, 0);
        costBox.SizeFlagsVertical = SizeFlags.ShrinkCenter;

        var costLine = UiKit.HBox(3);
        costLine.Alignment = BoxContainer.AlignmentMode.End;
        costLine.AddChild(UiKit.Icon("res://assets/icons/icon_rice.svg", 10, UiKit.Alpha(UiKit.Gold, 0.9f)));
        var costLabel = UiKit.Display(entry.Cost.ToString("N0"), 12, UiKit.Gold);
        costLine.AddChild(costLabel);
        costLine.AddChild(UiKit.Label("白饭", 8, UiKit.Faint, UiKit.BodyFont));
        costBox.AddChild(costLine);

        var button = UiKit.Button("购买", UiKit.Alpha(UiKit.Gold, 0.92f), UiKit.Gold, new Color("241a07"), 9);
        button.CustomMinimumSize = new Vector2(0, 22);
        button.Pressed += () =>
        {
            switch (entry.Category)
            {
                case ShopCategory.Building:
                    _state.TryBuy(index);
                    break;
                case ShopCategory.Equipment:
                    _state.BuyEquipment(index);
                    break;
                case ShopCategory.Tactical:
                    _state.UseTactical(index);
                    break;
            }
        };
        costBox.AddChild(button);
        row.AddChild(costBox);

        card.AddChild(UiKit.Pad(row, 10, 6, 10, 6));
        return new CardView(entry, card, costLabel, stateCaption, stateLabel, button);
    }

    private void Refresh()
    {
        if (_cards.Count == 0)
            return;

        _unlocked.Text = $"共 {_state.Buildings.Count + _state.Equipment.Count + _state.Tacticals.Count} 项";
        _footer.Text = _category switch
        {
            ShopCategory.Building => "建筑会持续产出白饭 · 价格随购买次数上涨",
            ShopCategory.Equipment => "战斗装备永久生效 · 价格随等级上涨",
            _ => "战术道具消耗白饭 · 使用后进入冷却",
        };

        foreach (var view in _cards)
        {
            var entry = view.Entry;
            var costText = entry.Cost.ToString("N0");
            if (view.Cost.Text != costText)
                view.Cost.Text = costText;

            var caption = entry.Category == ShopCategory.Tactical ? "状态" : "持有";
            string stateText;
            if (entry is Tactical tacticalState)
            {
                stateText = tacticalState.CooldownLeft > 0
                    ? $"冷却 {Mathf.CeilToInt((float)tacticalState.CooldownLeft)}s"
                    : tacticalState.Kind == TacticalKind.Repair && _state.Defense >= _state.MaxDefense
                        ? "防线满"
                        : "可用";
            }
            else
            {
                stateText = "×" + entry.Owned;
            }

            if (view.Caption.Text != caption)
                view.Caption.Text = caption;
            if (view.State.Text != stateText)
                view.State.Text = stateText;

            string buttonText;
            bool enabled;
            switch (entry)
            {
                case Tactical tactical when tactical.CooldownLeft > 0:
                    buttonText = "冷却中";
                    enabled = false;
                    break;
                case Tactical tactical when tactical.Kind == TacticalKind.Repair && _state.Defense >= _state.MaxDefense:
                    buttonText = "防线满";
                    enabled = false;
                    break;
                case Equipment equipment when equipment.Owned >= equipment.MaxLevel:
                    buttonText = "已满级";
                    enabled = false;
                    break;
                default:
                    enabled = _state.Rice >= entry.Cost;
                    buttonText = enabled ? (entry.Category == ShopCategory.Tactical ? "使用" : "购买") : "白饭不足";
                    break;
            }

            if (view.Button.Text != buttonText)
                view.Button.Text = buttonText;

            var affordable = _state.Rice >= entry.Cost;
            view.Cost.AddThemeColorOverride("font_color", affordable ? UiKit.Gold : UiKit.Alpha(UiKit.Red, 0.75f));

            if (view.LastEnabled != enabled || view.LastAffordable != affordable)
            {
                view.LastEnabled = enabled;
                view.LastAffordable = affordable;
                view.Card.Modulate = enabled ? Colors.White : new Color(1, 1, 1, 0.68f);
                ApplyButton(view.Button, enabled);
            }
        }
    }

    private static void ApplyButton(Button button, bool enabled)
    {
        button.Disabled = !enabled;
        if (enabled)
        {
            button.AddThemeStyleboxOverride("normal", UiKit.ButtonStyle(UiKit.Alpha(UiKit.Gold, 0.92f), UiKit.Gold));
            button.AddThemeStyleboxOverride("hover", UiKit.ButtonStyle(UiKit.Gold, UiKit.Gold));
            button.AddThemeStyleboxOverride("pressed", UiKit.ButtonStyle(UiKit.Alpha(UiKit.Gold, 0.8f), UiKit.Gold));
            button.AddThemeColorOverride("font_color", new Color("241a07"));
            button.AddThemeColorOverride("font_hover_color", new Color("241a07"));
            button.AddThemeColorOverride("font_pressed_color", new Color("241a07"));
        }
        else
        {
            button.AddThemeStyleboxOverride("normal", UiKit.ButtonStyle(UiKit.PanelDeep, UiKit.LineSoft));
            button.AddThemeStyleboxOverride("hover", UiKit.ButtonStyle(UiKit.PanelDeep, UiKit.LineSoft));
            button.AddThemeStyleboxOverride("pressed", UiKit.ButtonStyle(UiKit.PanelDeep, UiKit.LineSoft));
            button.AddThemeStyleboxOverride("disabled", UiKit.ButtonStyle(UiKit.PanelDeep, UiKit.LineSoft));
            button.AddThemeColorOverride("font_disabled_color", UiKit.Muted);
            button.AddThemeColorOverride("font_color", UiKit.Muted);
        }
    }

    private sealed class CardView
    {
        public ShopEntry Entry { get; }
        public PanelContainer Card { get; }
        public Label Cost { get; }
        public Label Caption { get; }
        public Label State { get; }
        public Button Button { get; }
        public bool? LastEnabled { get; set; }
        public bool? LastAffordable { get; set; }

        public CardView(ShopEntry entry, PanelContainer card, Label cost, Label caption, Label state, Button button)
        {
            Entry = entry;
            Card = card;
            Cost = cost;
            Caption = caption;
            State = state;
            Button = button;
        }
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
