https://www.youtube.com/watch?v=AkaBeFJcTv8

`addons/counterstrikesharp/configs/plugins/AntiRush/AntiRush.json`
```json
{
  "Version": 8,
  "Messages": "simple",  // "simple", "detailed", "off"
  "DrawZones": false,    // Draw zones
  "Warmup": false,       // Do zones in warmup
  "DisableOnBombPlant": true, // Disable rush zones when bomb is planted
  "RestartOnLoad": true, // Use mp_restartgame 1 when loading / reloading
  "NoRushTime": 0,       // How many seconds after round start should rush zones disable (Defined below in RushZones)
  "NoCampTime": 0,       // How many seconds after round start should camp zones enable (Defined below in CampZones)
  "RushZones": [ 0, 2, 3, 4 ],
  "CampZones": [ 1 ],    // 0 - Bounce, 1 - Hurt, 2 - Kill, 3 - Teleport, 4 - Wall
  "Countdown": [ 60, 30, 15, 10, 5, 3, 2, 1 ],
  "MinPlayers": 1,       // Minimum total players currently in a team for zones to work
  "MaxPlayers": 64,      // Maximum total players for zones to work, if currentPlayers >= MaxPlayers zones are disabled
  "ConfigVersion": 8
}
```
