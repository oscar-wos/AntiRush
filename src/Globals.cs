using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiRush;

[MinimumApiVersion(245)]
public partial class AntiRush
{
    public override string ModuleName => "Anti-Rush";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "https://github.com/oscar-wos/Anti-Rush";
    public Menu.Menu Menu { get; } = new();

    private string Prefix { get; } = $"[{ChatColors.Lime}Anti-Rush{ChatColors.White}] ";
    private readonly List<Zone> _zones = [];
    private readonly Dictionary<CCSPlayerController, PlayerData> _playerData = [];
}