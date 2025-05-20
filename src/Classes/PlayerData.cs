using AntiRush.Enums;
using FixVectorLeak.Structs;

namespace AntiRush.Classes;

public class PlayerData
{
    public AddZoneMenu? AddZoneMenu { get; set; } = null;
    public Zone? AddZone { get; set; } = null;
    public float LastMessage { get; set; }
    public bool Debug { get; set; }
    public bool[] DebugOptions { get; set; } = new bool[Enum.GetValues(typeof(ZoneType)).Length];
    public Vector_t? SpawnPos { get; set; } = null;
    public Vector_t? LastPos { get; set; } = null;
    public Vector_t? LastVel { get; set; } = null;
}