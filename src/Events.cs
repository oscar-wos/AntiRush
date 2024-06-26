using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiRush;

public partial class AntiRush
{
    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var controller = @event.Userid;

        if (controller == null || !controller.IsValid || !_playerData.TryGetValue(controller!, out var value))
            return HookResult.Continue;

        value.SpawnPos = new Vector(controller!.PlayerPawn!.Value!.AbsOrigin!.X, controller!.PlayerPawn!.Value!.AbsOrigin.Y, controller!.PlayerPawn!.Value!.AbsOrigin.Z);

        return HookResult.Continue;
    }

    private HookResult OnBulletImpact(EventBulletImpact @event, GameEventInfo info)
    {
        var controller = @event.Userid;

        if (!IsValidPlayer(controller) || !_playerData.TryGetValue(controller!, out var value) || value.AddZone == null || !Menu.IsCurrentMenu(controller!, value.AddZone))
            return HookResult.Continue;

        if (VectorIsZero(value.AddZone.Points[0]))
            value.AddZone.Points[0] = new Vector(@event.X, @event.Y, @event.Z);
        else if (VectorIsZero(value.AddZone.Points[1]))
            value.AddZone.Points[1] = new Vector(@event.X, @event.Y, @event.Z);

        Menu.PopMenu(controller!, value.AddZone);
        BuildAddZoneMenu(controller!);

        return HookResult.Continue;
    }

    private HookResult OnPlayerConnect(EventPlayerConnect @event, GameEventInfo info)
    {
        var controller = @event.Userid;

        if (controller == null || !controller.IsValid || !_playerData.ContainsKey(controller))
            return HookResult.Continue;

        _playerData.Add(controller, new PlayerData());

        return HookResult.Continue;
    }
}