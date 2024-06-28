using CounterStrikeSharp.API.Modules.Utils;
using AntiRush.Enums;

namespace AntiRush;

public class PlayerData
{
    public Vector LastPosition { get; set; } = Vector.Zero;
    public Vector LastVelocity { get; set; } = Vector.Zero;
    public Vector SpawnPos { get; set; } = Vector.Zero;
    public bool Debug { get; set; }
    public bool[] DebugOptions { get; set; } = new bool[Enum.GetValues(typeof(ZoneType)).Length];
    public AddZoneMenu? AddZone { get; set; } = null;
    public float LastMessage { get; set; }
}