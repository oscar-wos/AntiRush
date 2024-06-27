using System.Diagnostics;
using AntiRush.Enums;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiRush;

public partial class AntiRush
{
    private void OnTick()
    {
        foreach (var controller in Utilities.GetPlayers().Where(player => player is { IsValid: true, PawnIsAlive: true }))
        {
            var pos = controller!.PlayerPawn.Value!.AbsOrigin!;
            var bounce = false;

            foreach (var zone in _zones)
            {
                var isInZone = zone.IsInZone(pos);

                if (!isInZone)
                {
                    zone.Entry.Remove(controller);
                    continue;
                }

                if (!zone.Teams.Contains(controller.Team))
                    continue;

                if (zone.Delay != 0 && !zone.Entry.ContainsKey(controller))
                    zone.Entry[controller] = Server.CurrentTime;

                if (zone.Delay != 0)
                {
                    var diff = (zone.Entry[controller] + zone.Delay) - Server.CurrentTime;

                    if (diff > 0)
                    {
                        if (diff % 1 == 0)
                            controller!.PrintToChat($"{Prefix} {FormatZoneString(zone.Type)} in {diff} seconds");
                    }
                    else
                        DoAction(controller, zone);

                    continue;
                }

                DoAction(controller, zone);
            }

            if (bounce)
                continue;
            
            _playerData[controller].LastPosition = controller!.PlayerPawn.Value!.AbsOrigin!;
            _playerData[controller].LastVelocity = controller.PlayerPawn.Value.AbsVelocity;
        }
    }

    private void DoAction(CCSPlayerController controller, Zone zone)
    {
        switch (zone.Type)
        {
            case ZoneType.Bounce:
                BouncePlayer(controller);
                break;

            case ZoneType.Hurt:
                if (Server.CurrentTime % 1 == 0)
                {
                    controller.PlayerPawn.Value!.Health -= zone.Damage;
                    Utilities.SetStateChanged(controller.PlayerPawn.Value, "CBaseEntity", "m_iHealth");

                    if (controller.PlayerPawn.Value.Health <= 0)
                        controller.PlayerPawn.Value.CommitSuicide(true, true);
                }

                break;

            case ZoneType.Kill:
                controller.PlayerPawn.Value!.CommitSuicide(true, true);
                break;

            case ZoneType.Teleport:
                controller.PlayerPawn.Value!.Teleport(_playerData[controller].SpawnPos, controller.PlayerPawn!.Value.EyeAngles, Vector.Zero);
                break;
        }
    }

    private string FormatZoneString(ZoneType type)
    {
        return type switch
        {
            ZoneType.Hurt => $"{ChatColors.Orange}{Localizer["zone.Hurt"]}{ChatColors.White}",
            ZoneType.Kill => $"{ChatColors.Red}{Localizer["zone.Kill"]}{ChatColors.White}",
            ZoneType.Teleport => $"{ChatColors.Magenta}{Localizer["zone.Teleport"]}{ChatColors.White}",
        };
    }

    private void OnMapStart(string mapName)
    {
        LoadJson(mapName);
    }
}
