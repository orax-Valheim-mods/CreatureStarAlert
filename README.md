# CreaturesStarsWatcher

BepInEx mod for Valheim that displays an on-screen banner as soon as a creature
with **n stars** appears near the player, so a two star moose never goes unnoticed.

## Features

- Alerts when a creature with `n` stars or more appears (`n` is configurable, default 2).
- The creatures to watch are configurable: a comma separated list with `*` wildcard.
- Configurable detection radius (default 100 m, `0` = unlimited).
- Covers fresh spawns (spawn system, spawners, births, summons) **and** creatures
  already present when you enter or load a zone.
- Anti-spam: one alert per creature plus a global cooldown.
- The banner shows the localised name, the stars and the live distance.

Client-side only: you do not need to install it on the server.

## Project layout

| Folder | Contents |
| --- | --- |
| `CreaturesStarsWatcher/Config` | BepInEx configuration and its interfaces |
| `CreaturesStarsWatcher/Detection` | Filters, alert manager, zone scanner, Harmony patch |
| `CreaturesStarsWatcher/UI` | On-screen banner (the `IAlertSink` implementation) |

## Build

```
dotnet build -c Debug
```

The post-build script deploys the DLL to the path set in `Environment.props`
(`MOD_DEPLOYPATH`). `Release` packages the Thunderstore zip instead.

## Configuration

See [CreaturesStarsWatcher/README.md](CreaturesStarsWatcher/README.md) for the
full list of settings.
