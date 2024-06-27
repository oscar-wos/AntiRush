using AntiRush.Enums;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace AntiRush;

public partial class AntiRush
{
    public void CommandAntiRush(CCSPlayerController? controller, CommandInfo info)
    {
        if (!IsValidPlayer(controller))
            return;

        if (!AdminManager.PlayerHasPermissions(controller, "@css/generic"))
        {
            controller!.PrintToChat($"{Prefix}{Localizer["missingPermission", "@css/generic"]}");
            return;
        }

        BuildMenu(controller!);
    }

    public void CommandAddZone(CCSPlayerController? controller, CommandInfo info)
    {
        if (!IsValidPlayer(controller))
            return;

        if (!AdminManager.PlayerHasPermissions(controller, "@css/root"))
        {
            controller!.PrintToChat($"{Prefix}{Localizer["missingPermission", "@css/root"]}");
            return;
        }

        BuildMenu(controller!);
        BuildMenu(controller!, MenuType.Add);
    }

    public void CommandViewZones(CCSPlayerController? controller, CommandInfo info)
    {
        if (!IsValidPlayer(controller))
            return;

        if (!AdminManager.PlayerHasPermissions(controller, "@css/generic"))
        {
            controller!.PrintToChat($"{Prefix}{Localizer["missingPermission", "@css/generic"]}");
            return;
        }

        BuildMenu(controller!);
        BuildMenu(controller!, MenuType.View);
    }
}