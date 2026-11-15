# Not Left 4 Dead (NL4D)

**Not Left 4 Dead** is an immersive Mount & Blade II: Bannerlord (v1.5.7+) mod that expands the Native battlefield wreckage system. After battles in Calradia, surviving soldiers, fallen scouts, and wounded warriors cling to life amidst the ruins of land and naval clashes. Encounter them, hear their tales, nurse them back to health, recruit them into your ranks, or grant them freedom to find their way home.

---

## Features

- **Battle Wreckage Survivors**: Integrates seamlessly with Bannerlord v1.5.7's `BattleWreckage` system. Field and naval wreckage sites now feature dynamic survivors.
- **Dynamic Context Extraction**: Survivors remember their battle context: their commanding lord, former faction, opposing army, casualties suffered, and whether they fought on the winning or losing side.
- **Roleplay & Interactions**:
  - **Recruit**: Convince survivors to swear allegiance and join your party.
  - **Release / Shelter**: Offer aid, rations, and coins for honor, renown, and diplomatic goodwill.
  - **Enslave / Imprison**: Take survivors as captives for rogue playstyles.
- **Cultural & Faction Immersion**: Survivor dialogues and templates accurately reflect regional cultures and factions.
- **Non-Invasive Architecture**: Uses clean late-initialization, standard TaleWorlds save system definitions (`SaveableTypeDefiner`), and optional Mod Configuration Menu (MCM v5) integration without hard dependencies.

---

## Requirements

- **Mount & Blade II: Bannerlord** (v1.5.7 or compatible)
- Official Modules: `Native`, `SandBoxCore`, `Sandbox`, `StoryMode`
- *(Optional)* **Mod Configuration Menu (MCMv5)**: For in-game configuration settings. Fully functional with JSON fallback when MCM is not installed.

---

## Installation

1. Download the latest release (`NL4D-Latest.zip`).
2. Extract the archive into your Bannerlord `Modules/` directory:
   ```
   Mount & Blade II Bannerlord/Modules/NL4D/
       SubModule.xml
       bin/Win64_Shipping_Client/NL4D.dll
       bin/Win64_Shipping_Client/NL4D.MCMBridge.dll
       ...
   ```
3. Enable **Not Left 4 Dead** in your Bannerlord launcher or mod manager.

---

## Developer & Build Lifecycle

Developers must adhere to the 5-stage mod release lifecycle:

1. **Code**: Make changes in source files or XML configurations.
2. **Debug Build**:
   ```bash
   dotnet build -c Debug
   ```
   Increments the internal debug version and outputs binaries to your local game installation.
3. **Test**: Run Bannerlord and verify features in-game.
4. **Hotfix / Update Build**:
   - For bug fixes:
     ```bash
     dotnet build -c Hotfix
     ```
   - For new features:
     ```bash
     dotnet build -c Update
     ```
   Automatically bumps `SubModule.xml` version via `UpdateVersion.ps1`.
5. **Commit & Push**:
   Commit with descriptive prefixes (`Added: ...`, `fix: ...`) to feed into GitHub Release notes and Nexus Mods changelog.

---

## License

Created for the Mount & Blade II: Bannerlord community. All rights reserved.
