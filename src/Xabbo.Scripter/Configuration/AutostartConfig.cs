using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Xabbo.Scripter.Configuration;

public class AutostartEntry
{
    public string FileName { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}

public class AutostartConfig
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "xabbo", "scripter", "autostart.json"
    );

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public List<AutostartEntry> Entries { get; set; } = new();

    public static AutostartConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AutostartConfig>(json, JsonOptions) ?? new AutostartConfig();
            }
        }
        catch { }

        return new AutostartConfig();
    }

    public void Save()
    {
        try
        {
            string? dir = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }

    public bool Contains(string fileName)
    {
        return Entries.Exists(e => e.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(string fileName)
    {
        if (Contains(fileName)) return;
        Entries.Add(new AutostartEntry { FileName = fileName, AddedAt = DateTime.Now });
        Save();
    }

    public void Remove(string fileName)
    {
        Entries.RemoveAll(e => e.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        Save();
    }

    public void CleanupMissing(string scriptDirectory)
    {
        int removed = Entries.RemoveAll(e => !File.Exists(Path.Combine(scriptDirectory, e.FileName)));
        if (removed > 0) Save();
    }
}
