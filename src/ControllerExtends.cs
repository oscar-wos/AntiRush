using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiRush;

public static class ControllerExtends
{
    public static bool IsValid(this CCSPlayerController? controller, bool checkBot = false)
    {
        if (checkBot)
             return controller is { IsValid: true, IsBot: false };

        return controller is { IsValid: true };
    }

    public static void Damage(this CCSPlayerController? controller, int damage)
    {
        if (controller == null || !controller.IsValid() || controller.PlayerPawn.Value == null)
            return;

        controller.PlayerPawn.Value.Health -= damage;
        Utilities.SetStateChanged(controller.PlayerPawn.Value, "CBaseEntity", "m_iHealth");

        if (controller.PlayerPawn.Value.Health <= 0)
            controller.PlayerPawn.Value.CommitSuicide(true, true);
    }

    public static void Bounce(this CCSPlayerController? controller, float[] lastPos, float[] lastVel)
    {
        if (controller == null || controller.PlayerPawn.Value == null)
            return;

        var vel = new Vector(lastVel[0], lastVel[1], lastVel[2]);
        var speed = vel.Length2D();

        vel *= (-350 / speed);
        vel.Z = vel.Z <= 0 ? 150 : Math.Min(vel.Z, 150);
        controller.PlayerPawn.Value.Teleport(new Vector(lastPos[0], lastPos[1], lastPos[2]), null, vel);
    }

    public static bool HasPermission(this CCSPlayerController? controller, string permission)
    {
        return controller != null && controller.IsValid() && AdminManager.PlayerHasPermissions(controller, permission);
    }
}