using AntiRush.Extensions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSSharpUtils.Utils;
using FixVectorLeak.Extensions;
using FixVectorLeak.Structs;

namespace AntiRush;

public partial class AntiRush
{
    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _bombPlanted = false;
        _gameRules = GameUtils.GetGameRules();
        _roundStart = Server.CurrentTime;

        var count = Utilities.GetPlayers().Where(p => p.Team is (CsTeam.CounterTerrorist or CsTeam.Terrorist)).ToList().Count;
        _minPlayers = count >= Config.MinPlayers;
        _maxPlayers = count < Config.MaxPlayers;

        foreach (var zone in _zones)
        {
            zone.Entry = [];

            if (Config.DrawZones)
                zone.Draw();
        }

        return HookResult.Continue;
    }

    private HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo info)
    {
        if (!Config.DisableOnBombPlant || !_minPlayers || !_maxPlayers)
            return HookResult.Continue;

        _bombPlanted = true;
        Server.PrintToChatAll($"{Prefix}{Localizer["rushDisabled"]}");

        return HookResult.Continue;
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (player == null || !player.IsValid() || player.PlayerPawn.Value?.AbsOrigin == null)
            return HookResult.Continue;

        if (_playerData.TryGetValue(player, out var playerData))
            playerData.SpawnPos = player.PlayerPawn.Value.AbsOrigin.ToVector_t();

        return HookResult.Continue;
    }

    private HookResult OnBulletImpact(EventBulletImpact @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (player == null || !player.IsValid() || !_playerData.TryGetValue(player, out var playerData) || playerData.AddZoneMenu == null || !Menu.IsCurrentMenu(player, playerData.AddZoneMenu))
            return HookResult.Continue;

        if (playerData.AddZoneMenu.Points[0] is not null && playerData.AddZoneMenu.Points[1] is not null)
            return HookResult.Continue;

        if (Server.CurrentTime - playerData.AddZoneMenu.LastShot < 0.1)
            return HookResult.Continue;

        playerData.AddZoneMenu.LastShot = Server.CurrentTime;

        if (playerData.AddZoneMenu.Points[0] == null)
            playerData.AddZoneMenu.Points[0] = new Vector_t(@event.X, @event.Y, @event.Z);
        else
        {
            playerData.AddZoneMenu.Points[1] = new Vector_t(@event.X, @event.Y, @event.Z);

            if (playerData.AddZoneMenu.Points[0] == null || playerData.AddZoneMenu.Points[1] == null)
                return HookResult.Continue;

            var diffX = Math.Abs(playerData.AddZoneMenu.Points[0]!.Value.X - playerData.AddZoneMenu.Points[1]!.Value.X);
            var diffY = Math.Abs(playerData.AddZoneMenu.Points[0]!.Value.Y - playerData.AddZoneMenu.Points[1]!.Value.Y);
            var diffZ = Math.Abs(playerData.AddZoneMenu.Points[0]!.Value.Z - playerData.AddZoneMenu.Points[1]!.Value.Z);

            if (diffX < 32)
            {
                playerData.AddZoneMenu.Points[0] = new Vector_t(playerData.AddZoneMenu.Points[0]!.Value.X + ((playerData.AddZoneMenu.Points[0]!.Value.X >= playerData.AddZoneMenu.Points[1]!.Value.X ? 1 : -1) * ((32 - diffX) / 2)), playerData.AddZoneMenu.Points[0]!.Value.Y, playerData.AddZoneMenu.Points[0]!.Value.Z);
                playerData.AddZoneMenu.Points[1] = new Vector_t(playerData.AddZoneMenu.Points[1]!.Value.X + ((playerData.AddZoneMenu.Points[0]!.Value.X > playerData.AddZoneMenu.Points[1]!.Value.X ? -1 : 1) * ((32 - diffX) / 2)), playerData.AddZoneMenu.Points[1]!.Value.Y, playerData.AddZoneMenu.Points[1]!.Value.Z);
            }

            if (diffY < 32)
            {
                playerData.AddZoneMenu.Points[0] = new Vector_t(playerData.AddZoneMenu.Points[0]!.Value.X, playerData.AddZoneMenu.Points[0]!.Value.Y + ((playerData.AddZoneMenu.Points[0]!.Value.Y >= playerData.AddZoneMenu.Points[1]!.Value.Y ? 1 : -1) * ((32 - diffY) / 2)), playerData.AddZoneMenu.Points[0]!.Value.Z);
                playerData.AddZoneMenu.Points[1] = new Vector_t(playerData.AddZoneMenu.Points[1]!.Value.X, playerData.AddZoneMenu.Points[1]!.Value.Y + ((playerData.AddZoneMenu.Points[0]!.Value.Y > playerData.AddZoneMenu.Points[1]!.Value.Y ? -1 : 1) * ((32 - diffY) / 2)), playerData.AddZoneMenu.Points[1]!.Value.Z);
            }

            if (diffZ < 200)
            {
                if (playerData.AddZoneMenu.Points[0]!.Value.Z >= playerData.AddZoneMenu.Points[1]!.Value.Z)
                    playerData.AddZoneMenu.Points[0] = new Vector_t(playerData.AddZoneMenu.Points[0]!.Value.X, playerData.AddZoneMenu.Points[0]!.Value.Y, playerData.AddZoneMenu.Points[0]!.Value.Z + 200 - diffZ);
                else
                    playerData.AddZoneMenu.Points[1] = new Vector_t(playerData.AddZoneMenu.Points[1]!.Value.X, playerData.AddZoneMenu.Points[1]!.Value.Y, playerData.AddZoneMenu.Points[1]!.Value.Z + 200 - diffZ);
            }

            playerData.AddZone ??= new Zone(new Vector_t(), new Vector_t());
            playerData.AddZone.MinPoint = playerData.AddZoneMenu.Points[0]!.Value;
            playerData.AddZone.MaxPoint = playerData.AddZoneMenu.Points[1]!.Value;
            playerData.AddZone.Draw();
        }

        Menu.PopMenu(player, playerData.AddZoneMenu);
        BuildAddZoneMenu(player);

        return HookResult.Continue;
    }
}