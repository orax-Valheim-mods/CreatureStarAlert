# CreatureStarAlert

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
- One alert per creature, with a cooldown that a creature winning on the selection
  criterion may break, shown as a fading banner with the creature name, its stars
  and the live distance.
- The banner look is configurable: the colours of the name, the stars and the
  distance, the rectangle behind the text (shown by default, with its own opacity),
  its position, its font size and how long it stays.
- The banner disappears as soon as the creature is unloaded (zone change) or
  dead, and shows up again when its zone is loaded anew.
- The closest detected creature wins the banner: loaded zones are scanned best
  first, and a nearer creature takes the banner over once it leads by a distance
  that shrinks at close range (0.5 to 5 m), so stepping up to another creature a
  few metres away switches the banner. With the `Prefer most stars` setting,
  the creature with the most stars wins instead, ties being broken by distance.

This mod is client-side only: you do not need to install it on the server.

# Configuration

The file `BepInEx/config/orax.CreatureStarAlert.cfg` is created on the first
launch. Every setting can also be changed in game with **ConfigurationManager**.

| Section | Key | Default | Description |
| --- | --- | --- | --- |
| General | Enabled | `true` | Master switch. |
| General | Debug logging | `false` | Log every detection decision (troubleshooting). |
| Detection | Stars minimum | `2` | Minimum stars required (alerts trigger at `>=` this value). |
| Detection | Creature patterns | `*` | Creatures to watch, e.g. `Moose, Troll*`. `*` = everything. |
| Detection | Max distance | `100` | Maximum distance in metres (capped at 500), `0` = no distance filter. |
| Detection | Include loaded creatures | `true` | Also alert for creatures found when entering a zone. |
| Detection | Re alert on zone load | `true` | Alert again when a zone is loaded with the same creature. |
| Detection | Cooldown seconds | `10` | Minimum delay between two alerts; a creature winning on the criterion is not held back. |
| Detection | Scan interval seconds | `3` | How often loaded zones are scanned. |
| Alert | Duration seconds | `6` | How long the banner stays on screen; `0` = until the creature is unloaded or dies. |
| Alert | Anchor | `Center` | Point of the banner placed at the position: `TopLeft`, `TopCenter`, `TopRight`, `Left`, `Center`, `Right`, `BottomLeft`, `BottomCenter`, `BottomRight`. Changing it barely moves the banner: change `Position x` and `Position y` to move the banner on screen. |
| Alert | Position unit | `Percent` | Unit of `Position x` and `Position y`: `Percent` of the screen, or `Pixels` counted from the top left corner. |
| Alert | Position x | `50` | Horizontal position of the anchor: percent of the screen width (`0` = left edge, `100` = right edge) or pixels from the left edge. |
| Alert | Position y | `5` | Vertical position of the anchor: percent of the screen height (`0` = top edge, `100` = bottom edge) or pixels from the top edge. |
| Alert | Name color | `FFFFFFFF` | Colour of the creature name, as a `RRGGBBAA` hex value (no `#`). |
| Alert | Star color | `FFC53BFF` | Colour of the stars, as a `RRGGBBAA` hex value (no `#`). |
| Alert | Distance color | `A8A8A8FF` | Colour of the distance, as a `RRGGBBAA` hex value (no `#`). |
| Alert | Show rectangle | `true` | Draw the rectangle behind the banner text; `false` shows the text alone. |
| Alert | Rectangle opacity | `65` | Opacity of the rectangle in percent: `0` = invisible, `100` = opaque black. |
| Alert | Show distance | `true` | Show the live distance in the banner. |
| Alert | Max queued alerts | `3` | Maximum number of alerts waiting to be displayed, best first. |
| Alert | Font size | `0` | Font size in pixels, `0` = automatic (scaled to the screen height). |
| Alert | Prefer most stars | `false` | With several creatures, show the one with the most stars instead of the closest. |

# Installation

This mod is client-side only. You do not need to install it on the server.

## Using a mod manager (recommended)
You can install the mod using Vortex, R2ModMan or Gale.

## Manual
Install BepInExPack for Valheim.
Copy the DLL file to `Steam\steamapps\common\Valheim\BepInEx\plugins\`

# Misc.

Source code: [GitHub repository](https://github.com/orax-Valheim-mods/)
