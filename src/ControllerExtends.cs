using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

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
        if (!controller.IsValid() || controller!.PlayerPawn.Value == null)
            return;

        controller.PlayerPawn.Value.Health -= damage;
        Utilities.SetStateChanged(controller.PlayerPawn.Value, "CBaseEntity", "m_iHealth");

        if (controller.PlayerPawn.Value.Health <= 0)
            controller.PlayerPawn.Value.CommitSuicide(true, true);
    }

    public static void Bounce(this CCSPlayerController? controller)
    {
        if (!controller.IsValid() || controller!.PlayerPawn.Value == null)
            return;

        var vel = controller.PlayerPawn.Value.AbsVelocity;
        var speed = Math.Sqrt(vel.X * vel.X + vel.Y * vel.Y);

        vel *= (-350 / (float)speed);
        vel.Z = vel.Z <= 0f ? 150f : Math.Min(vel.Z, 150f);
        controller.PlayerPawn.Value.Teleport(controller.PlayerPawn.Value.AbsOrigin, controller.PlayerPawn.Value.EyeAngles, vel);
    }
}