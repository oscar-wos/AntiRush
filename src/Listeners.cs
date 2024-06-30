using CounterStrikeSharp.API;

namespace AntiRush;

public partial class AntiRush
{
    private void OnTick()
    {
        foreach (var controller in Utilities.GetPlayers().Where(c => c.IsValid() && c.PawnIsAlive && _playerData.ContainsKey(c)))
        {
            foreach (var zone in _zones)
            {
                var isInZone = zone.IsInZone(controller.PlayerPawn.Value!.AbsOrigin!);

                if (!zone.Data.TryGetValue(controller, out _))
                    zone.Data[controller] = new ZoneData();

                if (!isInZone)
                {
                    zone.Data[controller].Entry = 0;

                    if (zone.Data[controller].Exit == 0)
                        zone.Data[controller].Exit = Server.CurrentTime;

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