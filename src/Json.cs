using AntiRush.Enums;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using FixVectorLeak.Structs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AntiRush;

public partial class AntiRush
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    public void LoadJson(string mapName)
    {
        _zones.Clear();
        var path = Path.Join(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", "AntiRush", $"{mapName}.json");

        if (!File.Exists(path))
            return;

        var json = File.ReadAllText(path);
        var jsonZones = JsonSerializer.Deserialize<List<JsonZone>>(json);

        if (jsonZones == null)
            return;

        foreach (var zone in jsonZones)
            _zones.Add(new Zone(
                zone.Name,
                (ZoneType)zone.Type,
                zone.Delay,
                zone.Damage,
                [.. zone.Teams.Select(t => (CsTeam)Enum.ToObject(typeof(CsTeam), t))],
                new Vector_t(Math.Min(zone.X[0], zone.Y[0]), Math.Min(zone.X[1], zone.Y[1]), Math.Min(zone.X[2], zone.Y[2])),
                new Vector_t(Math.Max(zone.X[0], zone.Y[0]), Math.Max(zone.X[1], zone.Y[1]), Math.Max(zone.X[2], zone.Y[2]))
            ));
    }

    public void SaveJson(string mapName)
    {
        var path = Path.Join(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", "AntiRush");

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        path = Path.Join(path, $"{mapName}.json");

        if (_zones.Count == 0)
            return;

        List<JsonZone> jsonZones = [];

        jsonZones.AddRange(from zone in _zones
            select new JsonZone()
            {
                Name = zone.Name,
                Type = (int)zone.Type,
                Delay = zone.Delay,
                Damage = zone.Damage,
                Teams = [.. zone.Teams.Select(team => (int)team)],
                X = [zone.MinPoint.X, zone.MinPoint.Y, zone.MinPoint.Z],
                Y = [zone.MaxPoint.X, zone.MaxPoint.Y, zone.MaxPoint.Z]
            }
        );

        var json = JsonSerializer.Serialize(jsonZones, jsonSerializerOptions);
        File.WriteAllText(path, json);
    }
}

public class JsonZone
{
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("type")] public required int Type { get; set; }
    [JsonPropertyName("delay")] public required float Delay { get; set; }
    [JsonPropertyName("damage")] public required int Damage { get; set; }
    [JsonPropertyName("teams")] public required int[] Teams { get; set; }
    [JsonPropertyName("x")] public required float[] X { get; set; }
    [JsonPropertyName("y")] public required float[] Y { get; set; }
}