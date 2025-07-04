using AntiRush.Classes;
using AntiRush.Extensions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FixVectorLeak.Extensions;
using FixVectorLeak.Structs;

namespace AntiRush;

public partial class AntiRush
{
    private void OnTick()
    {
        if (_gameRules is { WarmupPeriod: true } && !Config.Warmup)
            return;

        if (!_minPlayers || !_maxPlayers)
            return;

        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid() && (LifeState_t)p.PlayerPawn.Value!.LifeState == LifeState_t.LIFE_ALIVE))
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

                var isInZone = zone.IsInZone(origin);

                if (!zone.Teams.Contains(player.Team))
                    continue;

                if (!zone.Entry.ContainsKey(player))
                    zone.Entry[player] = Server.CurrentTime;

                if (!isInZone)
                {
                    zone.Entry[player] = 0;
                    continue;
                }

                if (zone.Delay == 0)
                {
                    doAction = true;
                    DoAction(player, zone);
                    continue;
                }

                if (zone.Entry[player] == 0)
                    zone.Entry[player] = Server.CurrentTime;

                var diff = (zone.Entry[player] + zone.Delay) - Server.CurrentTime;

                if (diff > 0)
                {
                    var diffString = diff % 1;

                    if (diffString.ToString("0.00") is ("0.00" or "0.01") && diff >= 1)
                        player.PrintToChat($"{Prefix}{Localizer["delayRemaining", zone.ToString(Localizer), diff.ToString("0")]}");

                    continue;
                }

                doAction = true;
                DoAction(player, zone);
            }

            if (doAction)
                continue;

            playerData.LastPos = origin;
            playerData.LastVel = velocity;
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
}
