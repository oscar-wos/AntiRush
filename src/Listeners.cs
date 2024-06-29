using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiRush;

public partial class AntiRush
{
    private void OnTick()
    {
        foreach (var controller in Utilities.GetPlayers().Where(player => player is { IsValid: true, PawnIsAlive: true } && _playerData.ContainsKey(player)))
        {
            var bounce = false;

            foreach (var zone in _zones)
            {
                var isInZone = zone.IsInZone(controller.PlayerPawn.Value!.AbsOrigin!);

                if (!zone.Data.TryGetValue(controller, out _))
                    zone.Data[controller] = new ZoneData();

                if (!isInZone)
                {
                    zone.Data[controller].Entry = 0;
                    zone.Data[controller].Exit = Server.CurrentTime;
                    continue;
                }

                zone.Data[controller].Entry = Server.CurrentTime;

                if (!zone.Teams.Contains(controller.Team))
                    continue;

                if (zone.Delay != 0)
                {
                    var diff = (zone.Data[controller].Entry + zone.Delay) - Server.CurrentTime;

                    if (diff > 0)
                    {
                        var diffString = diff % 1;

                        if (diffString.ToString("0.00") is ("0.00" or "0.01") && diff >= 1)
                            controller.PrintToChat($"{Prefix}{Localizer["delayRemaining", FormatZoneString(zone.Type), diff.ToString("0")]}");
                    }
                    else
                        bounce = DoAction(controller, zone);

                    continue;
                }

                bounce = DoAction(controller, zone);
            }
            
            if (bounce)
                continue;

            _playerData[controller].LastPosition = new Vector(controller.PlayerPawn.Value!.AbsOrigin!.X, controller.PlayerPawn.Value.AbsOrigin.Y, controller.PlayerPawn.Value.AbsOrigin.Z);
            _playerData[controller].LastVelocity = new Vector(controller.PlayerPawn.Value.AbsVelocity.X, controller.PlayerPawn.Value.AbsVelocity.Y, controller.PlayerPawn.Value.AbsVelocity.Z);
        }
    }

    private void OnMapStart(string mapName)
    {
        LoadJson(mapName);
    }
}
