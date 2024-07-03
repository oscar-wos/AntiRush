using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace AntiRush;

public class AntiRushConfig : BasePluginConfig
{
    public override int Version { get; set; } = 3;
    [JsonPropertyName("Messages")] public string Messages { get; set; } = "simple";
    [JsonPropertyName("DrawZones")] public bool DrawZones { get; set; } = false;
    [JsonPropertyName("Warmup")] public bool Warmup { get; set; } = false;
    [JsonPropertyName("NoRushTime")] public int NoRushTime { get; set; } = 0;
    [JsonPropertyName("NoCampTime")] public int NoCampTime { get; set; } = 0;
}