using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum ShopCategory
{
    Building,
    Equipment,
    Tactical,
}

public abstract class ShopEntry
{
    public ShopCategory Category { get; init; } = ShopCategory.Building;
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Icon { get; init; } = "";
    public string Stat { get; init; } = "";
    public string StatIcon { get; init; } = "";
    public Color Accent { get; init; }
    public double BaseCost { get; init; }
    public double Growth { get; init; } = 1.15;
    public int Owned { get; set; }
    public double Cooldown { get; init; }
    public double CooldownLeft { get; set; }

    public virtual double Cost => Math.Floor(BaseCost * Math.Pow(Growth, Owned));
}

public sealed class Building : ShopEntry
{
    public double Production { get; init; }
    public int Combat { get; init; }
    public bool Recommended { get; init; }
}

public sealed class Equipment : ShopEntry
{
    public int MaxLevel { get; init; } = 5;
    public int PowerBonus { get; init; }
    public int DamageBonus { get; init; }
    public double FireIntervalDelta { get; init; }
    public double BruteBonus { get; init; }
    public int DefenseBonus { get; init; }
}

public sealed class Facility
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Icon { get; init; } = "";
    public string Stat { get; init; } = "";
    public Color Accent { get; init; }
    public double BaseCost { get; init; }
    public double Growth { get; init; } = 1.5;
    public int MaxLevel { get; init; } = 5;
    public int Owned { get; set; }
    public double DefenseRegen { get; init; }
    public double CooldownReduction { get; init; }
    public int ClickBonus { get; init; }

    public double Cost => Math.Floor(BaseCost * Math.Pow(Growth, Owned));
}

public enum TacticalKind
{
    Repair,
    Overload,
    Freeze,
}

public sealed class Tactical : ShopEntry
{
    public TacticalKind Kind { get; init; }
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
    public double GunDamage { get; private set; } = 36.4;
    public double FireInterval { get; private set; } = 0.38;
    public double BruteDamageMultiplier { get; private set; } = 1.0;
    public double Defense { get; private set; } = 1000;
    public double MaxDefense { get; private set; } = 1000;
    public int Kills { get; private set; }
    public int Score { get; private set; }
    public int Wave { get; private set; } = 1;
    public bool WaveActive { get; private set; }
    public double WaveTimer { get; private set; } = 5;
    public bool GameOver { get; private set; }
    public bool Paused { get; private set; }
    public bool AutoSave { get; set; } = true;
    public bool EffectsEnabled { get; set; } = true;
    public bool AudioEnabled { get; set; } = true;
    public double OverloadTimer { get; private set; }
    public double FreezeTimer { get; private set; }
    public double Combo { get; private set; } = 1.0;
    public int CritPercent => (int)Math.Round(CritChance * 100);
    public double ClickValue => ClickBase + Facilities.Sum(f => f.ClickBonus * f.Owned);
    public double DefenseRegen => Facilities.Sum(f => f.DefenseRegen * f.Owned);
    public double TacticalCooldownMultiplier => Math.Max(0.5, 1.0 - Facilities.Sum(f => f.CooldownReduction * f.Owned));

    public List<Building> Buildings { get; } = new();
    public List<Equipment> Equipment { get; } = new();
    public List<Tactical> Tacticals { get; } = new();
    public List<Facility> Facilities { get; } = new();

    public event Action? Changed;
    public event Action<double, bool>? Clicked;
    public event Action? PurchaseMade;
    public event Action? TacticalUsed;
    public event Action? GameEnded;

    public GameState()
    {
        _rng.Randomize();

        Buildings.Add(new Building
        {
            Category = ShopCategory.Building,
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
            Category = ShopCategory.Building,
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
            Category = ShopCategory.Building,
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
            Category = ShopCategory.Building,
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
            Category = ShopCategory.Building,
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
            Category = ShopCategory.Building,
            Name = "服务器农场",
            Description = "用算力换米饭，废土最奢侈的农场。",
            Icon = "res://assets/icons/shop_server.svg",
            Stat = "+120 白饭 / 秒",
            StatIcon = "res://assets/icons/icon_rice.svg",
            Accent = UiKit.Green,
            BaseCost = 130000,
            Production = 120.0,
        });

        Equipment.Add(new Equipment
        {
            Category = ShopCategory.Equipment,
            Name = "强化枪管",
            Description = "加厚枪管与复进簧，提升机枪单发伤害。",
            Icon = "res://assets/icons/icon_target.svg",
            Stat = "+8 机枪伤害 / 级",
            StatIcon = "res://assets/icons/icon_power.svg",
            Accent = UiKit.Cyan,
            BaseCost = 180,
            Growth = 1.4,
            MaxLevel = 5,
            DamageBonus = 8,
            PowerBonus = 6,
        });
        Equipment.Add(new Equipment
        {
            Category = ShopCategory.Equipment,
            Name = "冷却组件",
            Description = "水冷套件与轻量化枪机，缩短射击间隔。",
            Icon = "res://assets/icons/icon_rate.svg",
            Stat = "-0.02 秒射击间隔 / 级",
            StatIcon = "res://assets/icons/icon_fire.svg",
            Accent = UiKit.Green,
            BaseCost = 220,
            Growth = 1.4,
            MaxLevel = 5,
            FireIntervalDelta = 0.02,
            PowerBonus = 4,
        });
        Equipment.Add(new Equipment
        {
            Category = ShopCategory.Equipment,
            Name = "穿甲弹",
            Description = "对重型僵尸造成额外伤害。",
            Icon = "res://assets/icons/icon_power.svg",
            Stat = "+20% 对重型伤害 / 级",
            StatIcon = "res://assets/icons/icon_target.svg",
            Accent = UiKit.Gold,
            BaseCost = 280,
            Growth = 1.45,
            MaxLevel = 4,
            BruteBonus = 0.2,
            PowerBonus = 5,
        });
        Equipment.Add(new Equipment
        {
            Category = ShopCategory.Equipment,
            Name = "防线装甲",
            Description = "加装钢板和支撑架，提高防线上限并立即修复。",
            Icon = "res://assets/icons/icon_shield.svg",
            Stat = "+100 防线上限 / 级",
            StatIcon = "res://assets/icons/icon_shield.svg",
            Accent = UiKit.Green,
            BaseCost = 500,
            Growth = 1.5,
            MaxLevel = 5,
            DefenseBonus = 100,
            PowerBonus = 8,
        });

        Tacticals.Add(new Tactical
        {
            Category = ShopCategory.Tactical,
            Name = "防线维修",
            Description = "紧急焊接沙袋后的支撑架，恢复防线耐久。",
            Icon = "res://assets/icons/icon_shield.svg",
            Stat = "+300 防线耐久 · CD 20s",
            StatIcon = "res://assets/icons/icon_shield.svg",
            Accent = UiKit.Green,
            BaseCost = 400,
            Cooldown = 20,
            Kind = TacticalKind.Repair,
        });
        Tacticals.Add(new Tactical
        {
            Category = ShopCategory.Tactical,
            Name = "过载射击",
            Description = "短时间超频机枪，射速大幅提升。",
            Icon = "res://assets/icons/icon_fire.svg",
            Stat = "10 秒射速 +80% · CD 35s",
            StatIcon = "res://assets/icons/icon_fire.svg",
            Accent = UiKit.Gold,
            BaseCost = 250,
            Cooldown = 35,
            Kind = TacticalKind.Overload,
        });
        Facilities.Add(new Facility
        {
            Name = "医疗站",
            Description = "持续修复防线，争取更多清剿时间。",
            Icon = "res://assets/icons/icon_shield.svg",
            Stat = "每级 +2 防线/秒",
            Accent = UiKit.Green,
            BaseCost = 600,
            MaxLevel = 5,
            DefenseRegen = 2,
        });
        Facilities.Add(new Facility
        {
            Name = "战术电台",
            Description = "协调战术支援，缩短所有战术道具冷却。",
            Icon = "res://assets/icons/icon_rate.svg",
            Stat = "每级 -5% 道具冷却",
            Accent = UiKit.Cyan,
            BaseCost = 450,
            MaxLevel = 3,
            CooldownReduction = 0.05,
        });
        Facilities.Add(new Facility
        {
            Name = "训练场",
            Description = "让铁盆号保持手感，提高每次点击的白饭收益。",
            Icon = "res://assets/icons/icon_target.svg",
            Stat = "每级 +1 点击白饭",
            Accent = UiKit.Gold,
            BaseCost = 700,
            MaxLevel = 5,
            ClickBonus = 1,
        });

        Tacticals.Add(new Tactical
        {
            Category = ShopCategory.Tactical,
            Name = "急冻手雷",
            Description = "在街道上释放低温雾，短时间内拖慢尸潮。",
            Icon = "res://assets/icons/icon_flask.svg",
            Stat = "6 秒减速 85% · CD 30s",
            StatIcon = "res://assets/icons/icon_flask.svg",
            Accent = UiKit.Cyan,
            BaseCost = 200,
            Cooldown = 30,
            Kind = TacticalKind.Freeze,
        });

        Recalculate();
    }

    public void Tick(double delta)
    {
        if (GameOver || Paused)
            return;

        Rice += Production * delta;

        if (!WaveActive && WaveTimer > 0)
            WaveTimer = Math.Max(0, WaveTimer - delta);

        if (_comboTimer > 0)
        {
            _comboTimer -= delta;
            if (_comboTimer <= 0)
                Combo = 1.0;
        }

        foreach (var tactical in Tacticals)
        {
            if (tactical.CooldownLeft > 0)
                tactical.CooldownLeft = Math.Max(0, tactical.CooldownLeft - delta);
        }

        if (OverloadTimer > 0)
            OverloadTimer = Math.Max(0, OverloadTimer - delta);
        if (FreezeTimer > 0)
            FreezeTimer = Math.Max(0, FreezeTimer - delta);

        if (DefenseRegen > 0 && Defense < MaxDefense)
            Defense = Math.Min(MaxDefense, Defense + DefenseRegen * delta);

        _refreshTimer += delta;
        if (_refreshTimer >= 0.1)
        {
            _refreshTimer = 0;
            Changed?.Invoke();
        }
    }

    public void Click()
    {
        if (GameOver || Paused)
            return;

        Combo = Math.Min(ComboMax, Combo + ComboStep);
        _comboTimer = ComboWindow;

        bool crit = _rng.Randf() < CritChance;
        double gain = ClickValue * Combo * (crit ? 2.0 : 1.0);
        Rice += gain;

        Clicked?.Invoke(gain, crit);
        Changed?.Invoke();
    }

    public bool TryBuy(int index)
    {
        if (GameOver || Paused || index < 0 || index >= Buildings.Count)
            return false;

        var building = Buildings[index];
        if (Rice < building.Cost)
            return false;

        Rice -= building.Cost;
        building.Owned++;
        Recalculate();
        PurchaseMade?.Invoke();
        Changed?.Invoke();
        return true;
    }

    public bool BuyEquipment(int index)
    {
        if (GameOver || Paused || index < 0 || index >= Equipment.Count)
            return false;

        var equipment = Equipment[index];
        if (equipment.Owned >= equipment.MaxLevel || Rice < equipment.Cost)
            return false;

        Rice -= equipment.Cost;
        equipment.Owned++;
        if (equipment.DefenseBonus > 0)
            Defense = Math.Min(MaxDefense + equipment.DefenseBonus, Defense + equipment.DefenseBonus);
        Recalculate();
        PurchaseMade?.Invoke();
        Changed?.Invoke();
        return true;
    }

    public bool BuyFacility(int index)
    {
        if (GameOver || Paused || index < 0 || index >= Facilities.Count)
            return false;

        var facility = Facilities[index];
        if (facility.Owned >= facility.MaxLevel || Rice < facility.Cost)
            return false;

        Rice -= facility.Cost;
        facility.Owned++;
        PurchaseMade?.Invoke();
        Changed?.Invoke();
        return true;
    }

    public bool UseTactical(int index)
    {
        if (GameOver || Paused || index < 0 || index >= Tacticals.Count)
            return false;

        var tactical = Tacticals[index];
        if (tactical.CooldownLeft > 0 || Rice < tactical.Cost)
            return false;
        if (tactical.Kind == TacticalKind.Repair && Defense >= MaxDefense)
            return false;

        Rice -= tactical.Cost;
        tactical.CooldownLeft = tactical.Cooldown * TacticalCooldownMultiplier;

        switch (tactical.Kind)
        {
            case TacticalKind.Repair:
                Defense = Math.Min(MaxDefense, Defense + 300);
                break;
            case TacticalKind.Overload:
                OverloadTimer = 10;
                break;
            case TacticalKind.Freeze:
                FreezeTimer = 6;
                break;
        }

        TacticalUsed?.Invoke();
        Changed?.Invoke();
        return true;
    }

    public Building NextGoal()
    {
        return Buildings.FirstOrDefault(b => Rice < b.Cost) ?? Buildings[^1];
    }

    public void SetWave(int wave, bool active, double timer)
    {
        Wave = Math.Max(1, wave);
        WaveActive = active;
        WaveTimer = Math.Max(0, timer);
        Changed?.Invoke();
    }

    public void AddKill(int points, double rice)
    {
        Kills++;
        Score += Math.Max(0, points);
        Rice += Math.Max(0, rice);
        Changed?.Invoke();
    }

    public void AddWaveBonus(int points, double rice)
    {
        Score += Math.Max(0, points);
        Rice += Math.Max(0, rice);
        Changed?.Invoke();
    }

    public void DamageDefense(double damage)
    {
        if (GameOver)
            return;

        Defense = Math.Max(0, Defense - Math.Max(0, damage));
        if (Defense <= 0 && !GameOver)
        {
            GameOver = true;
            WaveActive = false;
            GameEnded?.Invoke();
        }

        Changed?.Invoke();
    }

    public void Pause()
    {
        if (GameOver || Paused)
            return;
        Paused = true;
        Changed?.Invoke();
    }

    public void Resume()
    {
        if (!Paused)
            return;
        Paused = false;
        Changed?.Invoke();
    }

    public SaveData CreateSave()
    {
        return new SaveData
        {
            Version = 2,
            SavedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Rice = Rice,
            Owned = Buildings.Select(b => b.Owned).ToArray(),
            EquipmentOwned = Equipment.Select(e => e.Owned).ToArray(),
            FacilityOwned = Facilities.Select(f => f.Owned).ToArray(),
            Defense = Defense,
            Kills = Kills,
            Score = Score,
            Wave = Wave,
            AutoSave = AutoSave,
            EffectsEnabled = EffectsEnabled,
            AudioEnabled = AudioEnabled,
        };
    }

    public void ApplySave(SaveData data)
    {
        Rice = Math.Max(0, data.Rice);
        if (data.Owned != null)
        {
            for (int i = 0; i < Buildings.Count && i < data.Owned.Length; i++)
                Buildings[i].Owned = Math.Max(0, data.Owned[i]);
        }

        if (data.EquipmentOwned != null)
        {
            for (int i = 0; i < Equipment.Count && i < data.EquipmentOwned.Length; i++)
                Equipment[i].Owned = Math.Max(0, data.EquipmentOwned[i]);
        }

        if (data.FacilityOwned != null)
        {
            for (int i = 0; i < Facilities.Count && i < data.FacilityOwned.Length; i++)
                Facilities[i].Owned = Math.Max(0, data.FacilityOwned[i]);
        }

        MaxDefense = 1000 + Equipment.Sum(e => e.DefenseBonus * e.Owned);
        Defense = Math.Clamp(data.Defense, 0, MaxDefense);
        if (Defense <= 0)
            Defense = MaxDefense;

        Kills = Math.Max(0, data.Kills);
        Score = Math.Max(0, data.Score);
        Wave = Math.Max(1, data.Wave);
        AutoSave = data.AutoSave;
        EffectsEnabled = data.EffectsEnabled;
        AudioEnabled = data.AudioEnabled;
        WaveActive = false;
        WaveTimer = 5;
        GameOver = false;
        Paused = false;
        Combo = 1;
        _comboTimer = 0;
        OverloadTimer = 0;
        FreezeTimer = 0;
        foreach (var tactical in Tacticals)
            tactical.CooldownLeft = 0;
        Recalculate();
        Changed?.Invoke();
    }

    private void Recalculate()
    {
        Production = Buildings.Sum(b => b.Production * b.Owned);
        CombatPower = 24 + Buildings.Sum(b => b.Combat * b.Owned) + Equipment.Sum(e => e.PowerBonus * e.Owned);

        double damage = 10 + CombatPower * 1.1 + Equipment.Sum(e => e.DamageBonus * e.Owned);
        GunDamage = damage;
        FireInterval = Math.Max(0.14, 0.38 - Equipment.Sum(e => e.FireIntervalDelta * e.Owned));
        BruteDamageMultiplier = 1.0 + Equipment.Sum(e => e.BruteBonus * e.Owned);

        double max = 1000 + Equipment.Sum(e => e.DefenseBonus * e.Owned);
        MaxDefense = max;
        Defense = Math.Min(Defense, MaxDefense);
    }
}
