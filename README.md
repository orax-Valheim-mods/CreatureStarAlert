Mod for Valheim that displays an on-screen banner as soon as a creature with **n stars** appears
near the player, so a two star moose never goes unnoticed.

# Example

![example](https://i.imgur.com/vEnnDAV.jpeg)

# Features

- Alerts when a creature with `n` stars or more appears (`n` is configurable, default 2).
- The creatures to watch are configurable: a comma separated list with `*` wildcard.
- Configurable detection radius (default 100 m, `0` = no distance filter: the game itself unloads
  zones and their creatures beyond its simulation distance, about 200 m with the default settings).
- The banner shows the localised name, the stars and the live distance and disappears as soon as the
  creature is unloaded, dead or out of the detection radius.
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
