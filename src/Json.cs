using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace AntiRush;

public partial class AntiRush
{
    public void LoadJson(string mapName)
    {
        _zones.Clear();
        var path = $"../../csgo/addons/counterstrikesharp/configs/plugins/Zones/{mapName}.json";

        if (!File.Exists(path))
            return;
        /*
        var json = File.ReadAllText(path);
        var obj = JsonSerializer.Deserialize<JsonBombsite>(json);

        foreach (var zone in obj!.a)
            AddZone(Bombsite.A, zone);

        foreach (var zone in obj!.b)
            AddZone(Bombsite.B, zone);

        return;

        void AddZone(Bombsite bombsite, JsonZone zone)
        {
            _zones.Add(new Zone(
                (ZoneType)zone.type,
                zone.teams.Select(t => (CsTeam)Enum.ToObject(typeof(CsTeam), t)).ToArray(),
                new Vector(Math.Min(zone.x[0], zone.y[0]), Math.Min(zone.x[1], zone.y[1]), Math.Min(zone.x[2], zone.y[2])),
                new Vector(Math.Max(zone.x[0], zone.y[0]), Math.Max(zone.x[1], zone.y[1]), Math.Max(zone.x[2], zone.y[2]))
            ));
        }
        */
    }

    public void SaveJson(string mapName)
    {
        var path = $"../../csgo/addons/counterstrikesharp/configs/plugins/Zones/{mapName}.json";

        if (!File.Exists(path))
        {
            Logger.LogError($"File {path} does not exist.");
            return;
        }
    }
}

public class JsonZone
{
    public required int type { get; set; }
    public required int[] teams { get; set; }
    public required float[] x { get; set; }
    public required float[] y { get; set; }
}