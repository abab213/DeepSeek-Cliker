using Godot;
using System;
using System.Collections.Generic;

public partial class CombatField : Control
{
    private const float SpawnY = 200f;
    private const float DefenseLine = 540f;
    private static readonly Vector2 Muzzle = new(245f, 548f);

    private readonly GameState _state;
    private readonly RandomNumberGenerator _rng = new();
    private readonly List<Zombie> _zombies = new();
    private readonly List<Tracer> _tracers = new();
    private readonly List<Particle> _particles = new();

    private float _time;
    private float _spawnTimer;
    private float _fireTimer;
    private float _muzzleFlash;
    private float _hitFlash;
    private int _spawnLeft;
    private int _wave = 1;
    private float _spawnInterval = 1.0f;
    private bool _bossWave;
    private bool _bossPending;
    private float _bossBanner;

    private enum ZombieType
    {
        Walker,
        Runner,
        Brute,
        Boss,
    }

    private sealed class Zombie
    {
        public float X;
        public float Y;
        public float Hp;
        public float MaxHp;
        public float Speed;
        public float Phase;
        public float Scale = 1f;
        public int Damage;
        public ZombieType Type;
    }

    private sealed class Tracer
    {
        public Vector2 From;
        public Vector2 To;
        public float Life;
        public float MaxLife;
    }

    private sealed class Particle
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float Life;
        public float MaxLife;
        public float Size;
        public Color Color;
    }

    public CombatField(GameState state)
    {
        _state = state;
        _rng.Randomize();
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = true;
    }

    public override void _Process(double delta)
    {
        if (_state.Paused)
            return;

        var dt = (float)delta;
        _time += dt;
        _muzzleFlash = Mathf.MoveToward(_muzzleFlash, 0f, dt * 9f);
        _hitFlash = Mathf.MoveToward(_hitFlash, 0f, dt * 3f);
        _bossBanner = Mathf.Max(0f, _bossBanner - dt);

        UpdateEffects(dt);

        if (!_state.GameOver)
        {
            if (_state.WaveActive)
                UpdateWave(dt);
            else if (_state.WaveTimer <= 0)
                StartWave();
        }

        QueueRedraw();
    }

    private void StartWave()
    {
        AudioManager.Play("wave", 0.8f);
        _wave = _state.Wave;
        _bossWave = _wave % 10 == 0;
        _bossPending = _bossWave;
        _bossBanner = _bossWave ? 3.2f : 0f;
        _spawnLeft = _bossWave ? 4 + _wave / 10 : WaveCount(_wave);
        _spawnInterval = Mathf.Max(0.24f, 0.9f - _wave * 0.045f);
        _spawnTimer = _bossWave ? 0.9f : 0.25f;
        _fireTimer = 0f;
        _zombies.Clear();
        _state.SetWave(_wave, true, 0);
        if (_bossWave)
            AudioManager.Play("wave", 1f);
    }

    private void UpdateWave(float dt)
    {
        if (_bossPending)
        {
            _spawnTimer -= dt;
            if (_spawnTimer <= 0f)
            {
                SpawnBoss();
                _bossPending = false;
                _spawnTimer = 0.9f;
            }
        }
        else if (_spawnLeft > 0)
        {
            _spawnTimer -= dt;
            while (_spawnLeft > 0 && _spawnTimer <= 0f)
            {
                SpawnZombie();
                _spawnLeft--;
                _spawnTimer += _spawnInterval;
            }
        }

        for (int i = _zombies.Count - 1; i >= 0; i--)
        {
            var zombie = _zombies[i];
            var slow = _state.FreezeTimer > 0 ? 0.15f : 1f;
            zombie.Y += zombie.Speed * slow * dt;
            zombie.Phase += dt * 7f;

            if (zombie.Y >= DefenseLine)
            {
                _state.DamageDefense(zombie.Damage);
                AudioManager.Play("breach", 0.85f);
                _hitFlash = _state.EffectsEnabled ? 1f : 0f;
                Burst(new Vector2(zombie.X, DefenseLine), UiKit.Red, 8);
                _zombies.RemoveAt(i);
            }
        }

        _fireTimer -= dt;
        if (_fireTimer <= 0f && _zombies.Count > 0)
            FireAtNearest();

        if (!_bossPending && _spawnLeft <= 0 && _zombies.Count == 0)
        {
            _state.AddWaveBonus(30 + _wave * 12, 30 + _wave * 10);
            _state.SetWave(_wave + 1, false, 7);
            _tracers.Clear();
        }
    }

    private void SpawnZombie()
    {
        var roll = _rng.Randf();
        var type = ZombieType.Walker;
        if (_wave >= 3 && roll < 0.25f)
            type = ZombieType.Brute;
        else if (_wave >= 2 && roll < 0.55f)
            type = ZombieType.Runner;

        var baseHp = NormalHp(_wave);
        var baseSpeed = NormalSpeed(_wave);
        var baseDamage = NormalDamage(_wave);

        var zombie = new Zombie
        {
            X = _rng.RandfRange(52f, 448f),
            Y = SpawnY + _rng.RandfRange(-8f, 8f),
            Phase = _rng.RandfRange(0f, Mathf.Tau),
            Type = type,
        };

        switch (type)
        {
            case ZombieType.Runner:
                zombie.MaxHp = baseHp * 0.7f;
                zombie.Speed = baseSpeed * 1.65f;
                zombie.Damage = (int)(baseDamage * 0.75f);
                zombie.Scale = 0.9f;
                break;
            case ZombieType.Brute:
                zombie.MaxHp = baseHp * 2.3f;
                zombie.Speed = baseSpeed * 0.72f;
                zombie.Damage = (int)(baseDamage * 1.9f);
                zombie.Scale = 1.35f;
                break;
            default:
                zombie.MaxHp = baseHp;
                zombie.Speed = baseSpeed;
                zombie.Damage = (int)baseDamage;
                break;
        }

        zombie.Hp = zombie.MaxHp;
        _zombies.Add(zombie);
    }

    private static int WaveCount(int wave) => Math.Min(90, 6 + wave * 2 + wave * wave / 30);

    private static float NormalHp(int wave) => 45f + wave * 15f + wave * wave * 0.7f;

    private static float NormalSpeed(int wave) => 24f + wave * 2.4f;

    private static float NormalDamage(int wave) => 50f + wave * 7f + wave * wave * 0.08f;

    private void SpawnBoss()
    {
        var hp = NormalHp(_wave) * (12f + _wave * 0.35f);
        var boss = new Zombie
        {
            X = _rng.RandfRange(170f, 330f),
            Y = SpawnY - 12f,
            Hp = hp,
            MaxHp = hp,
            Speed = 9f + _wave * 0.3f,
            Phase = _rng.RandfRange(0f, Mathf.Tau),
            Scale = 2.25f,
            Damage = (int)(NormalDamage(_wave) * 4f),
            Type = ZombieType.Boss,
        };
        _zombies.Add(boss);
        Burst(new Vector2(boss.X, boss.Y), UiKit.Red, 24);
    }

    private void FireAtNearest()
    {
        Zombie? target = null;
        foreach (var zombie in _zombies)
        {
            if (target == null || zombie.Y > target.Y)
                target = zombie;
        }

        if (target == null)
            return;

        _fireTimer = (float)_state.FireInterval * (_state.OverloadTimer > 0 ? 0.55f : 1f);
        AudioManager.Play("shot", 0.55f);
        var hit = new Vector2(target.X, target.Y - 38f * target.Scale);

        if (_state.EffectsEnabled)
        {
            _muzzleFlash = 1f;
            _tracers.Add(new Tracer
            {
                From = Muzzle,
                To = hit,
                Life = 0.09f,
                MaxLife = 0.09f,
            });
        }

        var damage = (float)_state.GunDamage;
        if (target.Type is ZombieType.Brute or ZombieType.Boss)
            damage *= (float)_state.BruteDamageMultiplier;
        target.Hp -= damage;

        if (target.Hp <= 0f)
        {
            _zombies.Remove(target);
            AudioManager.Play("hit", target.Type == ZombieType.Boss ? 1f : 0.65f);
            if (target.Type == ZombieType.Boss)
            {
                Burst(hit, UiKit.Gold, 28);
                _state.AddKill(300 + _wave * 50, 120 + _wave * 25);
            }
            else
            {
                Burst(hit, new Color("59d68a"), 7);
                _state.AddKill(8 + _wave * 4, 2 + _wave * 0.8);
            }
        }
        else
        {
            Burst(hit, UiKit.Alpha(UiKit.Gold, 0.8f), 2);
        }
    }

    private void Burst(Vector2 pos, Color color, int count)
    {
        if (!_state.EffectsEnabled)
            return;

        for (int i = 0; i < count; i++)
        {
            var angle = _rng.RandfRange(0f, Mathf.Tau);
            var speed = _rng.RandfRange(18f, 74f);
            _particles.Add(new Particle
            {
                Pos = pos,
                Vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed,
                Life = _rng.RandfRange(0.18f, 0.42f),
                MaxLife = 0.42f,
                Size = _rng.RandfRange(1.2f, 2.6f),
                Color = color,
            });
        }
    }

    private void UpdateEffects(float dt)
    {
        for (int i = _tracers.Count - 1; i >= 0; i--)
        {
            _tracers[i].Life -= dt;
            if (_tracers[i].Life <= 0)
                _tracers.RemoveAt(i);
        }

        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            p.Life -= dt;
            p.Pos += p.Vel * dt;
            p.Vel *= 0.96f;
            if (p.Life <= 0)
                _particles.RemoveAt(i);
        }
    }

    public override void _Draw()
    {
        DrawSpawnLine();
        DrawBossHud();

        var sorted = new List<Zombie>(_zombies);
        sorted.Sort((a, b) => a.Y.CompareTo(b.Y));
        foreach (var zombie in sorted)
            DrawZombie(zombie);

        foreach (var tracer in _tracers)
        {
            var alpha = Mathf.Clamp(tracer.Life / tracer.MaxLife, 0f, 1f);
            DrawLine(tracer.From, tracer.To, UiKit.Alpha(UiKit.Gold, 0.18f * alpha), 5f, true);
            DrawLine(tracer.From, tracer.To, UiKit.Alpha(UiKit.Cyan, 0.9f * alpha), 1.8f, true);
        }

        if (_muzzleFlash > 0)
        {
            var radius = 4f + _muzzleFlash * 9f;
            DrawCircle(Muzzle, radius, UiKit.Alpha(UiKit.Gold, 0.22f * _muzzleFlash));
            DrawCircle(Muzzle, radius * 0.45f, UiKit.Alpha(UiKit.Text, 0.8f * _muzzleFlash));
        }

        foreach (var p in _particles)
        {
            var alpha = Mathf.Clamp(p.Life / p.MaxLife, 0f, 1f);
            DrawCircle(p.Pos, p.Size, UiKit.Alpha(p.Color, alpha));
        }

        if (_hitFlash > 0)
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), UiKit.Alpha(UiKit.Red, 0.1f * _hitFlash));
    }

    private void DrawBossHud()
    {
        Zombie? boss = null;
        foreach (var zombie in _zombies)
        {
            if (zombie.Type == ZombieType.Boss)
            {
                boss = zombie;
                break;
            }
        }

        if (boss != null)
        {
            const float x = 274f;
            const float y = 18f;
            const float w = 212f;
            const float h = 44f;
            DrawRect(new Rect2(x, y, w, h), new Color(0.01f, 0.02f, 0.035f, 0.9f));
            DrawRect(new Rect2(x, y, w, 1f), UiKit.Alpha(UiKit.Red, 0.8f));
            DrawString(UiKit.DisplayFont, new Vector2(x + 10, y + 13), "BOSS // 暴君",
                HorizontalAlignment.Left, w - 20, 8, UiKit.Gold);

            var ratio = Mathf.Clamp(boss.Hp / boss.MaxHp, 0f, 1f);
            var bar = new Rect2(x + 10, y + 20, w - 20, 4);
            DrawRect(bar, new Color(0, 0, 0, 0.72f));
            DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * ratio, bar.Size.Y)), UiKit.Red);
            DrawString(UiKit.BodyFont, new Vector2(x + 10, y + 36),
                $"{boss.Hp:N0} / {boss.MaxHp:N0}",
                HorizontalAlignment.Left, w - 20, 7, UiKit.Alpha(UiKit.Text, 0.8f));
        }

        if (_bossBanner > 0f)
        {
            var alpha = Mathf.Clamp(_bossBanner, 0f, 1f);
            DrawRect(new Rect2(85, 280, 330, 40), new Color(0.04f, 0.01f, 0.02f, 0.8f * alpha));
            DrawRect(new Rect2(85, 280, 330, 1f), UiKit.Alpha(UiKit.Red, 0.8f * alpha));
            DrawString(UiKit.BoldFont, new Vector2(85, 305), "BOSS WAVE // 暴君",
                HorizontalAlignment.Center, 330, 11, UiKit.Alpha(UiKit.Red, alpha));
        }
    }

    private void DrawSpawnLine()
    {
        var pulse = 0.35f + 0.15f * Mathf.Sin(_time * 2.4f);
        DrawLine(new Vector2(18, SpawnY - 18), new Vector2(Size.X - 18, SpawnY - 18), UiKit.Alpha(UiKit.Red, 0.16f * pulse + 0.06f), 1f, true);
        for (int i = 0; i < 5; i++)
        {
            var x = 66f + i * 92f;
            DrawLine(new Vector2(x - 5, SpawnY - 22), new Vector2(x + 5, SpawnY - 22), UiKit.Alpha(UiKit.Red, 0.55f), 1f, true);
        }
    }

    private void DrawZombie(Zombie z)
    {
        var s = z.Scale;
        var bob = Mathf.Sin(z.Phase) * 2f * s;
        var x = z.X;
        var feet = z.Y;
        var bodyColor = z.Type switch
        {
            ZombieType.Runner => new Color("b3a35a"),
            ZombieType.Brute => new Color("a85a5a"),
            ZombieType.Boss => new Color("7d2d42"),
            _ => new Color("7fa36a"),
        };
        if (_state.FreezeTimer > 0)
            bodyColor = bodyColor.Lerp(new Color("7fd8ff"), 0.45f);
        var headColor = bodyColor.Lerp(UiKit.Text, 0.3f);
        var eyeColor = z.Type is ZombieType.Brute or ZombieType.Boss ? UiKit.Red : new Color("ffcb7a");
        var outline = new Color(0.02f, 0.04f, 0.035f, 0.9f);

        if (z.Type == ZombieType.Boss)
            DrawCircle(new Vector2(x, feet - 24f * s), 22f * s, UiKit.Alpha(UiKit.Red, 0.09f));

        DrawCircle(new Vector2(x, feet + 1), 10f * s, new Color(0, 0, 0, 0.42f));

        var shoulder = new Vector2(x, feet - 35f * s + bob);
        var head = new Vector2(x, feet - 43f * s + bob);

        var swing = Mathf.Sin(z.Phase) * 5f * s;
        var legL = new Vector2(x - 6f * s, feet - 18f * s);
        var legR = new Vector2(x + 6f * s, feet - 18f * s);
        var footL = new Vector2(x - 6f * s + swing, feet);
        var footR = new Vector2(x + 6f * s - swing, feet);
        DrawLine(legL, footL, outline, 6.5f * s, true);
        DrawLine(legR, footR, outline, 6.5f * s, true);
        DrawLine(legL, footL, bodyColor, 4f * s, true);
        DrawLine(legR, footR, bodyColor, 4f * s, true);

        DrawRect(new Rect2(x - 8.5f * s, shoulder.Y + 0.5f, 17f * s, 23f * s), outline);
        DrawRect(new Rect2(x - 7f * s, shoulder.Y + 2f, 14f * s, 20f * s), bodyColor);

        var armL = new Vector2(x - 15f * s + swing * 0.6f, shoulder.Y + 21f);
        var armR = new Vector2(x + 15f * s - swing * 0.6f, shoulder.Y + 21f);
        DrawLine(new Vector2(x - 7f * s, shoulder.Y + 7f), armL, outline, 5.5f * s, true);
        DrawLine(new Vector2(x + 7f * s, shoulder.Y + 7f), armR, outline, 5.5f * s, true);
        DrawLine(new Vector2(x - 7f * s, shoulder.Y + 7f), armL, bodyColor, 3.2f * s, true);
        DrawLine(new Vector2(x + 7f * s, shoulder.Y + 7f), armR, bodyColor, 3.2f * s, true);

        DrawCircle(head, 8f * s, outline);
        DrawCircle(head, 6.8f * s, headColor);
        DrawCircle(head + new Vector2(-3f * s, 1f), 1.4f * s, eyeColor);
        DrawCircle(head + new Vector2(3f * s, 1f), 1.4f * s, eyeColor);

        if (z.Type == ZombieType.Boss)
        {
            var crown = head + new Vector2(0, -10f * s);
            var gold = UiKit.Gold;
            DrawLine(crown + new Vector2(-8f * s, 0), crown + new Vector2(-4f * s, -6f * s), gold, 2f, true);
            DrawLine(crown + new Vector2(-4f * s, -6f * s), crown, gold, 2f, true);
            DrawLine(crown, crown + new Vector2(4f * s, -6f * s), gold, 2f, true);
            DrawLine(crown + new Vector2(4f * s, -6f * s), crown + new Vector2(8f * s, 0), gold, 2f, true);
        }

        var barWidth = z.Type == ZombieType.Boss ? 62f : 26f;
        var barY = head.Y - 14f * s;
        DrawRect(new Rect2(x - barWidth * 0.5f, barY, barWidth, 3f), new Color(0.02f, 0.04f, 0.05f, 0.8f));
        var ratio = Mathf.Clamp(z.Hp / z.MaxHp, 0f, 1f);
        var barColor = ratio > 0.5f ? UiKit.Green : ratio > 0.25f ? UiKit.Gold : UiKit.Red;
        DrawRect(new Rect2(x - barWidth * 0.5f, barY, barWidth * ratio, 3f), barColor);
    }
}
