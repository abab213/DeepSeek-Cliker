using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Building
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Icon { get; init; } = "";
    public string Stat { get; init; } = "";
    public string StatIcon { get; init; } = "";
    public Color Accent { get; init; }
    public double BaseCost { get; init; }
    public double Production { get; init; }
    public int Combat { get; init; }
    public int Owned { get; set; }
    public bool Recommended { get; init; }

    public double Cost => Math.Floor(BaseCost * Math.Pow(1.15, Owned));
}

public sealed class GameState
{
    public const double ClickBase = 2.0;
    public const double CritChance = 0.05;
    private const double ComboStep = 0.1;
    private const double ComboMax = 2.0;
    private const double ComboWindow = 1.4;

    private readonly RandomNumberGenerator _rng = new();
    private double _refreshTimer;
    private double _comboTimer;

    public double Rice { get; private set; } = 1280;
    public double Production { get; private set; }
    public int CombatPower { get; private set; } = 24;
    public double Defense { get; private set; } = 780;
    public double MaxDefense { get; private set; } = 1000;
    public int Kills { get; private set; } = 42;
    public int Score { get; private set; } = 360;
    public int Wave { get; private set; } = 3;
    public double Combo { get; private set; } = 1.0;
    public int CritPercent => (int)Math.Round(CritChance * 100);

    public List<Building> Buildings { get; } = new();

    public event Action? Changed;
    public event Action<double, bool>? Clicked;

    public GameState()
    {
        _rng.Randomize();

        Buildings.Add(new Building
        {
            Name = "电饭煲",
            Description = "基础炊事设备，稳定产出白饭。",
            Icon = "res://assets/icons/shop_rice_cooker.svg",
            Stat = "+0.5 白饭 / 秒",
            StatIcon = "res://assets/icons/icon_rice.svg",
            Accent = UiKit.Green,
            BaseCost = 15,
            Production = 0.5,
            Owned = 3,
            Recommended = true,
        });
        Buildings.Add(new Building
        {
            Name = "自动售饭机",
            Description = "无人值守，24 小时不打烊。",
            Icon = "res://assets/icons/shop_vending.svg",
            Stat = "+2.0 白饭 / 秒",
            StatIcon = "res://assets/icons/icon_rice.svg",
            Accent = UiKit.Green,
            BaseCost = 100,
            Production = 2.0,
            Owned = 1,
        });
        Buildings.Add(new Building
        {
            Name = "铁盆哨塔",
            Description = "铁盆号同款装甲，提供火力支援。",
            Icon = "res://assets/icons/shop_tower.svg",
            Stat = "+1.0 白饭/秒 · 战斗力 +5",
            StatIcon = "res://assets/icons/icon_power.svg",
            Accent = UiKit.Cyan,
            BaseCost = 500,
            Production = 1.0,
            Combat = 5,
        });
        Buildings.Add(new Building
        {
            Name = "深潜食堂",
            Description = "让整个防区都吃上热饭。",
            Icon = "res://assets/icons/shop_canteen.svg",
            Stat = "+8.0 白饭 / 秒",
            StatIcon = "res://assets/icons/icon_rice.svg",
            Accent = UiKit.Green,
            BaseCost = 2400,
            Production = 8.0,
        });
        Buildings.Add(new Building
        {
            Name = "稻田温室",
            Description = "在废土上种出希望。",
            Icon = "res://assets/icons/shop_greenhouse.svg",
            Stat = "+30 白饭 / 秒",
            StatIcon = "res://assets/icons/icon_rice.svg",
            Accent = UiKit.Green,
            BaseCost = 12000,
            Production = 30.0,
        });
        Buildings.Add(new Building
        {
            Name = "服务器农场",
            Description = "用算力换米饭，废土最奢侈的农场。",
            Icon = "res://assets/icons/shop_server.svg",
            Stat = "+120 白饭 / 秒",
            StatIcon = "res://assets/icons/icon_rice.svg",
            Accent = UiKit.Green,
            BaseCost = 130000,
            Production = 120.0,
        });

        Recalculate();
    }

    public void Tick(double delta)
    {
        Rice += Production * delta;

        if (_comboTimer > 0)
        {
            _comboTimer -= delta;
            if (_comboTimer <= 0)
                Combo = 1.0;
        }

        _refreshTimer += delta;
        if (_refreshTimer >= 0.1)
        {
            _refreshTimer = 0;
            Changed?.Invoke();
        }
    }

    public void Click()
    {
        Combo = Math.Min(ComboMax, Combo + ComboStep);
        _comboTimer = ComboWindow;

        bool crit = _rng.Randf() < CritChance;
        double gain = ClickBase * Combo * (crit ? 2.0 : 1.0);
        Rice += gain;

        Clicked?.Invoke(gain, crit);
        Changed?.Invoke();
    }

    public bool TryBuy(int index)
    {
        if (index < 0 || index >= Buildings.Count)
            return false;

        var building = Buildings[index];
        if (Rice < building.Cost)
            return false;

        Rice -= building.Cost;
        building.Owned++;
        Recalculate();
        Changed?.Invoke();
        return true;
    }

    public Building NextGoal()
    {
        return Buildings.FirstOrDefault(b => Rice < b.Cost) ?? Buildings[^1];
    }

    private void Recalculate()
    {
        Production = Buildings.Sum(b => b.Production * b.Owned);
        CombatPower = 24 + Buildings.Sum(b => b.Combat * b.Owned);
    }
}
