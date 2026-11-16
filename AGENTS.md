# Agent Knowledge Base: Not Left 4 Dead (NL4D)

This document provides technical insights, architectural patterns, and development guidelines for AI agents and future developers working on the **Not Left 4 Dead** mod.

---

## Project Architecture

### 1. Mod Initialization (`NL4DSubModule.cs`)
The mod implements standard `MBSubModuleBase` lifecycle hooks with late-initialization:
- **`OnSubModuleLoad`**: Activates the optional MCM bridge (`TryActivateOptionalMcmBridge()`). Loads `NL4D.MCMBridge.dll` via reflection only if `MCMv5` is detected in the current AppDomain or runtime assembly loader.
- **`OnBeforeInitialModuleScreenSetAsRoot`**: Initializes core JSON settings (`Nl4dSettingsManager.Initialize()`).
- **`OnGameStart`**: When the game type is `Campaign`, casts `gameStarterObject` to `CampaignGameStarter` and registers `WreckageSurvivorBehavior`.

### 2. Optional Dependencies (Bridge Pattern)
To eliminate hard dependencies on MCM (Mod Configuration Menu):
- Core mod logic resides in `NL4D.dll`.
- MCM-specific adapters and settings UI reside in `NL4D.MCMBridge.dll`.
- At startup, `NL4DSubModule` checks for `MCMv5`. If present, `BridgeBootstrap.TryRegister` connects the MCM property provider.
- If MCM is absent, `Nl4dSettingsManager` seamlessly falls back to `config.json`.

### 3. Save System Architecture (`NL4DSaveDefiner.cs`)
The mod integrates with TaleWorlds' native binary save system via `SaveableTypeDefiner`:
- **Unique Base ID**: `2849170` (reserved to prevent conflicts with other community mods).
- **Class Definitions**:
  - `AddClassDefinition(typeof(RescuedSurvivorRecord), 1);`
- **Container Definitions**:
  - `ConstructContainerDefinition(typeof(List<RescuedSurvivorRecord>));`
- Standard primitive types and `List<string>` are already registered by Native `SaveableBasicTypeDefiner` and `SaveableCampaignTypeDefiner`.

### 4. Core Data Structures (`NL4D.Data`)
- **`WreckageBattleContext`**: Transient runtime representation of a battle wreckage site. Extracts data from Bannerlord 1.5.7's `BattleWreckage` (former leader, former faction, enemy faction, casualties, hours elapsed, land vs. naval wreckage, position).
- **`RescuedSurvivorRecord`**: Persistent saved record tracking survivors who were rescued, recruited, or released. Marked with `[SaveableField]` attributes (1 through 7) for save compatibility.

### 5. Automatic Version Management (`NL4D/Build/UpdateVersion.ps1`)
Build-time version bumping script invoked before `PostBuildEvent`:
- **Version format**: `vMAJOR.UPDATE.HOTFIX.DEBUG` (e.g., `v1.0.0.1`).
- `Debug` configuration: Increments DEBUG segment.
- `Hotfix` configuration: Increments HOTFIX and DEBUG segments.
- `Update` configuration: Increments UPDATE and DEBUG, resets HOTFIX.
- `BigUpdate` configuration: Increments MAJOR and DEBUG, resets UPDATE and HOTFIX.

---

## Development Guidelines

### Developer Release & Build Lifecycle (Mandatory)
Every developer and agent must strictly follow this 5-stage lifecycle:
1. **Code (Kodla)**: Make necessary changes to code or XML.
2. **Debug Build (Debug buildi al)**: Build in `Debug` configuration (`dotnet build -c Debug`). Increments the internal debug version segment.
3. **Test (Test et)**: Verify gameplay and sanity check in-game or via tests.
4. **Hotfix / Update Build (Hotfix/Update buildi al)**:
   - Run `dotnet build -c Hotfix` for bugfixes, or `dotnet build -c Update` for feature releases.
   - Automatically increments the version in `SubModule.xml` via `UpdateVersion.ps1`.
5. **Commit & Push with Changelog (Commit ve Push)**:
   - Write action-oriented commit messages (`Added: ...`, `fix: ...`) because commit messages automatically generate the Nexus Mods changelog.
   - Always ask for explicit user permission before `git push origin main`.

### Internationalization
- **Source Code**: All comments, variable names, and logic must be in **English**.
- **User Interface**: All user-facing strings must use the localization system (`{=ID}Text`).
- **Translations**: Localized XMLs are located in `NL4D/ModuleData/Languages/`. Ensure both `EN` and `TR` are updated when adding new strings.

### Description Maintenance Rule (Mandatory)
- **`DESCRIPTION.md` (Nexus Mods BBCode) Must Stay in Sync**: `DESCRIPTION.md` is formatted in **Nexus Mods BBCode** (`[size=4][b]...[/b][/size]`, `[list]`, `[color=#D4D4D8]`) for direct copy-paste to Nexus Mods. Whenever any gameplay mechanic, MCM/JSON setting, console command, dialogue flow, dependency, supported Bannerlord version, or localization language is added, changed, or removed, agents and developers **MUST** update `DESCRIPTION.md` in the same task/commit so it always accurately reflects the current state of the mod.

---

## File Structure

```
NotLeft4Dead/
├── NotLeft4Dead.sln
├── DESCRIPTION.md
├── README.md
├── AGENTS.md
├── .gitignore
├── .github/
│   └── workflows/
│       └── release.yml
├── NL4D/
│   ├── NL4D.csproj
│   ├── SubModule.xml
│   ├── NL4DSubModule.cs
│   ├── Build/
│   │   └── UpdateVersion.ps1
│   ├── Config/              (Owned by Subagent 2)
│   ├── Calculators/         (Owned by Subagent 3)
│   ├── Services/            (Owned by Subagent 3)
│   ├── Behaviors/           (Owned by Subagent 4)
│   ├── Data/                (Owned by Subagent 1)
│   │   ├── WreckageBattleContext.cs
│   │   ├── RescuedSurvivorRecord.cs
│   │   └── NL4DSaveDefiner.cs
│   └── ModuleData/          (Owned by Subagent 5)
│       └── Languages/
└── NL4D.MCMBridge/          (Owned by Subagent 2)
    └── NL4D.MCMBridge.csproj
```
