using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using AntiRush.Enums;

namespace AntiRush;

public partial class AntiRush : BasePlugin
{
    public override void Load(bool isReload)
    {
        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventBulletImpact>(OnBulletImpact);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);

        AddCommand("css_antirush", "Anti-Rush", CommandAntiRush);
        AddCommand("css_addzone", "Add Zone", CommandAddZone);
        AddCommand("css_viewzones", "View Zones", CommandViewZones);

        if (!isReload)
            return;

        foreach (var controller in Utilities.GetPlayers())
            _playerData[controller] = new PlayerData();

        LoadJson(Server.MapName);
        Server.ExecuteCommand("mp_restartgame 1");
    }

    private void SaveZone(CCSPlayerController controller)
    {
        var menu = _playerData[controller].AddZone;

        CsTeam[] teams = menu!.Items[1].Option switch
        {
            0 => [CsTeam.Terrorist],
            1 => [CsTeam.CounterTerrorist],
            2 => [CsTeam.Terrorist, CsTeam.CounterTerrorist],
            _ => []
        };

        var zoneType = (ZoneType)menu.Items[0].Option;
        var minPoint = new Vector(Math.Min(menu.Points[0].X, menu.Points[1].X), Math.Min(menu.Points[0].Y, menu.Points[1].Y), Math.Min(menu.Points[0].Z, menu.Points[1].Z));
        var maxPoint = new Vector(Math.Max(menu.Points[0].X, menu.Points[1].X), Math.Max(menu.Points[0].Y, menu.Points[1].Y), Math.Max(menu.Points[0].Z, menu.Points[1].Z));
        var delay = zoneType != ZoneType.Bounce && float.TryParse(menu.Items[3].DataString, out var valueDelay) ? valueDelay : 0;
        var damage = zoneType == ZoneType.Hurt && int.TryParse(menu.Items[4].DataString, out var valueDamage) ? valueDamage : 10;
        var name = menu.Items[2].DataString;

        if (name.Length == 0)
            name = "noname";

        var zone = new Zone(zoneType, teams, minPoint, maxPoint, name, delay, damage);
        _zones.Add(zone);

        var printMessage = $"{Prefix}{Localizer["saving", name, FormatZoneString(zoneType)]} | {Localizer["menu.Teams"]} [";

        if (teams.Contains(CsTeam.Terrorist))
            printMessage += $" {ChatColors.LightYellow}{Localizer["t"]}{ChatColors.White}";

        if (teams.Contains(CsTeam.CounterTerrorist))
            printMessage += $" {ChatColors.Blue}{Localizer["ct"]}{ChatColors.White}";

        printMessage += " ]";

        if (zoneType != ZoneType.Bounce)
            printMessage += $" | {Localizer["menu.Delay"]} {ChatColors.Green}{delay}{ChatColors.White}";

        if (zoneType == ZoneType.Hurt)
            printMessage += $" | {Localizer["menu.Damage"]} {ChatColors.Green}{damage}{ChatColors.White}";

        controller.PrintToChat(printMessage);
    }

    private bool DoAction(CCSPlayerController controller, Zone zone)
    {
        if (zone.Type != ZoneType.Bounce && Server.CurrentTime - _playerData[controller].LastMessage > 1)
            controller!.PrintToChat($"{Prefix}{FormatZoneString(zone.Type)}");

        _playerData[controller].LastMessage = Server.CurrentTime;

        switch (zone.Type)
        {
            case ZoneType.Bounce:
                var speed = Math.Sqrt(_playerData[controller].LastVelocity.X * _playerData[controller].LastVelocity.X + _playerData[controller].LastVelocity.Y * _playerData[controller].LastVelocity.Y);

                _playerData[controller].LastVelocity *= (-350 / (float)speed);
                _playerData[controller].LastVelocity.Z = _playerData[controller].LastVelocity.Z <= 0f ? 150f : Math.Min(_playerData[controller].LastVelocity.Z, 150f);
                controller.PlayerPawn.Value!.Teleport(_playerData[controller].LastPosition, controller.PlayerPawn!.Value.EyeAngles, _playerData[controller].LastVelocity);
                return true;

            case ZoneType.Hurt:
                if (Server.CurrentTime % 1 != 0)
                    return false;

                controller.PlayerPawn.Value!.Health -= zone.Damage;
                Utilities.SetStateChanged(controller.PlayerPawn.Value, "CBaseEntity", "m_iHealth");

                if (controller.PlayerPawn.Value.Health <= 0)
                    controller.PlayerPawn.Value.CommitSuicide(true, true);

                return false;

            case ZoneType.Kill:
                controller.PlayerPawn.Value!.CommitSuicide(true, true);
                return false;

            case ZoneType.Teleport:
                controller.PlayerPawn.Value!.Teleport(_playerData[controller].SpawnPos, controller.PlayerPawn!.Value.EyeAngles, Vector.Zero);
                return false;
        }

        return false;
    }

    private string FormatZoneString(ZoneType type)
    {
        return type switch
        {
            ZoneType.Bounce => $"{ChatColors.Yellow}{Localizer["zone.Bounce"]}{ChatColors.White}",
            ZoneType.Hurt => $"{ChatColors.Orange}{Localizer["zone.Hurt"]}{ChatColors.White}",
            ZoneType.Kill => $"{ChatColors.Red}{Localizer["zone.Kill"]}{ChatColors.White}",
            ZoneType.Teleport => $"{ChatColors.Magenta}{Localizer["zone.Teleport"]}{ChatColors.White}",
            _ => ""
        };
    }

    private static bool IsValidPlayer(CCSPlayerController? player)
    {
        return player != null && player is { IsValid: true, Connected: PlayerConnectedState.PlayerConnected, PawnIsAlive: true, IsBot: false };
    }
}