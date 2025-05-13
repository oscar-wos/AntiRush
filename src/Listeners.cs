using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using AntiRush.Classes;
using AntiRush.Extensions;
using FixVectorLeak.src;
using FixVectorLeak.src.Structs;

namespace AntiRush;

public partial class AntiRush
{
    private void OnTick()
    {
        if (_gameRules is { WarmupPeriod: true } && !Config.Warmup)
            return;

        if (!_minPlayers || !_maxPlayers)
            return;

        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid() && p.PawnIsAlive))
        {
            if (player.PlayerPawn.Value?.AbsOrigin == null)
                continue;

            if (!_playerData.TryGetValue(player, out var playerData))
                continue;

            var doAction = false;
            Vector_t origin = player.PlayerPawn.Value.AbsOrigin.ToVector_t();
            Vector_t velocity = player.PlayerPawn.Value.AbsVelocity.ToVector_t();

            foreach (var zone in _zones)
            {
                if (((Config.NoRushTime != 0 && Config.NoRushTime + _roundStart < Server.CurrentTime) || _bombPlanted) && Config.RushZones.Contains((int)zone.Type))
                {
                    zone.Clear();
                    continue;
                }

                if (Config.NoCampTime != 0 && Config.NoCampTime + _roundStart > Server.CurrentTime && Config.CampZones.Contains((int)zone.Type))
                    continue;

                var isInZone = zone.IsInZone(origin.X, origin.Y, origin.Z);

                if (!zone.Data.TryGetValue(player, out var zoneData))
                    zoneData = new ZoneData();

                if (!zone.Teams.Contains(player.Team))
                    continue;

                if (!isInZone)
                {
                    zoneData.Entry = 0;
                    continue;
                }

                if (zone.Delay == 0)
                {
                    doAction = true;
                    DoAction(player, zone);
                    continue;
                }

                if (zoneData.Entry == 0)
                    zoneData.Entry = Server.CurrentTime;
                
                var diff = (zoneData.Entry + zone.Delay) - Server.CurrentTime;

                if (diff > 0)
                {
                    if (Math.Abs(diff) < 0.01 && diff >= 1)
                        player.PrintToChat($"{Prefix}{Localizer["delayRemaining", zone.ToString(Localizer), diff.ToString("0")]}");

                    continue;
                }

                doAction = true;
                DoAction(player, zone);
            }

            if (doAction)
                continue;

            playerData.LastPos = [origin.X, origin.Y, origin.Z];
            playerData.LastVel = [velocity.X, velocity.Y, velocity.Z];
        }

        if (Config.NoRushTime != 0 && !_bombPlanted)
        {
            var diff = (Config.NoRushTime + _roundStart) - Server.CurrentTime;

            if (diff > 0 && _countdown.Contains(diff))
                Server.PrintToChatAll($"{Prefix}{Localizer["delayRemaining", Localizer["rushDisabled"], diff.ToString("0")]}");

            else if (diff == 0)
                Server.PrintToChatAll($"{Prefix}{Localizer["rushDisabled"]}");
        }

        if (Config.NoCampTime != 0)
        {
            var diff = (Config.NoCampTime + _roundStart) - Server.CurrentTime;

            if (diff > 0 && _countdown.Contains(diff))
                Server.PrintToChatAll($"{Prefix}{Localizer["delayRemaining", Localizer["campEnabled"], diff.ToString("0")]}");

            else if (diff == 0)
                Server.PrintToChatAll($"{Prefix}{Localizer["campEnabled"]}");
        }
    }

    private void OnMapStart(string mapName)
    {
        LoadJson(mapName);
    }

    private void OnClientPutInServer(int playerSlot)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);

        if (player == null || !player.IsValid())
            return;

        _playerData[player] = new PlayerData();
    }

    private HookResult OnProcessMovement(DynamicHook h)
    {
        try
        {
            CCSPlayer_MovementServices ms = h.GetParam<CCSPlayer_MovementServices>(0);
            var player = ms.Pawn.Value.Controller.Value?.As<CCSPlayerController>();

            if (player == null || !player.IsValid() || !_playerData.TryGetValue(player, out var playerData))
                return HookResult.Continue;

            CUserCmd userCmd = new(h.GetParam<IntPtr>(_isLinux ? 1 : 2));
            var baseCmd = userCmd.GetBaseCmd();

            if (playerData.BlockButtons != 0)
            {
                if (Server.TickedTime >= playerData.BlockButtons)
                    playerData.BlockButtons = 0;
                else
                {
                    baseCmd.DisableForwardMove();
                    baseCmd.DisableSideMove();
                    baseCmd.DisableUpMove();

                    userCmd.DisableInput(h.GetParam<IntPtr>(_isLinux ? 1 : 2), 6); //disable jump (2) + duck (4) = 6
                }
            }

            return HookResult.Changed;
        }
        catch (Exception)
        {
            return HookResult.Continue;
        }
    }
}