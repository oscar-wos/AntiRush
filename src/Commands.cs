using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using AntiRush.Enums;

namespace AntiRush;

public partial class AntiRush
{
    public void CommandAntiRush(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller == null || !controller.IsValid(true) || !controller.HasPermission("@css/generic")) 
            return;

        BuildMenu(controller);
    }

    public void CommandAddZone(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller == null || !controller.IsValid(true) || !controller.HasPermission("@css/root"))
            return;

        BuildMenu(controller);
        BuildMenu(controller, MenuType.Add);
    }

    public void CommandViewZones(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller == null || !controller.IsValid(true) || !controller.HasPermission("@css/generic"))
            return;

        BuildMenu(controller);
        BuildMenu(controller, MenuType.View);
    }
}