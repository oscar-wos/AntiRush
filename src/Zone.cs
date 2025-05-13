using Microsoft.Extensions.Localization;
using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using AntiRush.Classes;
using AntiRush.Enums;
using FixVectorLeak.src;
using FixVectorLeak.src.Structs;

namespace AntiRush;

public class Zone
{
    public Zone(string name, ZoneType type, float delay, int damage, CsTeam[] teams, float[] minPoint, float[] maxPoint)
    {
        Name = name;
        Type = type;
        Delay = delay;
        Damage = damage;
        Teams = teams;
        MinPoint = minPoint;
        MaxPoint = maxPoint;
    }

    public Zone(float[] minPoint, float[] maxPoint)
    {
        MinPoint = minPoint;
        MaxPoint = maxPoint;
    }

    public string Name = "";
    public ZoneType Type;
    public float Delay;
    public int Damage;
    public CsTeam[] Teams = [];
    public float[] MinPoint;
    public float[] MaxPoint;
    public Dictionary<CCSPlayerController, ZoneData> Data { get; set; } = [];
    public List<CBeam> Beams { get; } = [];

    public bool IsInZone(float x, float y, float z)
    {
        return x >= MinPoint[0] && x <= MaxPoint[0] && y >= MinPoint[1] && y <= MaxPoint[1] && z >= MinPoint[2] && z <= MaxPoint[2];
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
        foreach (var beam in Beams)
        {
            if (beam == null || !beam.IsValid)
                continue;

            beam.AcceptInput("Kill");
        }

        Beams.Clear();
    }

    public void Draw()
    {
        Clear();

        if (MinPoint is null || MaxPoint is null)
            return;

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

    private CBeam? DrawBeam(Vector_t start, Vector_t end)
    {
        var beam = Utilities.CreateEntityByName<CBeam>("beam");

        if (beam == null)
            return null;

        beam.Teleport(start);
        beam.EndPos.X = end.X;
        beam.EndPos.Y = end.Y;
        beam.EndPos.Z = end.Z;
        beam.Render = GetBeamColor();
        beam.Width = 1f;
        beam.DispatchSpawn();
        Beams.Add(beam);

        return beam;
    }
}