using AntiRush.Enums;
using CounterStrikeSharp.API;
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
                if (!zone.IsInZone(pos))
                    continue;

                if (zone.Type == ZoneType.Bounce)
                {
                    bounce = true;
                    BouncePlayer(controller);
                    continue;
                }

                if (zone.Type == ZoneType.Teleport)
                {
                    controller.PlayerPawn.Value!.Teleport(_playerData[controller].SpawnPos, controller.PlayerPawn!.Value.EyeAngles, Vector.Zero);
                    continue;
                }
            }

            if (bounce)
                continue;
            
            _playerData[controller].LastPosition = controller!.PlayerPawn.Value!.AbsOrigin!;
            _playerData[controller].LastVelocity = controller.PlayerPawn.Value.AbsVelocity;
        }
    }

    private void OnMapStart(string mapName)
    {
        LoadJson(mapName);
    }
}