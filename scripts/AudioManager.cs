using Godot;
using System.Collections.Generic;

public static class AudioManager
{
    private static readonly Dictionary<string, AudioStream> Streams = new();
    private static readonly List<AudioStreamPlayer> Players = new();
    private static AudioStreamPlayer? _music;
    private static int _cursor;

    public static bool Enabled { get; private set; } = true;
    public static bool MusicEnabled { get; private set; } = true;

    public static void Init(Node host)
    {
        Streams.Clear();
        Players.Clear();
        _cursor = 0;

        foreach (var key in new[] { "click", "buy", "shot", "hit", "wave", "breach", "gameover", "tactical" })
        {
            var stream = GD.Load<AudioStream>($"res://assets/audio/{key}.wav");
            if (stream != null)
                Streams[key] = stream;
        }

        for (int i = 0; i < 12; i++)
        {
            var player = new AudioStreamPlayer
            {
                Bus = "Master",
                ProcessMode = Node.ProcessModeEnum.Always,
            };
            host.AddChild(player);
            Players.Add(player);
        }

        _music = new AudioStreamPlayer
        {
            Bus = "Master",
            ProcessMode = Node.ProcessModeEnum.Always,
            VolumeDb = -11f,
        };
        host.AddChild(_music);

        var bgm = GD.Load<AudioStream>("res://assets/audio/bgm.wav");
        if (bgm is AudioStreamWav wav)
        {
            wav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
            wav.LoopBegin = 0;
            wav.LoopEnd = wav.Data.Length / 2;
        }

        _music.Stream = bgm;
        if (MusicEnabled)
            _music.Play();
    }

    public static void SetMusicEnabled(bool enabled)
    {
        MusicEnabled = enabled;
        if (_music == null)
            return;

        if (enabled && !_music.Playing)
            _music.Play();
        else if (!enabled)
            _music.Stop();
    }

    public static void StopMusic()
    {
        _music?.Stop();
    }

    public static void SetEnabled(bool enabled)
    {
        Enabled = enabled;
        if (enabled)
            return;

        foreach (var player in Players)
            player.Stop();
    }

    public static void Play(string key, float volume = 1f)
    {
        if (!Enabled || Players.Count == 0 || !Streams.TryGetValue(key, out var stream))
            return;

        AudioStreamPlayer? player = null;
        foreach (var candidate in Players)
        {
            if (!candidate.Playing)
            {
                player = candidate;
                break;
            }
        }

        if (player == null)
        {
            player = Players[_cursor % Players.Count];
            _cursor++;
        }

        player.Stream = stream;
        player.VolumeDb = Mathf.LinearToDb(Mathf.Clamp(volume, 0.01f, 1f) * 0.7f);
        player.Play();
    }
}

