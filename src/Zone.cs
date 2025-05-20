using AntiRush.Enums;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FixVectorLeak.Extensions;
using FixVectorLeak.Structs;
using Microsoft.Extensions.Localization;
using System.Drawing;

namespace AntiRush;

public class Zone
{
    public Zone(string name, ZoneType type, float delay, int damage, CsTeam[] teams, Vector_t minPoint, Vector_t maxPoint)
    {
        Name = name;
        Type = type;
        Delay = delay;
        Damage = damage;
        Teams = teams;
        MinPoint = minPoint;
        MaxPoint = maxPoint;
    }

    public Zone(Vector_t minPoint, Vector_t maxPoint)
    {
        MinPoint = minPoint;
        MaxPoint = maxPoint;
    }

    public string Name { get; set; } = string.Empty;
    public ZoneType Type { get; set; }
    public float Delay { get; set; }
    public int Damage { get; set; }
    public CsTeam[] Teams { get; set; } = [];
    public Vector_t MinPoint { get; set; }
    public Vector_t MaxPoint { get; set; }
    public List<CBeam> Beams { get; } = [];
    public Dictionary<CCSPlayerController, float> Entry { get; set; } = [];

    public bool IsInZone(Vector_t pos)
    {
        return pos.X >= MinPoint.X && pos.X <= MaxPoint.X && pos.Y >= MinPoint.Y && pos.Y <= MaxPoint.Y && pos.Z + 36 >= MinPoint.Z && pos.Z + 36 <= MaxPoint.Z;
    }

    public string ToString(IStringLocalizer localize)
    {
        return Type switch
        {
            ZoneType.Bounce => $"{ChatColors.Yellow}{localize["zone.Bounce"]}{ChatColors.White}",
            ZoneType.Hurt => $"{ChatColors.Orange}{localize["zone.Hurt"]}{ChatColors.White}",
            ZoneType.Kill => $"{ChatColors.Red}{localize["zone.Kill"]}{ChatColors.White}",
            ZoneType.Teleport => $"{ChatColors.Magenta}{localize["zone.Teleport"]}{ChatColors.White}",
            ZoneType.Wall => $"{ChatColors.Blue}{localize["zone.Wall"]}{ChatColors.White}",
            _ => ""
        };
    }

    private Color GetBeamColor()
    {
        return Type switch
        {
            ZoneType.Bounce => Color.Yellow,
            ZoneType.Hurt => Color.DarkOrange,
            ZoneType.Kill => Color.Red,
            ZoneType.Teleport => Color.Magenta,
            ZoneType.Wall => Color.Blue,
            _ => Color.White
        };
    }

    public void Clear()
    {
        foreach (var beam in Beams.Where(b => b.IsValid))
            beam.AcceptInput("Kill");

        Beams.Clear();
    }

    public void Draw()
    {
        Clear();

        var points = new Vector_t[8];

        for (var i = 0; i < 8; i++)
        {
            var x = (i & 1) == 0 ? MinPoint[0] : MaxPoint[0];
            var y = (i & 2) == 0 ? MinPoint[1] : MaxPoint[1];
            var z = (i & 4) == 0 ? MinPoint[2] : MaxPoint[2];

            points[i] = new Vector_t(x, y, z);
        }

        DrawBeam(points[0], points[1]);
        DrawBeam(points[0], points[2]);
        DrawBeam(points[3], points[1]);
        DrawBeam(points[3], points[2]);

        DrawBeam(points[4], points[5]);
        DrawBeam(points[4], points[6]);
        DrawBeam(points[7], points[5]);
        DrawBeam(points[7], points[6]);

        for (var i = 0; i < 4; i++)
            DrawBeam(points[i], points[i + 4]);
    }

    private void DrawBeam(Vector_t start, Vector_t end)
    {
        var beam = Utilities.CreateEntityByName<CBeam>("beam");

        if (beam == null)
            return;

        beam.Render = GetBeamColor();
        beam.Width = 1f;

        beam.Teleport(start);
        beam.EndPos.X = end.X;
        beam.EndPos.Y = end.Y;
        beam.EndPos.Z = end.Z;
        beam.DispatchSpawn();

        Beams.Add(beam);
        return;
    }
}