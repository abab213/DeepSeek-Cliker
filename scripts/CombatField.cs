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

    private enum ZombieType
    {
        Walker,
        Runner,
        Brute,
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
        _spawnLeft = 8 + _wave * 2;
        _spawnInterval = Mathf.Max(0.3f, 0.55f - _wave * 0.02f);
        _spawnTimer = 0.25f;
        _fireTimer = 0f;
        _zombies.Clear();
        _state.SetWave(_wave, true, 0);
    }

    private void UpdateWave(float dt)
    {
        if (_spawnLeft > 0)
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

        if (_spawnLeft <= 0 && _zombies.Count == 0)
        {
            _state.AddWaveBonus(25 + _wave * 6, 25 + _wave * 4);
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

        var baseHp = 50f + _wave * 15f;
        var baseSpeed = 27f + _wave * 2.5f;
        var baseDamage = 55f + _wave * 8f;

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
        if (target.Type == ZombieType.Brute)
            damage *= (float)_state.BruteDamageMultiplier;
        target.Hp -= damage;

        if (target.Hp <= 0f)
        {
            _zombies.Remove(target);
            AudioManager.Play("hit", 0.65f);
            Burst(hit, new Color("59d68a"), 7);
            _state.AddKill(10 + _wave * 3, 2 + _wave * 0.5);
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
            _ => new Color("7fa36a"),
        };
        if (_state.FreezeTimer > 0)
            bodyColor = bodyColor.Lerp(new Color("7fd8ff"), 0.45f);
        var headColor = bodyColor.Lerp(UiKit.Text, 0.3f);
        var eyeColor = z.Type == ZombieType.Brute ? UiKit.Red : new Color("ffcb7a");
        var outline = new Color(0.02f, 0.04f, 0.035f, 0.9f);

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

        var barWidth = 26f;
        var barY = head.Y - 14f * s;
        DrawRect(new Rect2(x - barWidth * 0.5f, barY, barWidth, 3f), new Color(0.02f, 0.04f, 0.05f, 0.8f));
        var ratio = Mathf.Clamp(z.Hp / z.MaxHp, 0f, 1f);
        var barColor = ratio > 0.5f ? UiKit.Green : ratio > 0.25f ? UiKit.Gold : UiKit.Red;
        DrawRect(new Rect2(x - barWidth * 0.5f, barY, barWidth * ratio, 3f), barColor);
    }
}
