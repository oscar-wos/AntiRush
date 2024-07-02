using CounterStrikeSharp.API;
using AntiRush.Enums;

namespace AntiRush;

public partial class AntiRush
{
    private void OnTick()
    {
        var announceNoRush = false;
        var announceNoCamp = false;

        if (Config.NoRushTime != 0 && Math.Abs(Config.NoRushTime + _roundStart - Server.CurrentTime).ToString("0.00") == "0.00")
            announceNoRush = true;

        if (Config.NoCampTime != 0 && Math.Abs(Config.NoCampTime + _roundStart - Server.CurrentTime).ToString("0.00") == "0.00")
            announceNoCamp = true;

        if (announceNoRush || announceNoCamp)
        {
        }

        foreach (var controller in Utilities.GetPlayers().Where(c => c.IsValid() && c.PawnIsAlive))
        {
            foreach (var zone in _zones)
            {
                if (Config.NoRushTime != 0 && Config.NoRushTime + _roundStart < Server.CurrentTime && zone.Type is (ZoneType.Bounce or ZoneType.Teleport))
                    continue;

                if (Config.NoCampTime != 0 && Config.NoCampTime + _roundStart > Server.CurrentTime && zone.Type is ZoneType.Hurt)
                    continue;

                var isInZone = zone.IsInZone(controller.PlayerPawn.Value!.AbsOrigin!);

                if (!zone.Data.TryGetValue(controller, out _))
                    zone.Data[controller] = new ZoneData();

                if (!isInZone)
                {
                    if (zone.Data[controller].Entry != 0)
                    {
                        zone.Data[controller].Entry = 0;
                        zone.Data[controller].Exit = Server.CurrentTime;
                    }
                    
                    continue;
                }

                if (zone.Data[controller].Entry == 0)
                {
                    zone.Data[controller].Entry = Server.CurrentTime;
                    zone.Data[controller].Exit = 0;
                }

                if (!zone.Teams.Contains(controller.Team))
                    continue;

                if (zone.Delay != 0)
                {
                    var diff = (zone.Data[controller].Entry + zone.Delay) - Server.CurrentTime;

                    if (diff > 0)
                    {
                        var diffString = diff % 1;

                        if (diffString.ToString("0.00") is ("0.00" or "0.01") && diff >= 1)
                            controller.PrintToChat($"{Prefix}{Localizer["delayRemaining", zone.ToString(Localizer), diff.ToString("0")]}");
                    }
                    else
                        DoAction(controller, zone);

                    continue;
                }

                DoAction(controller, zone);
            }
        }
    }

    private void OnMapStart(string mapName)
    {
        LoadJson(mapName);
    }
}