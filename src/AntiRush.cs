using AntiRush.Classes;
using AntiRush.Enums;
using AntiRush.Extensions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CSSharpUtils.Extensions;
using CSSharpUtils.Utils;
using FixVectorLeak.Extensions;
using FixVectorLeak.Structs;
using Microsoft.Extensions.Logging;

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

        Logger.LogInformation("{ModuleName} loaded successfully!", ModuleName);
    }

    public override void Unload(bool isReload)
    {
        Logger.LogInformation("{ModuleName} unloaded successfully!", ModuleName);
    }

    private void SaveZone(CCSPlayerController player)
    {
        if (!_playerData.TryGetValue(player, out var playerData))
            return;

        var menu = playerData.AddZoneMenu;
        playerData.AddZone?.Clear();

        CsTeam[] teams = menu!.Items[1].Option switch
        {
            0 => [CsTeam.Terrorist, CsTeam.CounterTerrorist],
            1 => [CsTeam.Terrorist],
            2 => [CsTeam.CounterTerrorist],
            _ => []
        };

        var zoneType = (ZoneType)menu.Items[0].Option;
        var minPoint = new Vector_t(Math.Min(menu.Points[0]!.Value.X, menu.Points[1]!.Value.X), Math.Min(menu.Points[0]!.Value.Y, menu.Points[1]!.Value.Y), Math.Min(menu.Points[0]!.Value.Z, menu.Points[1]!.Value.Z));
        var maxPoint = new Vector_t(Math.Max(menu.Points[0]!.Value.X, menu.Points[1]!.Value.X), Math.Max(menu.Points[0]!.Value.Y, menu.Points[1]!.Value.Y), Math.Max(menu.Points[0]!.Value.Z, menu.Points[1]!.Value.Z));
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

        if (zone.Type is ZoneType.Hurt && Server.CurrentTime % 1 != 0)
            return false;

        switch (Config.Messages)
        {
            case "simple":
                player.PrintToChat($"{Prefix}{zone.ToString(Localizer)}");
                return true;

            case "detailed":
                if (zone.Type is (ZoneType.Bounce or ZoneType.Teleport or ZoneType.Wall))
                {
                    player.PrintToChat(Config.NoRushTime != 0
                        ? $"{Prefix}{Localizer["rushDelayRemaining", zone.ToString(Localizer), (_roundStart + Config.NoRushTime - Server.CurrentTime).ToString("0")]}"
                        : $"{Prefix}{zone.ToString(Localizer)}");

                    return true;
                }

                if (zone.Type is ZoneType.Hurt)
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

        if (!_playerData.TryGetValue(player, out var playerData))
            return;

        if (PrintAction(player, zone))
            playerData.LastMessage = Server.CurrentTime;

        switch (zone.Type)
        {
            case ZoneType.Bounce:
                if (playerData.LastPos is not null && playerData.LastVel is not null)
                    player.Bounce((Vector_t)playerData.LastPos, (Vector_t)playerData.LastVel);

                return;

            case ZoneType.Hurt:
                if (Server.CurrentTime % 1 == 0)
                    player.Damage(zone.Damage);

                return;

            case ZoneType.Kill:
                player.PlayerPawn.Value.CommitSuicide(true, true);
                return;

            case ZoneType.Teleport:
                if (playerData.SpawnPos is not null)
                    player.PlayerPawn.Value.Teleport(playerData.SpawnPos, velocity: new Vector_t());

                return;

            case ZoneType.Wall:
                if (playerData.LastPos is not null)
                    player.PlayerPawn.Value.Teleport(playerData.LastPos, velocity: new Vector_t());

                return;
        }
    }
}