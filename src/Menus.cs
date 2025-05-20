using AntiRush.Enums;
using AntiRush.Extensions;
using CounterStrikeSharp.API.Core;
using CSSharpUtils.Extensions;
using FixVectorLeak.Structs;
using Menu;
using Menu.Enums;

namespace AntiRush;

public partial class AntiRush
{
    private void BuildMenu(CCSPlayerController player, MenuType type = MenuType.Main)
    {
        if (!player.IsValid())
            return;

        if (_playerData.TryGetValue(player, out var playerData))
        {
            if (playerData.AddZoneMenu is not null)
            {
                for (var i = 0; i < 2; i++)
                    playerData.AddZoneMenu.Points[i] = null;

                playerData.AddZoneMenu = null;
            }

            playerData.AddZone?.Clear();
        }

        switch (type)
        {
            case MenuType.Main:
                BuildMainMenu(player);
                break;

            case MenuType.Add:
                BuildAddZoneMenu(player);
                break;

            case MenuType.View:
                BuildViewZoneMenu(player);
                break;
        }
    }

    private void BuildMainMenu(CCSPlayerController player, bool updateMenu = false)
    {
        var mainMenu = new MenuBase(new MenuValue("AntiRush") { Prefix = "<font class=\"fontSize-l\">", Suffix = "<font class=\"fontSize-m\">" });

        var customButtons = new List<MenuValue>
        {
            new CustomButton(Localizer["menu.Add"], c => BuildMenu(c, MenuType.Add)) { Suffix = "<font color=\"#FFFFFF\">" },
            new CustomButton(Localizer["menu.View"], c => BuildMenu(c, MenuType.View))
        };

        customButtons[0].Prefix = !player.HasPermission("@css/root") ? "<font color=\"#808080\">" : "";

        mainMenu.AddItem(new MenuItem(MenuItemType.Button, customButtons));
        mainMenu.AddItem(new MenuItem(MenuItemType.Spacer));
        mainMenu.AddItem(new MenuItem(MenuItemType.Bool, new MenuValue($"{Localizer["menu.Debug"]} ")) { Data = [_playerData[player].Debug ? 1 : 0] });

        if (_playerData[player].Debug)
        {
            var debugOptions = new List<MenuValue>
            {
                new(Localizer["zone.Bounce"]),
                new(Localizer["zone.Hurt"]),
                new(Localizer["zone.Kill"]),
                new(Localizer["zone.Teleport"])
            };

            mainMenu.AddItem(new MenuItem(MenuItemType.ChoiceBool, debugOptions, true) { Data = [.. _playerData[player].DebugOptions.Select(o => o ? 1 : 0)] });
        }

        if (updateMenu)
            mainMenu.Option = 1;

        Menu.SetMenu(player, mainMenu, (buttons, menu, selectedItem) =>
        {
            if (buttons != MenuButtons.Select)
                return;

            switch (menu.Option)
            {
                case 0:
                    if (selectedItem!.Option == 0 && !player.HasPermission("@css/root"))
                    {
                        player.PrintToChat($"{Prefix}{Localizer["missingPermission", "@css/root"]}");
                        return;
                    }

                    var customButton = (CustomButton)selectedItem.Values![selectedItem.Option];
                    customButton.Callback.Invoke(player);
                    break;

                case 1:
                    _playerData[player].Debug = !_playerData[player].Debug;
                    BuildMainMenu(player, true);
                    break;

                case 2:
                    _playerData[player].DebugOptions[selectedItem!.Option] = !_playerData[player].DebugOptions[selectedItem.Option];
                    break;
            }
        });
    }

    private void BuildAddZoneMenu(CCSPlayerController player)
    {
        var addZoneMenu = new AddZoneMenu(new MenuValue(Localizer["menu.Add"]) { Suffix = "<font class=\"fontSize-m\">" }) { Input = new MenuValue("____") { Prefix = "<font color=\"#00FF00\">", Suffix = "<font color=\"#FFFFFF\">" }};

        if (_playerData[player].AddZoneMenu == null)
            addZoneMenu.AddItem(new MenuItem(MenuItemType.Text, new MenuValue(Localizer["menu.Shoot", "1"])));
        else if (_playerData[player].AddZoneMenu!.Points[1] is null)
            addZoneMenu.AddItem(new MenuItem(MenuItemType.Text, new MenuValue(Localizer["menu.Shoot", "2"])));
        else
        {
            var zoneTypes = new List<MenuValue>
            {
                new(Localizer["zone.Bounce"]) { Prefix = "<font color=\"#FFFF00\">", Suffix = "<font color=\"#FFFFFF\">" },
                new(Localizer["zone.Hurt"]) { Prefix = "<font color=\"#FFA500\">", Suffix = "<font color=\"#FFFFFF\">" },
                new(Localizer["zone.Kill"]) { Prefix = "<font color=\"#FF0000\">", Suffix = "<font color=\"#FFFFFF\">" },
                new(Localizer["zone.Teleport"]) { Prefix = "<font color=\"#FF00FF\">", Suffix = "<font color=\"#FFFFFF\">" },
                new(Localizer["zone.Wall"]) { Prefix = "<font color=\"#0000FF\">", Suffix = "<font color=\"#FFFFFF\">" }
            };

            var teams = new List<MenuValue>
            {
                new(Localizer["both"]) { Prefix = "<font color=\"#C2AD7D\">", Suffix = "<font color=\"#FFFFFF\">" },
                new(Localizer["t"]) { Prefix = "<font color=\"#FF8C00\">", Suffix = "<font color=\"#FFFFFF\">" },
                new(Localizer["ct"]) { Prefix = "<font color=\"#87CEFA\">", Suffix = "<font color=\"#FFFFFF\">" }
            };

            addZoneMenu.AddItem(new MenuItem(MenuItemType.Choice, new MenuValue($"{Localizer["menu.Type"]} "), zoneTypes, true));
            addZoneMenu.AddItem(new MenuItem(MenuItemType.Choice, new MenuValue($"{Localizer["menu.Teams"]} "), teams, true));
            addZoneMenu.AddItem(new MenuItem(MenuItemType.Input, new MenuValue($"{Localizer["menu.Name"]} ")));

            var delayCheck = _playerData[player].AddZoneMenu!.Items[0].Option != 0 && _playerData[player].AddZoneMenu!.Items[0].Option != 4;

            addZoneMenu.AddItem(delayCheck
                ? new MenuItem(MenuItemType.Input, new MenuValue($"{Localizer["menu.Delay"]} "), new MenuValue($" {Localizer["menu.Seconds"]}"))
                : new MenuItem(MenuItemType.Spacer));

            addZoneMenu.AddItem(_playerData[player].AddZoneMenu!.Items[0].Option == 1
                ? new MenuItem(MenuItemType.Input, new MenuValue($"{Localizer["menu.Damage"]} "), new MenuValue($" {Localizer["menu.PerSecond"]}"))
                : new MenuItem(MenuItemType.Spacer));

            addZoneMenu.AddItem(new MenuItem(MenuItemType.Button, [new CustomButton(Localizer["menu.Save"], SaveZone)]));

            addZoneMenu.Items[3].DataString = "0.0";
            addZoneMenu.Items[4].DataString = "10";
        }

        if (_playerData[player].AddZoneMenu != null)
        {
            addZoneMenu.Points = _playerData[player].AddZoneMenu!.Points;
            addZoneMenu.Option = _playerData[player].AddZoneMenu!.Option;
            addZoneMenu.LastShot = _playerData[player].AddZoneMenu!.LastShot;

            for (var i = 0; i < _playerData[player].AddZoneMenu!.Items.Count; i++)
            {
                addZoneMenu.Items[i].Option = _playerData[player].AddZoneMenu!.Items[i].Option;
                addZoneMenu.Items[i].DataString = _playerData[player].AddZoneMenu!.Items[i].DataString;
            }
        }

        _playerData[player].AddZoneMenu = addZoneMenu;

        Menu.AddMenu(player, addZoneMenu, (buttons, menu, selectedItem) =>
        {
            if (buttons == MenuButtons.Input)
            {
                if (menu.Option == 2 && selectedItem!.DataString.Length > 16)
                    selectedItem.DataString = selectedItem.DataString[..16];

                if (menu.Option == 3)
                {
                    if (!float.TryParse(selectedItem!.DataString, out var delay))
                    {
                        player.PrintToChat($"{Prefix}{Localizer["invalidInput", selectedItem.DataString, "float"]}");
                        selectedItem.DataString = "0.0";
                    }
                    else
                        selectedItem.DataString = delay.ToString("0.0");
                }

                if (menu.Option == 4 && !int.TryParse(selectedItem!.DataString, out _))
                {
                    player.PrintToChat($"{Prefix}{Localizer["invalidInput", selectedItem.DataString, "int"]}");
                    selectedItem.DataString = "10";
                }
            }

            if (buttons is MenuButtons.Left or MenuButtons.Right && selectedItem is { Type: MenuItemType.Choice })
            {
                Menu.PopMenu(player, menu);
                BuildAddZoneMenu(player);
            }

            if (buttons != MenuButtons.Select || selectedItem is not { Type: MenuItemType.Button })
                return;

            var customButton = (CustomButton)selectedItem.Values![selectedItem.Option];
            customButton.Callback.Invoke(player);
            Menu.ClearMenus(player);
        });
    }

    private void BuildViewZoneMenu(CCSPlayerController player)
    {
        var viewZoneMenu = new AddZoneMenu(new MenuValue(Localizer["menu.View"]) { Suffix = "<font class=\"fontSize-m\">" });
        viewZoneMenu.AddItem(new MenuItem(MenuItemType.Text, new MenuValue(Localizer["menu.None"])));

        /*
        if (_zones.Count == 0)
            viewZoneMenu.AddItem(new MenuItem(MenuItemType.Text, new MenuValue(Localizer["menu.None"])));
        else
        {
            if (_zones.Count == 0)
                return;

            var zones = new List<MenuValue>[_zones.Count];

            for (var i = 0; i < _zones.Count; i++)
                zones.Add(new ZoneValue($"{i}", _zones[i]));
            

            //var zones = _zones.Select(zone => new ZoneValue(zone.Name, zone)).Cast<MenuValue>().ToList();
            viewZoneMenu.AddItem(new MenuItem(MenuItemType.Choice, zones, true));

            viewZoneMenu.AddItem(new MenuItem(MenuItemType.Spacer));
            viewZoneMenu.AddItem(new MenuItem(MenuItemType.Button, [new MenuValue(Localizer["menu.Teleport"]), new MenuValue(Localizer["menu.Delete"])]));
        }
        */

        Menu.AddMenu(player, viewZoneMenu, (buttons, menu, selectedItem) =>
        {

        });
    }
}

public class CustomButton(string value, Action<CCSPlayerController> callback) : MenuValue(value)
{
    public Action<CCSPlayerController> Callback { get; } = callback;
}

public class AddZoneMenu(MenuValue title) : MenuBase(title)
{
    public Vector_t?[] Points { get; set; } = [null, null];
    public float LastShot { get; set; }
}

public class ZoneValue(string title, Zone zone) : MenuValue(title)
{
    public Zone Zone { get; set; } = zone;
}