# CreaturesStarsWatcher

Displays an on-screen banner as soon as a creature with **n stars** spawns near
you, so you never miss a two star moose (or any other creature you configure).

# Description

- Alerts when a creature with **n stars or more** appears (`n` is configurable, default 2).
- The creatures to watch are configurable (comma separated list with `*` wildcard).
- Detection radius is configurable (default 100 m, `0` = no distance filter). Zones and their
  creatures are unloaded beyond the game's simulation distance (about 200 m with the default
  settings), so the setting is capped at 500 m instead of pretending to be unlimited.
- Covers fresh spawns (spawn system, spawners, births, summons) **and** creatures
  already present when you enter or load a zone.
- One alert per creature, with a cooldown, shown as a fading banner with the
  creature name, its stars and the live distance.
- The banner disappears as soon as the creature is unloaded (zone change) or
  dead, and shows up again when its zone is loaded anew.
- The closest detected creature wins the banner: loaded zones are scanned nearest
  first, and a nearer creature takes the banner over.

This mod is client-side only: you do not need to install it on the server.

# Configuration

The file `BepInEx/config/orax.CreaturesStarsWatcher.cfg` is created on the first
launch. Every setting can also be changed in game with **ConfigurationManager**.

| Section | Key | Default | Description |
| --- | --- | --- | --- |
| General | Enabled | `true` | Master switch. |
| General | DebugLogging | `false` | Log every detection decision (troubleshooting). |
| Detection | StarsMinimum | `2` | Minimum stars required (alerts trigger at `>=` this value). |
| Detection | Creatures | `*` | Creatures to watch, e.g. `Moose, Troll*`. `*` = everything. |
| Detection | MaxDistance | `100` | Maximum distance in metres (capped at 500), `0` = no distance filter. |
| Detection | IncludeLoadedCreatures | `true` | Also alert for creatures found when entering a zone. |
| Detection | ReAlertOnZoneLoad | `true` | Alert again when a zone is loaded with the same creature. |
| Detection | CooldownSeconds | `10` | Minimum delay between two alerts; a nearer creature is not held back. |
| Detection | ScanIntervalSeconds | `1` | How often loaded zones are scanned. |
| Alert | DurationSeconds | `6` | How long the banner stays on screen. |
| Alert | Position | `TopCenter` | `TopCenter`, `Center` or `BottomCenter`. |
| Alert | ShowDistance | `true` | Show the live distance in the banner. |
| Alert | MaxQueuedAlerts | `3` | Maximum number of alerts waiting to be displayed, closest first. |

# Installation

This mod is client-side only. You do not need to install it on the server.

## Using a mod manager (recommended)
You can install the mod using Vortex, R2ModMan or Gale.

## Manual
Install BepInExPack for Valheim.
Copy the DLL file to `Steam\steamapps\common\Valheim\BepInEx\plugins\`

# Misc.

Source code: [GitHub repository](https://github.com/orax-Valheim-mods/)
