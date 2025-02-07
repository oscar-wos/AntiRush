using CounterStrikeSharp.API;

namespace AntiRush;

public partial class AntiRush
{
    private void OnTick()
    {
        if (_gameRules is { WarmupPeriod: true } && !Config.Warmup)
            return;

        if (!_minPlayers || !_maxPlayers)
            return;

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

        foreach (var controller in Utilities.GetPlayers().Where(c => c.IsValid() && c.PawnIsAlive))
        {
            if (controller.PlayerPawn.Value == null)
                continue;

            var doAction = false;

            foreach (var zone in _zones)
            {
                if (((Config.NoRushTime != 0 && Config.NoRushTime + _roundStart < Server.CurrentTime) || _bombPlanted) && Config.RushZones.Contains((int)zone.Type))
                    continue;

                if (Config.NoCampTime != 0 && Config.NoCampTime + _roundStart > Server.CurrentTime && Config.CampZones.Contains((int)zone.Type))
                    continue;

                var isInZone = zone.IsInZone(controller.PlayerPawn.Value.AbsOrigin!);

                if (!zone.Data.TryGetValue(controller, out _))
                    zone.Data[controller] = new ZoneData();

                if (!zone.Teams.Contains(controller.Team))
                    continue;

                if (!isInZone)
                {
                    zone.Data[controller].Entry = 0;
                    continue;
                }

                if (zone.Delay == 0)
                {
                    doAction = true;
                    DoAction(controller, zone);
                    continue;
                }

                if (zone.Data[controller].Entry == 0)
                    zone.Data[controller].Entry = Server.CurrentTime;
                
                var diff = (zone.Data[controller].Entry + zone.Delay) - Server.CurrentTime;

                if (diff > 0)
                {
                    var diffString = diff % 1;

                    if (diffString.ToString("0.00") is ("0.00" or "0.01") && diff >= 1)
                        controller.PrintToChat($"{Prefix}{Localizer["delayRemaining", zone.ToString(Localizer), diff.ToString("0")]}");

                    continue;
                }

                doAction = true;
                DoAction(controller, zone);
            }

            if (doAction)
                continue;

            _playerData[controller].LastPos = [controller.PlayerPawn.Value.AbsOrigin!.X, controller.PlayerPawn.Value.AbsOrigin.Y, controller.PlayerPawn.Value.AbsOrigin.Z];
            _playerData[controller].LastVel = [controller.PlayerPawn.Value.AbsVelocity.X, controller.PlayerPawn.Value.AbsVelocity.Y, controller.PlayerPawn.Value.AbsVelocity.Z];
        }
    }

    private void OnMapStart(string mapName)
    {
        LoadJson(mapName);
    }

    private void OnClientPutInServer(int playerSlot)
    {
        var controller = Utilities.GetPlayerFromSlot(playerSlot);

        if (controller == null || !controller.IsValid())
            return;

        _playerData[controller] = new PlayerData();
    }
}