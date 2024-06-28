using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiRush;

public partial class AntiRush
{
    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var controller = @event.Userid;

        if (controller == null || !controller.IsValid)
            return HookResult.Continue;

        if (!_playerData.ContainsKey(controller))
            _playerData[controller] = new PlayerData();

        _playerData[controller].SpawnPos = new Vector(controller.PlayerPawn.Value!.AbsOrigin!.X, controller.PlayerPawn.Value.AbsOrigin.Y, controller.PlayerPawn.Value.AbsOrigin.Z);

        return HookResult.Continue;
    }

    private HookResult OnBulletImpact(EventBulletImpact @event, GameEventInfo info)
    {
        var controller = @event.Userid;

        if (!IsValidPlayer(controller) || !_playerData.TryGetValue(controller!, out var value) || value.AddZone == null || !Menu.IsCurrentMenu(controller!, value.AddZone))
            return HookResult.Continue;

        if (!value.AddZone.Points[0].IsZero() && !value.AddZone.Points[1].IsZero())
            return HookResult.Continue;

        if (Server.CurrentTime - value.AddZone.LastShot < 0.1)
            return HookResult.Continue;

        value.AddZone.LastShot = Server.CurrentTime;

        if (value.AddZone.Points[0].IsZero())
            value.AddZone.Points[0] = new Vector(@event.X, @event.Y, @event.Z);
        else
        {
            value.AddZone.Points[1] = new Vector(@event.X, @event.Y, @event.Z);

            var diff = Math.Abs(value.AddZone.Points[0].Z - value.AddZone.Points[1].Z);

            if (diff < 200)
            {
                if (value.AddZone.Points[0].Z >= value.AddZone.Points[1].Z)
                    value.AddZone.Points[0].Z += 200 - diff;
                else
                    value.AddZone.Points[1].Z += 200 - diff;
            }
        }

        Menu.PopMenu(controller!, value.AddZone);
        BuildAddZoneMenu(controller!);

        return HookResult.Continue;
    }

    private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var controller = @event.Userid;

        if (controller == null || !controller.IsValid)
            return HookResult.Continue;

        if (!_playerData.ContainsKey(controller))
            _playerData[controller] = new PlayerData();

        return HookResult.Continue;
    }
}