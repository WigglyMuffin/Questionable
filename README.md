# Questionable

**A tiny quest helper plugin for Final Fantasy XIV.**

*Automate your questing experience with intelligent pathfinding and seamless quest completion*

[![GitHub Release](https://img.shields.io/github/v/release/WigglyMuffin/Questionable?style=for-the-badge&logo=github&color=brightgreen)](https://github.com/WigglyMuffin/Questionable/releases)
[![Discord](https://img.shields.io/badge/Discord-Join%20Server-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/pngyvpYVt2)

---

## Features

- **Automatic Quest Completion**: Handles quest navigation, dialogue, and turn-ins automatically
- **Navmesh Integration**: Uses advanced pathfinding to navigate between quest objectives
- **MSQ Support**: Main Scenario Quest automation with priority handling
- **Allied Society Quests**: Automated daily quest completion for all tribes
- **Class/Job Quests**: Automatic completion of class and job-specific quests
- **Aetheryte Shortcuts**: Smart teleportation to optimize travel time
- **Quest Validation**: Built-in validation system to ensure quest data integrity
- **Manual Override**: Full manual control when needed with step-by-step execution

## Installation

Add the following URL to your Dalamud plugin repositories: 

`https://github.com/WigglyMuffin/DalamudPlugins/raw/main/pluginmaster.json`

**Installation Steps:**
1. Open XIVLauncher/Dalamud
2. Go to Settings → Experimental
3. Add the repository URL above
4. Go to Plugin Installer
5. Search for "Questionable" and install

## Required Dependencies

Questionable requires the following plugins to function properly:

### Core Dependencies
- **[vnavmesh](https://github.com/awgil/ffxiv_navmesh/)** - Handles navigation within zones
- **[Lifestream](https://github.com/NightmareXIV/Lifestream)** - Used for aethernet travel in cities
- **[TextAdvance](https://github.com/NightmareXIV/TextAdvance)** - Automatically accepts quests, skips cutscenes and dialogue

### Combat Plugins (Choose One)
- **[Boss Mod (VBM)](https://github.com/awgil/ffxiv_bossmod)**
- **[Wrath Combo](https://github.com/PunishXIV/WrathCombo)**
- **[Rotation Solver Reborn](https://github.com/FFXIV-CombatReborn/RotationSolverReborn)**

### Recommended Plugins
- **[CBT (Automaton)](https://github.com/Jaksuhn/Automaton)** - 'Sniper no sniping' feature
- **[Pandora's Box](https://github.com/PunishXIV/PandorasBox)** - 'Auto Active Time Maneuver' feature
- **[NotificationMaster](https://github.com/NightmareXIV/NotificationMaster)** - Out-of-game notifications

## Usage

### Basic Commands
- `/questionable` or `/qst` - Open the main plugin window
- `/qst start` - Starts doing quests
- `/qst stop` - Stops doing quests
- `/qst reload` - Reload all quest data
- `/qst which` - Shows all quests starting with your selected target
- `/qst zone` - Shows all quests starting in the current zone (only includes quests with a known quest path, and currently visible unaccepted quests)

### Getting Started
1. Accept a quest manually or let the plugin handle MSQ progression
2. Open the Questionable window (`/qst`)
3. Click the **Play** button to start automation
4. The plugin will handle navigation, dialogue, and quest completion

### Quest Types Supported
- **Main Scenario Quests (MSQ)** - Full automation support
- **Side Quests** - Most side quests are supported
- **Class/Job Quests** - Automated with priority handling
- **Allied Society Quests** - Daily quest automation
- **Delivery Quests** - Custom delivery and supply missions

### Limitations
- **Dungeons**: Certain dungeons must be completed manually or with other automation tools
- **Single Player Duties**: Certain single player duties require manual completion
- **Combat**: Requires a combat plugin or manual intervention
- **Some Complex Mechanics**: May require manual intervention

## Configuration

Access configuration through the main plugin window or `/questionable config`:

- **Combat Module**: Choose your preferred combat automation plugin
- **Stop Conditions**: Set level or quest-based stopping points  
- **Advanced Settings**: Debug options and additional status information
- **Quest Priority**: Manage quest execution order

## Support & Community

- **Discord**: Join our community for support, updates, and discussions: [https://discord.gg/pngyvpYVt2](https://discord.gg/pngyvpYVt2)
- **Bug Reports**: Use [GitHub Issues](https://github.com/WigglyMuffin/Questionable/issues) for bug reports
- **Feature Requests**: Submit suggestions via GitHub Issues

## Quest Coverage

**Supported Expansions:**
- ✅ A Realm Reborn (ARR)
- ✅ Heavensward (HW) 
- ✅ Stormblood (SB)
- ✅ Shadowbringers (ShB)
- ✅ Endwalker (EW)
- ✅ Dawntrail (DT)

**Quest Types:**
- ✅ Main Scenario Quests
- ✅ Class/Job Quests
- ✅ Allied Society Quests  
- ✅ Aether Current Quests
- ✅ Side Quests (Most)
- ⚠️ Custom Delivery (Partial)

**Quest Database & Tools:**
- **[Quest Coverage](https://wigglymuffin.github.io/FFXIV-Tools/)** - Explore quest coverage and paths

## Disclaimer

**Use at your own risk.** While this plugin automates quest completion and is designed to simulate normal player behaviour, never leave it unattended as automation always carries inherent risks.

## License

**As of version 6.9, this project has transitioned from AGPL v3 to proprietary licensing.**
- Version 6.8 and earlier: GNU Affero General Public License v3.0
- Version 6.9 and later: All Rights Reserved (proprietary)

See the [LICENSE](LICENSE) file for details.

## Credits

- **Original Author**: Liza Carvelli
- **Current Maintainer**: WigglyMuffin  
- **Contributors**: All the amazing people who contribute quest paths and improvements
