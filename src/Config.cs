﻿using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace AntiRush;

public class AntiRushConfig : BasePluginConfig
{
    public override int Version { get; set; } = 9;
    [JsonPropertyName("Prefix")] public string Prefix { get; set; } = "{White}[{Lime}AntiRush{White}] ";
    [JsonPropertyName("Messages")] public string Messages { get; set; } = "simple";
    [JsonPropertyName("DrawZones")] public bool DrawZones { get; set; } = true;
    [JsonPropertyName("Warmup")] public bool Warmup { get; set; } = false;
    [JsonPropertyName("DisableOnBombPlant")] public bool DisableOnBombPlant { get; set; } = true;
    [JsonPropertyName("RestartOnLoad")] public bool RestartOnLoad { get; set; } = true;
    [JsonPropertyName("NoRushTime")] public int NoRushTime { get; set; } = 0;
    [JsonPropertyName("NoCampTime")] public int NoCampTime { get; set; } = 0;
    [JsonPropertyName("RushZones")] public int[] RushZones { get; set; } = [0, 2, 3, 4];
    [JsonPropertyName("CampZones")] public int[] CampZones { get; set; } = [1];
    [JsonPropertyName("Countdown")] public int[] Countdown { get; set; } = [60, 30, 15, 10, 5, 3, 2, 1];
    [JsonPropertyName("MinPlayers")] public int MinPlayers { get; set; } = 1;
    [JsonPropertyName("MaxPlayers")] public int MaxPlayers { get; set; } = 64;
}