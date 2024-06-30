using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
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
    public Dictionary<CCSPlayerController, ZoneData> Data { get; } = [];

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
}