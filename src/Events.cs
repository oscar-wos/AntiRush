using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiRush;

public partial class AntiRush
{
    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _roundStart = Server.CurrentTime;
        _bombPlanted = false;

        _gameRules ??= Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
        _warmup = _gameRules.WarmupPeriod;

        foreach (var zone in _zones)
        {
            zone.Data = [];

            if (Config.DrawZones)
                zone.Draw();
        }

        return HookResult.Continue;
    }

    private HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo info)
    {
        if (!Config.DisableOnBombPlant)
            return HookResult.Continue;

        _bombPlanted = true;
        Server.PrintToChatAll($"{Prefix}{Localizer["rushDisabled"]}");

        return HookResult.Continue;
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var controller = @event.Userid;

        if (controller == null || !controller.IsValid() || controller.PlayerPawn.Value == null || controller.PlayerPawn.Value.AbsOrigin == null)
            return HookResult.Continue;

        if (_playerData.TryGetValue(controller, out var value))
            value.SpawnPos = new Vector(controller.PlayerPawn.Value.AbsOrigin.X, controller.PlayerPawn.Value.AbsOrigin.Y, controller.PlayerPawn.Value.AbsOrigin.Z);

        return HookResult.Continue;
    }

    private HookResult OnBulletImpact(EventBulletImpact @event, GameEventInfo info)
    {
        var controller = @event.Userid;

        if (controller == null || !controller.IsValid() || !_playerData.TryGetValue(controller, out var value) || value.AddZone == null || !Menu.IsCurrentMenu(controller, value.AddZone))
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

            var diffX = Math.Abs(value.AddZone.Points[0].X - value.AddZone.Points[1].X);
            var diffY = Math.Abs(value.AddZone.Points[0].Y - value.AddZone.Points[1].Y);
            var diffZ = Math.Abs(value.AddZone.Points[0].Z - value.AddZone.Points[1].Z);

            if (diffX < 32)
            {
                value.AddZone.Points[0].X += (value.AddZone.Points[0].X >= value.AddZone.Points[1].X ? 1 : -1) * ((32 - diffX) / 2);
                value.AddZone.Points[1].X += (value.AddZone.Points[0].X > value.AddZone.Points[1].X ? -1 : 1) * ((32 - diffX) / 2);
            }

            if (diffY < 32)
            {
                value.AddZone.Points[0].Y += (value.AddZone.Points[0].Y >= value.AddZone.Points[1].Y ? 1 : -1) * ((32 - diffY) / 2);
                value.AddZone.Points[1].Y += (value.AddZone.Points[0].Y > value.AddZone.Points[1].Y ? -1 : 1) * ((32 - diffY) / 2);
            }

            if (diffZ < 200)
            {
                if (value.AddZone.Points[0].Z >= value.AddZone.Points[1].Z)
                    value.AddZone.Points[0].Z += 200 - diffZ;
                else
                    value.AddZone.Points[1].Z += 200 - diffZ;
            }
        }

        Menu.PopMenu(controller, value.AddZone);
        BuildAddZoneMenu(controller);

        return HookResult.Continue;
    }
}