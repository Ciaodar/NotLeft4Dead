# Not Left 4 Dead (NL4D)

**Not Left 4 Dead** is a **100% Zero-Harmony-Patch** Mount & Blade II: Bannerlord (**v1.5.7+**) mod that brings the new **Battle Wreckage** mechanic to life.

When you finish scavenging a battlefield or naval wreckage, a survivor clinging to life amidst the carnage may crawl out of the debris. Hear their story, ask about the battle, learn who abandoned them, and decide whether to take them into your company on the spot or send them to recover at the nearest tavern — where they will remember your mercy and join you for **free** whenever you meet again.

---

## Key Features

### 1. Zero-Harmony-Patch Integration (v1.5.7 Battle Wreckage)
- Hooks cleanly into Bannerlord **v1.5.7**'s native `BattleWreckage` system using official `CampaignEvents` and `Game.Current.AfterTick` — **0 Harmony patches**, maximum stability, and full save-game safety.
- Triggers **right when the 9-hour wreckage investigation progress bar completes** (on top of the wounded recovery decision or results screen) without interrupting the vanilla wreckage flow.

### 2. Battle-Aware Lore & Clickable Encyclopedia Links
- Pulls real historical context directly from the `BattleWreckage` site:
  - **Commanding Lords & Factions**: Both the survivor's former lord (`{SURVIVOR_LORD_NAME}`) and the opposing commander (`{ENEMY_LORD_NAME}`), plus their kingdoms/clans.
  - **Casualties & Elapsed Time**: Total battle casualties and how many hours the survivor lay bleeding among the fallen.
  - **Winner or Defeated Side**: Survivors can be abandoned retainers left behind in a rout *or* wounded veterans forgotten by a victorious army rushing after loot.
  - **Land & Naval Wreckages**: Dedicated narrative lines for both land battles and shattered naval engagements.
- Every Lord, Faction, and Settlement mentioned in dialogue is rendered as a **clickable Encyclopedia link**.
- Every rescued survivor receives a **custom Encyclopedia biography** recording the battle, the lord who left them behind, and you as their rescuer, and is immediately marked as **met (`SetHasMet`)**.

### 3. Interactive Multi-Option Rescue Dialogue Hub
Before deciding their fate, you can question the survivor in any order:
- *"What actually happened here? Tell me of the battle."*
- *"Who are you really? What is your story?"*
- *"What happened to you? How bad are your wounds?"*

Then choose:
- **Recruit Immediately**: They join your party for **0 gold**, heavily wounded (**15%–20% HP** by default), with **+100 personal relation**.
- **Send to Nearest Town**: If you lack space or prefer to wait, they travel to the nearest town tavern (`+100` relation intact) and roam Calradia's taverns like a normal wanderer.

### 4. Tavern Reunion & Free Recruitment
- Unrecruited survivors never forget you. Meet them later in any town tavern and a high-priority custom reunion dialogue triggers:
  - *"I have not forgotten how you pulled me from the jaws of death... I still wish with all my heart to join your company."*
  - Recruit them anytime for **0 gold** or ask them to keep waiting.
- **Native Purge Protection**: Protected from Native's `CompanionsCampaignBehavior.TryKillCompanion` random wanderer purge.
- **World FIFO Cap**: To prevent tavern bloat over long campaigns, unrecruited survivors in the world are capped (default: **5**). When a 6th is left behind, the oldest unrecruited survivor is cleanly retired.

### 5. Gradual Spawn Scaling & Lore-Friendly Custom Name Pools
- Requires at least one real **NPC Lord** on either side of the battle (player-led battles are excluded).
- Spawn probability scales with total battle casualties — reaching **100%** at **1,500+ casualties** (configurable).
- Uses native culture-matched Wanderer templates for varied faces, hair, beards, skills, traits, and gear, paired with **gender-aware custom culture name & epithet pools** (customizable in `config.json`).

### 6. Optional MCMv5 Support + JSON Fallback
- Full **Mod Configuration Menu (MCM v5)** integration with instant live hot-reload (no restart or console reload required):
  - Mod Enabled toggle
  - Min Base Spawn Chance, Guaranteed Casualty Threshold (default `1500`), Spawn Chance Multiplier, Winning Side Weight
  - **Allow Exceeding Companion Limit** toggle
  - World FIFO Cap (default `5`), Wounded HP Ratio Min/Max, Relation Bonus (default `100`), Custom Name Pool toggle, Debug Force Spawn
- No hard dependency on MCM: if MCM is absent, settings load from `Modules/NL4D/config.json` (with fallback to `Documents/Mount and Blade II Bannerlord/Configs/NL4D/config.json`).

---

## Requirements & Compatibility

- **Mount & Blade II: Bannerlord**: `v1.5.7` or newer
- **Required Modules**: `Native`, `SandBoxCore`, `Sandbox`, `StoryMode`
- **Optional**: `Mod Configuration Menu (MCM v5)` *(no dependency conflict if not installed)*
- **Harmony Required?**: **No** (`0` Harmony patches)
- **Save-Game Compatible?**: Safe to add mid-campaign (`SaveableTypeDefiner` base ID `2849170`).
- **Languages**: Full **English (`EN`)** and **Turkish (`TR`)** localization.

---

## Installation

1. Download `NL4D-Latest.zip` (or extract via Vortex / Mod Manager).
2. Place the `NL4D` folder into your Bannerlord `Modules/` directory:
   ```text
   Mount & Blade II Bannerlord/Modules/NL4D/
   ```
3. In the Bannerlord Launcher, enable **Not Left 4 Dead** (place below `Mod Configuration Menu v5` if using MCM).

---

## Developer / QA Console Commands

Open the in-game console (`ALT + ~` or `ALT + "`):
- `nl4d.spawn_test_wreckage` — Spawns a 1,600-casualty Epic `BattleWreckage` between two Kingdom Lords right next to your party and guarantees a survivor spawn on investigation.
- `nl4d.force_next` — Forces the next wreckage investigation to 100% spawn a survivor.
- `nl4d.status` — Lists all rescued survivors, their status (Recruited / Town), HP, relation, and former Lord.
- `nl4d.reload_config` — Hot-reloads `config.json` from disk.
