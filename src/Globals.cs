using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using AntiRush.Classes;

namespace AntiRush;

[MinimumApiVersion(304)]
public partial class AntiRush
{
    public override string ModuleName => "AntiRush";
    public override string ModuleVersion => "2.0.0";
    public override string ModuleAuthor => "https://github.com/oscar-wos/AntiRush";
    public AntiRushConfig Config { get; set; } = new();
    public Menu.Menu Menu { get; } = new();
    public MemoryFunctionVoid<CCSPlayer_MovementServices, IntPtr>? _ProcessMovement;

    private string Prefix { get; set; } = "";
    private readonly Dictionary<CCSPlayerController, PlayerData> _playerData = [];
    private readonly List<Zone> _zones = [];
    private float _roundStart;
    private bool _bombPlanted;
    private float[] _countdown = [];
    private CCSGameRules? _gameRules;
    private bool _minPlayers;
    private bool _maxPlayers;
    private bool _isLinux;
}