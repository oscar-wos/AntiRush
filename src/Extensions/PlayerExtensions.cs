using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FixVectorLeak.src;
using FixVectorLeak.src.Structs;

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

    public static void Bounce(this CCSPlayerController? player, float[] lastPos, float[] lastVel)
    {
        if (player == null || player.PlayerPawn.Value == null)
            return;

        var speed = -300 / (float)Math.Sqrt(lastVel[0] * lastVel[0] + lastVel[1] * lastVel[1]);

        Vector_t newPos = new(lastPos[0], lastPos[1], lastPos[2]);
        Vector_t newVel = new(lastVel[0] * speed, lastVel[1] * speed, lastVel[2] * speed <= 0 ? 100 : Math.Min(lastVel[2] * speed, 100));

        player.PlayerPawn.Value.Teleport(newPos, velocity: newVel);
    }
}