using System.Text.Json;
using CounterStrikeSharp.API.Modules.Utils;
using AntiRush.Enums;

namespace AntiRush;

public partial class AntiRush
{
    public void LoadJson(string mapName)
    {
        _zones.Clear();
        var path = $"../../csgo/addons/counterstrikesharp/configs/plugins/AntiRush/{mapName}.json";

        if (!File.Exists(path))
            return;

        var json = File.ReadAllText(path);
        var jsonZones = JsonSerializer.Deserialize<List<JsonZone>>(json);

        if (jsonZones == null)
            return;

        foreach (var zone in jsonZones)
            _zones.Add(new Zone(
                zone.name,
                (ZoneType)zone.type,
                zone.delay,
                zone.damage,
                zone.teams.Select(t => (CsTeam)Enum.ToObject(typeof(CsTeam), t)).ToArray(),
                new Vector(Math.Min(zone.x[0], zone.y[0]), Math.Min(zone.x[1], zone.y[1]), Math.Min(zone.x[2], zone.y[2])),
                new Vector(Math.Max(zone.x[0], zone.y[0]), Math.Max(zone.x[1], zone.y[1]), Math.Max(zone.x[2], zone.y[2]))
            ));
    }

    public void SaveJson(string mapName)
    {
        var path = "../../csgo/addons/counterstrikesharp/configs/plugins/AntiRush/";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        path += $"{mapName}.json";

        if (_zones.Count == 0)
            return;

        List<JsonZone> jsonZones = [];

        jsonZones.AddRange(from zone in _zones
            select new JsonZone()
            {
                name = zone.Name,
                type = (int)zone.Type,
                delay = zone.Delay,
                damage = zone.Damage,
                teams = zone.Teams.Select(team => (int)team).ToArray(),
                x = [zone.MinPoint.X, zone.MinPoint.Y, zone.MinPoint.Z],
                y = [zone.MaxPoint.X, zone.MaxPoint.Y, zone.MaxPoint.Z]
            }
        );

        var json = JsonSerializer.Serialize(jsonZones, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}

public class JsonZone
{
    public required string name { get; set; }
    public required int type { get; set; }
    public required float delay { get; set; }
    public required int damage { get; set; }
    public required int[] teams { get; set; }
    public required float[] x { get; set; }
    public required float[] y { get; set; }
}