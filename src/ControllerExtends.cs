using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiRush;

public static class ControllerExtends
{
    public static bool IsValid(this CCSPlayerController? controller, bool checkBot = false)
    {
        if (checkBot)
             return controller != null && controller is { IsValid: true, IsBot: false };

        return controller != null && controller.IsValid;
    }

    public static void Damage(this CCSPlayerController? controller, int damage)
    {
        if (!controller.IsValid())
            return;

        controller!.Health -= damage;

        if (controller.Health <= 0)
            controller.PlayerPawn.Value!.CommitSuicide(true, true);
    }

    public static void Bounce(this CCSPlayerController? controller)
    {
        if (!controller.IsValid())
            return;

        var pos = controller!.PlayerPawn.Value!.AbsOrigin;
        var eyes = controller.PlayerPawn.Value.EyeAngles;
        var vel = controller.PlayerPawn.Value.AbsVelocity;
        var speed = Math.Sqrt(vel.X * vel.X + vel.Y * vel.Y);
        var newVel = new Vector(controller.PlayerPawn.Value.AbsVelocity.X, controller.PlayerPawn.Value.AbsVelocity.Y, controller.PlayerPawn.Value.AbsVelocity.Z);

        newVel *= (-350 / (float)speed);
        newVel.Z = newVel.Z <= 0f ? 150f : Math.Min(newVel.Z, 150f);
        controller.PlayerPawn.Value.Teleport(pos, eyes, newVel);
    }
}