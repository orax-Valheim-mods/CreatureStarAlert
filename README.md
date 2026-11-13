Mod for Valheim that displays an on-screen banner as soon as a creature with **n stars** appears
near the player, so a two star moose never goes unnoticed.

# Example

![example](https://i.imgur.com/vEnnDAV.jpeg)

# Features

- Alerts when a creature with `n` stars or more appears (`n` is configurable, default 2).
- The creatures to watch are configurable: a comma separated list with `*` wildcard.
- Tamed creatures are ignored by default (`Ignore tamed`), so a farm full of starred animals
  stays quiet. Turn the setting off to be alerted about them as well.
- Configurable detection radius (default 100 m, `0` = no distance filter: the game itself unloads
  zones and their creatures beyond its simulation distance, about 200 m with the default settings).
- The banner shows an arrow pointing towards the creature the way the camera faces, the localised
  name, the stars and the live distance and disappears as soon as the creature is unloaded, dead or
  out of the detection radius. The arrow can be turned off (`Show arrow`), and its glyph
  (`Arrow glyph`) and colour (`Arrow color`) are configurable: the glyph is expected to point
  straight ahead and is turned towards the creature (the default is `➤`, and
  `Arrow rotation offset` adds degrees to the rotation, 90 by default).
- The layout of the banner text is set by `Banner format`, a template with the `{arrow}`, `{name}`,
  `{stars}` and `{distance}` placeholders (each part keeps its configured colour).
- The closest detected creature wins the banner: a nearer one takes it over, and only the best
  alerts are kept waiting behind it. With the `Prefer most stars` setting enabled, the creature with
  the most stars wins instead, ties being broken by distance.

# Installation

Client-side only: you do not need to install it on the server.

## Using a mod manager (recommended)

You can install the mod using Vortex or Gale.

## Manual

Install BepInExPack for Valheim. Copy the DLL file to
`Steam\steamapps\common\Valheim\BepInEx\plugins\`

# Misc.

Source code: https://github.com/orax-Valheim-mods/
