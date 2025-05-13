using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using AntiRush.Classes;
using AntiRush.Enums;
using AntiRush.Extensions;
using CSSharpUtils.Extensions;
using CSSharpUtils.Utils;
using FixVectorLeak.src;
using FixVectorLeak.src.Structs;

namespace AntiRush;

public partial class AntiRush : BasePlugin, IPluginConfig<AntiRushConfig>
{
    public void OnConfigParsed(AntiRushConfig config)
    {
        if (config.Update())
            config.Reload();

        Config = config;
        Prefix = ChatUtils.FormatMessage(config.Prefix);
        _countdown = [.. Config.Countdown.Select(c => (float)c)];
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

        LoadJson(Server.MapName);

        Server.NextFrame(() =>
        {
            foreach (var player in Utilities.GetPlayers())
                _playerData[player] = new PlayerData();

            if (Config.RestartOnLoad)
                Server.ExecuteCommand("mp_restartgame 1");
        });
    }

    private void SaveZone(CCSPlayerController player)
    {
        var menu = _playerData[player].AddZoneMenu;
         
        CsTeam[] teams = menu!.Items[1].Option switch
        {
            0 => [CsTeam.Terrorist],
            1 => [CsTeam.CounterTerrorist],
            2 => [CsTeam.Terrorist, CsTeam.CounterTerrorist],
            _ => []
        };

        var zoneType = (ZoneType)menu.Items[0].Option;
        float[] minPoint = [Math.Min(menu.Points[0]!.Value.X, menu.Points[1]!.Value.X), Math.Min(menu.Points[0]!.Value.Y, menu.Points[1]!.Value.Y), Math.Min(menu.Points[0]!.Value.Z, menu.Points[1]!.Value.Z)];
        float[] maxPoint = [Math.Max(menu.Points[0]!.Value.X, menu.Points[1]!.Value.X), Math.Max(menu.Points[0]!.Value.Y, menu.Points[1]!.Value.Y), Math.Max(menu.Points[0]!.Value.Z, menu.Points[1]!.Value.Z)];
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

        player.PrintToChat(printMessage);
        SaveJson(Server.MapName);

        if (Config.DrawZones)
            zone.Draw();
    }

    private bool PrintAction(CCSPlayerController player, Zone zone)
    {
        if (!player.IsValid(true) || !(Server.CurrentTime - _playerData[player].LastMessage >= 1))
            return false;

        if (zone.Type == ZoneType.Hurt && Server.CurrentTime % 1 != 0)
            return false;

        switch (Config.Messages)
        {
            case "simple":
                player.PrintToChat($"{Prefix}{zone.ToString(Localizer)}");
                return true;

            case "detailed":
                if (zone.Type is (ZoneType.Bounce or ZoneType.Teleport))
                {
                    player.PrintToChat(Config.NoRushTime != 0
                        ? $"{Prefix}{Localizer["rushDelayRemaining", zone.ToString(Localizer), (_roundStart + Config.NoRushTime - Server.CurrentTime).ToString("0")]}"
                        : $"{Prefix}{zone.ToString(Localizer)}");

                    return true;
                }

                if (zone.Type == ZoneType.Hurt)
                {
                    player.PrintToChat($"{Prefix}{Localizer["hurtDamage", zone.ToString(Localizer), zone.Damage]}");
                    return true;
                }

                player.PrintToChat($"{Prefix}{zone.ToString(Localizer)}");
                return true;
        }

        return false;
    }

    private void DoAction(CCSPlayerController player, Zone zone)
    {
        if (player.PlayerPawn.Value?.MovementServices is null)
            return;

        if (PrintAction(player, zone))
            _playerData[player].LastMessage = Server.CurrentTime;

        switch (zone.Type)
        {
            case ZoneType.Bounce:
                _playerData[player].BlockButtons = (float)Server.TickedTime + 1;
                player.Bounce(_playerData[player].LastPos, _playerData[player].LastVel);

                return;

            case ZoneType.Hurt:
                if (Server.CurrentTime % 1 == 0)
                    player.Damage(zone.Damage);

                return;

            case ZoneType.Kill:
                player.PlayerPawn.Value.CommitSuicide(true, true);
                return;

            case ZoneType.Teleport:
                if (_playerData[player].SpawnPos is not null)
                    player.PlayerPawn.Value.Teleport(_playerData[player].SpawnPos, velocity: new Vector_t());

                return;

            case ZoneType.Wall:
                player.PlayerPawn.Value.Teleport(new Vector_t(_playerData[player].LastPos[0], _playerData[player].LastPos[1], _playerData[player].LastPos[2]), velocity: new Vector_t());
                return;
        }
    }
}