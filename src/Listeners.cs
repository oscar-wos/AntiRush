using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using AntiRush.Classes;
using FixVectorLeak.src;
using FixVectorLeak.src.Structs;
using AntiRush.Extensions;

namespace AntiRush;

public partial class AntiRush
{
    private void OnTick()
    {
        if (_gameRules is { WarmupPeriod: true } && !Config.Warmup)
            return;

        if (!_minPlayers || !_maxPlayers)
            return;

        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid() && p.PawnIsAlive))
        {
            if (player.PlayerPawn.Value?.AbsOrigin == null)
                continue;

            if (!_playerData.TryGetValue(player, out var playerData))
                continue;

            if (playerData.BlockButtons != 0)
            {
                if (Server.TickedTime < playerData.BlockButtons)
                {
                    if (player.PlayerPawn.Value?.MovementServices is null)
                        continue;

                    player.PrintToCenterHtml(Server.TickedTime + "<br>" + playerData.BlockButtons.ToString());

                    const PlayerButtons checkButtons = PlayerButtons.Forward | PlayerButtons.Back | PlayerButtons.Moveleft | PlayerButtons.Moveright | PlayerButtons.Jump | PlayerButtons.Duck;


                    var playerMovementServices = new CCSPlayer_MovementServices(player.PlayerPawn.Value.MovementServices.Handle);
                    var playerMovementSers = player.PlayerPawn.Value.MovementServices;
                    playerMovementSers.QueuedButtonChangeMask &= (ulong)~checkButtons;
                    playerMovementSers.QueuedButtonDownMask &= (ulong)~checkButtons;
                    playerMovementSers.ToggleButtonDownMask &= (ulong)~checkButtons;

                    playerMovementServices.Buttons.ButtonStates[0] &= (ulong)~checkButtons;
                    playerMovementServices.Buttons.ButtonStates[1] &= (ulong)~checkButtons;
                    playerMovementServices.Buttons.ButtonStates[2] &= (ulong)~checkButtons;


                    playerMovementServices.QueuedButtonChangeMask &= (ulong)~checkButtons;
                    playerMovementServices.QueuedButtonDownMask &= (ulong)~checkButtons;
                    playerMovementServices.ButtonDownMaskPrev &= (ulong)~checkButtons;
                    playerMovementServices.ToggleButtonDownMask &= (ulong)~checkButtons;

                    playerMovementSers.LeftMove = 0;
                    playerMovementSers.ForwardMove = 0;

                    playerMovementServices.LeftMove = 0;
                    playerMovementServices.ForwardMove = 0;

                    playerMovementSers.LastMovementImpulses[0] = 0;
                    playerMovementSers.LastMovementImpulses[1] = 0;
                    playerMovementSers.LastMovementImpulses[2] = 0;

                    playerMovementServices.LastMovementImpulses[0] = 0;
                    playerMovementServices.LastMovementImpulses[1] = 0;
                    playerMovementServices.LastMovementImpulses[2] = 0;

                    playerMovementSers.Buttons.ButtonStates[0] &= (ulong)~checkButtons;
                    playerMovementSers.Buttons.ButtonStates[1] &= (ulong)~checkButtons;
                    playerMovementSers.Buttons.ButtonStates[2] &= (ulong)~checkButtons;

                    //playerMovementSers.ButtonDownMaskPrev &= (ulong)~checkButtons;
                }
                else
                    playerData.BlockButtons = 0;
            }

            var doAction = false;
            Vector_t origin = player.PlayerPawn.Value.AbsOrigin.ToVector_t();
            Vector_t velocity = player.PlayerPawn.Value.AbsVelocity.ToVector_t();

            foreach (var zone in _zones)
            {
                if (((Config.NoRushTime != 0 && Config.NoRushTime + _roundStart < Server.CurrentTime) || _bombPlanted) && Config.RushZones.Contains((int)zone.Type))
                    continue;

                if (Config.NoCampTime != 0 && Config.NoCampTime + _roundStart > Server.CurrentTime && Config.CampZones.Contains((int)zone.Type))
                    continue;

                var isInZone = zone.IsInZone(origin.X, origin.Y, origin.Z);

                if (!zone.Data.TryGetValue(player, out var zoneData))
                    zoneData = new ZoneData();

                if (!zone.Teams.Contains(player.Team))
                    continue;

                if (!isInZone)
                {
                    zoneData.Entry = 0;
                    continue;
                }

                if (zone.Delay == 0)
                {
                    doAction = true;
                    DoAction(player, zone);
                    continue;
                }

                if (zoneData.Entry == 0)
                    zoneData.Entry = Server.CurrentTime;
                
                var diff = (zoneData.Entry + zone.Delay) - Server.CurrentTime;

                if (diff > 0)
                {
                    if (Math.Abs(diff) < 0.01 && diff >= 1)
                        player.PrintToChat($"{Prefix}{Localizer["delayRemaining", zone.ToString(Localizer), diff.ToString("0")]}");

                    continue;
                }

                doAction = true;
                DoAction(player, zone);
            }

            if (doAction)
                continue;

            _playerData[player].LastPos = [origin.X, origin.Y, origin.Z];
            _playerData[player].LastVel = [velocity.X, velocity.Y, velocity.Z];
        }

        if (Config.NoRushTime != 0 && !_bombPlanted)
        {
            var diff = (Config.NoRushTime + _roundStart) - Server.CurrentTime;

            if (diff > 0 && _countdown.Contains(diff))
                Server.PrintToChatAll($"{Prefix}{Localizer["delayRemaining", Localizer["rushDisabled"], diff.ToString("0")]}");

            else if (diff == 0)
                Server.PrintToChatAll($"{Prefix}{Localizer["rushDisabled"]}");
        }

        if (Config.NoCampTime != 0)
        {
            var diff = (Config.NoCampTime + _roundStart) - Server.CurrentTime;

            if (diff > 0 && _countdown.Contains(diff))
                Server.PrintToChatAll($"{Prefix}{Localizer["delayRemaining", Localizer["campEnabled"], diff.ToString("0")]}");

            else if (diff == 0)
                Server.PrintToChatAll($"{Prefix}{Localizer["campEnabled"]}");
        }
    }

    private void OnMapStart(string mapName)
    {
        LoadJson(mapName);
    }

    private void OnClientPutInServer(int playerSlot)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);

        if (player == null || !player.IsValid())
            return;

        _playerData[player] = new PlayerData();
    }
}