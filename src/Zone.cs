using Microsoft.Extensions.Localization;
using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using AntiRush.Enums;

namespace AntiRush;

public class Zone(string name, ZoneType type, float delay, int damage, CsTeam[] teams, Vector minPoint, Vector maxPoint)
{
    public string Name { get; init; } = name;
    public ZoneType Type { get; init; } = type;
    public float Delay { get; init; } = delay;
    public int Damage { get; init; } = damage;
    public CsTeam[] Teams { get; init; } = teams;
    public Vector MinPoint { get; init; } = minPoint;
    public Vector MaxPoint { get; init; } = maxPoint;
    public Dictionary<CCSPlayerController, ZoneData> Data { get; set; } = [];
    public CBeam[] Beams { get; } = [];

    public bool IsInZone(Vector point)
    {
        return point.X >= MinPoint.X && point.X <= MaxPoint.X && point.Y >= MinPoint.Y && point.Y <= MaxPoint.Y && point.Z + 36 >= MinPoint.Z && point.Z + 36 <= MaxPoint.Z;
    }

    public string ToString(IStringLocalizer localize)
    {
        return Type switch
        {
            ZoneType.Bounce => $"{ChatColors.Yellow}{localize["zone.Bounce"]}{ChatColors.White}",
            ZoneType.Hurt => $"{ChatColors.Orange}{localize["zone.Hurt"]}{ChatColors.White}",
            ZoneType.Kill => $"{ChatColors.Red}{localize["zone.Kill"]}{ChatColors.White}",
            ZoneType.Teleport => $"{ChatColors.Magenta}{localize["zone.Teleport"]}{ChatColors.White}",
            _ => ""
        };
    }

    public void Draw()
    {
        if (MinPoint.IsZero() || MaxPoint.IsZero())
            return;

        var points = new Vector[8];

        for (var i = 0; i < 8; i++)
        {
            var x = (i & 1) == 0 ? MinPoint.X : MaxPoint.X;
            var y = (i & 2) == 0 ? MinPoint.Y : MaxPoint.Y;
            var z = (i & 4) == 0 ? MinPoint.Z : MaxPoint.Z;
            
            points[i] = new Vector(x, y, z);
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

        return;

        CBeam? DrawBeam(Vector start, Vector end)
        {
            var beam = Utilities.CreateEntityByName<CBeam>("beam");

            if (beam == null)
                return null;

            beam.Teleport(start, QAngle.Zero, Vector.Zero);
            beam.EndPos.Add(end);
            beam.Render = GetBeamColor();
            beam.Width = 1f;
            beam.DispatchSpawn();

            return beam;
        }
    }

    private Color GetBeamColor()
    {
        return Type switch
        {
            ZoneType.Bounce => Color.Yellow,
            ZoneType.Hurt => Color.DarkOrange,
            ZoneType.Kill => Color.Red,
            ZoneType.Teleport => Color.Magenta,
            _ => Color.White
        };
    }
}