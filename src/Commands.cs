using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using AntiRush.Enums;
using AntiRush.Extensions;
using CSSharpUtils.Extensions;

namespace AntiRush;

public partial class AntiRush
{
    public void CommandAntiRush(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid(true) || !player.HasPermission("@css/generic")) 
            return;

        BuildMenu(player);
    }

    public void CommandAddZone(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid(true) || !player.HasPermission("@css/root"))
            return;

        BuildMenu(player);
        BuildMenu(player, MenuType.Add);
    }

    public void CommandViewZones(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid(true) || !player.HasPermission("@css/generic"))
            return;

        BuildMenu(player);
        BuildMenu(player, MenuType.View);
    }
}