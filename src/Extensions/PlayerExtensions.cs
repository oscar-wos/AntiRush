using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FixVectorLeak.Extensions;
using FixVectorLeak.Structs;

namespace AntiRush.Extensions;

public static class PlayerExtentions
{
    public static bool IsValid(this CCSPlayerController? player, bool checkBot = false)
    {
        if (checkBot)
            return player is { IsValid: true, IsBot: false };

        return player is { IsValid: true };
    }

    public static void Damage(this CCSPlayerController? player, int damage)
    {
        if (player == null || !player.IsValid() || player.PlayerPawn.Value == null)
            return;

        player.PlayerPawn.Value.Health -= damage;
        Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iHealth");

        if (player.PlayerPawn.Value.Health <= 0)
            player.PlayerPawn.Value.CommitSuicide(true, true);
    }

    public static void Bounce(this CCSPlayerController? player, Vector_t lastPos, Vector_t lastVel)
    {
        if (player == null || player.PlayerPawn.Value == null)
            return;

        var speed = -300 / (float)Math.Sqrt(lastVel.X * lastVel.X + lastVel.Y * lastVel.Y);
        Vector_t newVel = new(lastVel.X * speed, lastVel.Y * speed, -100);

        player.PlayerPawn.Value.Teleport(lastPos, velocity: newVel);
    }
}