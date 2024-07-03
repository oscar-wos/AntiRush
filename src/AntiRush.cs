using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using AntiRush.Enums;

namespace AntiRush;

public partial class AntiRush : BasePlugin, IPluginConfig<AntiRushConfig>
{
    public void OnConfigParsed(AntiRushConfig config)
    {
        Config = config;
    }

    public override void Load(bool isReload)
    {
        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventBombPlanted>(OnBombPlanted);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventBulletImpact>(OnBulletImpact);

        AddCommand("css_antirush", "Anti-Rush", CommandAntiRush);
        AddCommand("css_addzone", "Add Zone", CommandAddZone);
        //AddCommand("css_viewzones", "View Zones", CommandViewZones);

        LoadJson(Server.MapName);

        Server.NextFrame(() =>
        {
            foreach (var controller in Utilities.GetPlayers())
                _playerData[controller] = new PlayerData();

            Server.ExecuteCommand("mp_restartgame 1");
        });
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
        var delay = zoneType != ZoneType.Bounce && float.TryParse(menu.Items[3].DataString, out var valueDelay) ? (float)Math.Floor(valueDelay * 10) / 10 : 0;
        var damage = zoneType == ZoneType.Hurt && int.TryParse(menu.Items[4].DataString, out var valueDamage) ? valueDamage : 0;
        var name = menu.Items[2].DataString;

        var zone = new Zone(name, zoneType, delay, damage, teams, minPoint, maxPoint);
        _zones.Add(zone);

        var printMessage = $"{Prefix}{Localizer["saving", zone.ToString(Localizer), name]} | {Localizer["menu.Teams"]} [";

        if (teams.Contains(CsTeam.Terrorist))
            printMessage += $"{ChatColors.LightYellow}{Localizer["t"]}{ChatColors.White}";

        if (teams.Contains(CsTeam.CounterTerrorist))
            printMessage += $"{(teams.Contains(CsTeam.Terrorist) ? "|" : "")}{ChatColors.Blue}{Localizer["ct"]}{ChatColors.White}";

        printMessage += "]";

        if (zoneType != ZoneType.Bounce)
            printMessage += $" | {Localizer["menu.Delay"]} {ChatColors.Green}{delay}{ChatColors.White}";

        if (zoneType == ZoneType.Hurt)
            printMessage += $" | {Localizer["menu.Damage"]} {ChatColors.Green}{damage}{ChatColors.White}";

        controller.PrintToChat(printMessage);
        SaveJson(Server.MapName);

        if (Config.DrawZones)
            zone.Draw();
    }

    private void DoAction(CCSPlayerController controller, Zone zone)
    {
        var currentTime = Server.CurrentTime;

        if (controller.IsValid(true) && currentTime - _playerData[controller].LastMessage >= 1)
        {
            switch (Config.Messages)
            {
                case "simple":
                    if ((zone.Type == ZoneType.Hurt && currentTime % 1 == 0) || zone.Type != ZoneType.Hurt)
                        controller.PrintToChat($"{Prefix}{zone.ToString(Localizer)}");

                    break;

                case "detailed":
                    if (zone.Type is (ZoneType.Bounce or ZoneType.Teleport))
                    {
                        controller.PrintToChat(Config.NoRushTime != 0
                            ? $"{Prefix}{zone.ToString(Localizer)}{Localizer["rushDelayRemaining", (_roundStart + Config.NoRushTime - currentTime).ToString("0")]}"
                            : $"{Prefix}{zone.ToString(Localizer)}");
                    }
                    else if ((zone.Type == ZoneType.Hurt && currentTime % 1 == 0) || zone.Type != ZoneType.Hurt)
                        controller.PrintToChat($"{Prefix}{zone.ToString(Localizer)}");

                    break;
            }

            _playerData[controller].LastMessage = currentTime;
        }

        switch (zone.Type)
        {
            case ZoneType.Bounce:
                controller.Bounce();
                return;

            case ZoneType.Hurt:
                if (currentTime % 1 == 0)
                    controller.Damage(zone.Damage);

                return;

            case ZoneType.Kill:
                controller.PlayerPawn.Value!.CommitSuicide(true, true);
                return;

            case ZoneType.Teleport:
                controller.PlayerPawn.Value!.Teleport(_playerData[controller].SpawnPos, controller.PlayerPawn.Value.EyeAngles, Vector.Zero);
                return;
        }
    }
}