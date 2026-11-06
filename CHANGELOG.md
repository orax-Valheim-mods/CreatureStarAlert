## [0.2.0] - 2026-11-06

### 🚀 Features

- *(detection)* Ignore tamed creatures

### 🐛 Bug Fixes

- *(banner)* Drop the banner when the mod is disabled
- *(detection)* Re-evaluate the alerts when a detection setting changes

### 📚 Documentation

- Add example image to README.md
- *(workflows)* Align the upload callers with the publish template

### ⚙️ Miscellaneous Tasks

- *(publish)* Add Thunderstore upload workflow
- *(changelog)* Reset cliff.toml to default template
## [0.1.0] - 2026-10-27

### 🚀 Features

- *(config)* Add BepInEx settings for star creature alerts
- *(detection)* Turn star creatures into alerts
- *(ui)* Add fading on-screen banner for star alerts
- Wire detection and banner to creature level ups
- *(config)* Add ReAlertOnZoneLoad setting
- *(detection)* Report loaded creatures closest first
- *(ui)* Keep the closest detected creature on the banner
- *(detection)* Let a nearer creature break the cooldown
- *(config)* Add a banner font size setting
- *(config)* Allow StarsMinimum up to 9
- *(config)* Add an option to alert on the most starred creature
- *(detection)* Rank the loaded creatures on the alert criterion
- *(ui)* Show the creature winning the criterion on the banner
- *(config)* Display the setting keys with spaces
- *(ui)* Keep the banner on screen while Duration seconds is 0
- *(ui)* Add the corner and side positions to the banner
- *(ui)* Position the banner with an anchor and percentages
- *(config)* Let the banner position be given in pixels
- *(config)* Default the banner to the centre near the top
- *(config)* Add settings for the text colours and the rectangle
- *(ui)* Draw the banner with the configured colours and rectangle
- *(config)* Default the scan interval to 3 seconds
- *(config)* Default Duration seconds to 0

### 🐛 Bug Fixes

- *(detection)* Raise the alert again when a zone is loaded
- *(ui)* Drop the banner when the creature is unloaded
- *(config)* Bound MaxDistance to the game zone loading range
- *(ui)* Let nearby creatures take the banner over
- *(detection)* Drop and forget alerts outside the detection radius
- Write forward slashes in release ZIP entry paths

### 📚 Documentation

- Describe the mod and its configuration
- Document the ReAlertOnZoneLoad setting
- Mention that the banner follows the creature lifetime
- Describe MaxDistance as bounded by zone unloading
- Document the closest creature priority
- Mention that a nearer creature is not held back by the cooldown
- Document the banner font size setting
- Document the alert creature criterion
- Use the readable setting keys in the documentation
- Document the indefinite banner duration
- Document how far ahead the candidate must be
- List the nine banner positions
- Document the anchor and percentage positions
- Document the position unit
- Update the default banner position
- Document the text colours and rectangle settings
- Use the new mod name in the documentation
- Match the scan interval default in the settings table
- *(config)* Explain that Position x and y move the banner
- Drop the cooldown from the readmes
- Match the Duration seconds default
- Document the banner drop outside the detection radius
- Rewrite the root README as the mod page

### 🚜 Refactor

- [**breaking**] Rename the mod to CreatureStarAlert
- *(config)* [**breaking**] Remove the Cooldown seconds option

### ⚙️ Miscellaneous Tasks

- Init
- *(package)* Set Thunderstore metadata for CreaturesStarsWatcher
- *(publish)* Call the reusable workflows from orax-modding/publish
- Enforce LF line endings via .gitattributes and .editorconfig
- *(ci)* Generate changelog and upload release to GitHub in post-build script
- *(manifest)* Set website_url and align on BepInExPack 5.4.2351
- Add a prettier configuration
- *(package)* Stop tracking the packaged README
- *(package)* Update the mod icon
- *(publish)* Use 7-Zip instead of .NET compression for release archives
- *(release)* Bump version to v0.1.0

### 💼 Other

- Drop the UnityEngine.ProfilerModule reference (MSB3245)
