using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public sealed class SaveData
{
    public int Version { get; set; } = 1;
    public string SavedAt { get; set; } = "";
    public double Rice { get; set; }
    public int[] Owned { get; set; } = Array.Empty<int>();
    public int[] EquipmentOwned { get; set; } = Array.Empty<int>();
    public int[] FacilityOwned { get; set; } = Array.Empty<int>();
    public double Defense { get; set; }
    public int Kills { get; set; }
    public int Score { get; set; }
    public int Wave { get; set; } = 1;
    public bool AutoSave { get; set; } = true;
    public bool EffectsEnabled { get; set; } = true;
    public bool AudioEnabled { get; set; } = true;
    public bool MusicEnabled { get; set; } = true;
    public float SfxVolume { get; set; } = 0.7f;
    public float MusicVolume { get; set; } = 0.35f;
}

public static class SaveService
{
    private const string UserSave = "user://click_deepseek_save.json";
    private const string ResSave = "res://savegame.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public static string LastError { get; private set; } = "";

    private static string[] SavePaths()
    {
        var paths = new List<string>();
        var home = System.Environment.GetEnvironmentVariable("CLICKDEEPSEEK_HOME");
        if (!string.IsNullOrWhiteSpace(home))
            paths.Add(Path.Combine(home, "savegame.json"));
        var portable = PortableSavePath();
        if (portable != null)
            paths.Add(portable);
        paths.Add(ProjectSettings.GlobalizePath(ResSave));
        paths.Add(ProjectSettings.GlobalizePath(UserSave));
        return paths.ToArray();
    }

    /// <summary>
    /// 发布版的存档位置：可执行文件同级的 save/ 目录。
    /// 目录只读（如装在 Program Files）时会写失败，自动回退到下一条路径。
    /// 在编辑器里调试时不启用，避免把存档写进引擎目录。
    /// </summary>
    private static string? PortableSavePath()
    {
        try
        {
            var exe = OS.GetExecutablePath();
            if (string.IsNullOrWhiteSpace(exe))
                return null;
            var name = Path.GetFileNameWithoutExtension(exe);
            if (name.StartsWith("Godot", StringComparison.OrdinalIgnoreCase))
                return null;
            var dir = Path.GetDirectoryName(exe);
            return string.IsNullOrEmpty(dir) ? null : Path.Combine(dir, "save", "savegame.json");
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

    public static bool Save(GameState state)
    {
        LastError = "";
        var json = JsonSerializer.Serialize(state.CreateSave(), Options);

        foreach (var path in SavePaths())
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
        }

        return false;
    }

    public static SaveData? Load()
    {
        foreach (var path in SavePaths())
        {
            try
            {
                if (!File.Exists(path))
                    continue;
                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<SaveData>(json, Options);
                if (data != null)
                    return data;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
        }

        return null;
    }

    public static bool HasSave()
    {
        foreach (var path in SavePaths())
            if (File.Exists(path))
                return true;
        return false;
    }

    public static void Clear()
    {
        foreach (var path in SavePaths())
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
        }
    }
}
