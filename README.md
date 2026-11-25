# Classic Us Colorblind Text

A BepInEx accessibility mod for **Classic Us** that adds clear, color-coded text identifiers to player names across the entire game interface, matching modern *Among Us* UI accessibility conventions.

## Features
* **Overworld & Gameplay**: Adds a clean two-line colorblind text label underneath player nameplates.
* **Meeting & Voting UI**: Appends parenthetical color labels to player names on voting cards.
* **Alive Players List**: Integrates clean color labels directly into HUD player trackers.
* **Chat Bubbles**: Displays modern inline parenthetical color tags next to usernames (e.g., `PlayerName (Color)`) without interfering with chat messages or avatar icons.
* **Configurable**: Includes a built-in BepInEx config entry to enable or disable the mod on the fly.

---

## Building the Release Package

To compile your mod into a distribution-ready `.dll` file:

1. Open your terminal or command prompt in your mod's project directory.
2. Run the release build command:
   ```bash
   dotnet publish -c Release
