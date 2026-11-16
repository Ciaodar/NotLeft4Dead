[center][size=7][b]Not Left 4 Dead (NL4D)[/b][/size]
[size=5][b][color=#FFD700]⚔️ BANNERLORD v1.5.7+ BATTLE WRECKAGE SURVIVORS ⚔️[/color][/b][/size]
[b][color=#00ff00]STATUS: OFFICIAL RELEASE (v1.0.0.0) | NO HARMONY PATCHES[/color][/b][/center]

[center][size=5][b]Important: NL4D supports both Steam and PC Game Pass (Xbox Store) versions.[/b][/size]
[size=5][b]Full Turkish (TR) and English (EN) localization is supported.[/b][/size][/center]

[line]

[size=6][b]What Does This Mod Do?[/b][/size]

[b]Not Left 4 Dead (NL4D)[/b] expands Mount & Blade II: Bannerlord's [b]v1.5.7 Battle Wreckage[/b] mechanic with immersive, lore-driven companion rescues, built with [b]no Harmony patches[/b].

When you finish scavenging a land or naval battle wreckage, a wounded survivor clinging to life amidst the carnage may crawl out of the debris. Question them about what happened, see the exact Lords and Factions involved as clickable Encyclopedia links, and decide whether to recruit them on the spot or send them to recover at the nearest tavern, where they will remember your mercy and join you for [b]free[/b] whenever you cross paths again.

[line]

[size=6][b]Core Features[/b][/size]

[list]
[*][b]🛡️ No-Harmony-Patch Integration:[/b] Built purely on official TaleWorlds v1.5.7 events with no Harmony patches. Triggers seamlessly right when the 9-hour wreckage investigation progress bar completes, directly over the decision/results menu.
[*][b]📜 Real Battle Context & Clickable Encyclopedia Links:[/b] Extracts the actual commanding lord, opposing commander, kingdoms/clans, total casualties, and elapsed hours from the [font=Courier New]BattleWreckage[/font] site. Every Lord, Faction, and Settlement mentioned in dialogue is a clickable Encyclopedia link.
[*][b]🗣️ Interactive Lore Dialogue Hub:[/b] Before deciding their fate, ask the survivor in any order:
  - [i]"What actually happened here? Tell me of the battle."[/i]
  - [i]"Who are you really? What is your story?"[/i]
  - [i]"What happened to you? How bad are your wounds?"[/i]
[*][b]🎭 70+ Dynamic Dialogue Variations & Emotional States:[/b] Survivors react with distinct psychological states including near-death angel hallucinations, bitter fury, broken morale, dark humor, survivor guilt, and shell-shock. Every inquiry reveals unique reactions based on battle outcome, casualties, and trauma.
[*][b]📜 12 Unique Character Background Stories:[/b] Every survivor carries a distinct past (blacksmith apprentice, border scout, caravan guard, pit-fighter, disgraced noble, siege sapper, battlefield surgeon's helper, reformed highwayman, standard-bearer, and more).
[*][b]📖 Custom Encyclopedia Biography & Met Status:[/b] Rescued survivors are automatically marked as met ([i]HasMet[/i]) and receive a custom Encyclopedia biography recording who abandoned them, who they fought, and that you pulled them from the wreckage.
[*][b]⚔️ Winner or Defeated Side, Land or Naval:[/b] Survivors can be loyal retainers abandoned in a rout [i]or[/i] wounded veterans forgotten by a victorious army rushing after loot, across both land and naval wreckages.
[*][b]📊 Casualty-Scaled Spawn Chance:[/b] Requires at least one real NPC Lord on either side of the battle (player battles excluded). Spawn probability scales with battle casualties, reaching [b]100% at 1,500+ casualties[/b] (configurable).
[*][b]🩸 Wounded Start & +100 Relation:[/b] Survivors start heavily wounded ([b]15% to 20% HP[/b]) and immediately gain [b]+100 personal relation[/b] with you whether recruited immediately or sent to town.
[*][b]🍺 Tavern Reunion & Free Recruitment:[/b] Unrecruited survivors travel to the nearest town tavern and roam Calradia like normal wanderers (protected from Native's random wanderer purge). Meet them later in any tavern for a custom reunion dialogue and recruit them for [b]0 gold[/b].
[*][b]🧹 World FIFO Cap & Custom Name Pools:[/b] Unrecruited survivors in the world are capped at [b]5[/b] by default (oldest is retired when exceeded) to prevent save bloat. Uses native culture-matched Wanderer templates for varied faces, hair, beards, and gear, paired with gender-aware custom culture name & epithet pools.
[*][b]🌐 Cross-Platform & Localization:[/b] Full Steam and PC Game Pass folder support, with complete [b]English (EN)[/b] and [b]Turkish (TR)[/b] localization.
[/list]

[line]

[size=6][b]Configuration Modes[/b][/size]

[list]
[*][b]JSON Mode (Default):[/b] Works with no extra dependencies. Settings and custom culture name/epithet pools are read from [font=Courier New]Modules\NL4D\config.json[/font] (fallback: [font=Courier New]Documents\Mount and Blade II Bannerlord\Configs\NL4D\config.json[/font]). Hot-reload anytime via [font=Courier New]nl4d.reload_config[/font].
[*][b]MCM Mode (Optional):[/b] If you have [b]MCM v5[/b] installed, full settings sliders and toggles (including [b]Allow Exceeding Companion Limit[/b]) appear automatically with instant live hot-reload. No separate downloads required.
[/list]

[line]

[size=6][b]Installation[/b][/size]

[list=1]
[*]Download the latest [font=Courier New]NL4D[/font] package and extract it to your Bannerlord [font=Courier New]Modules[/font] folder.
[*]Enable [font=Courier New]Not Left 4 Dead[/font] in the launcher.
[*][b]Game Pass Users:[/b] If the mod doesn't load, go to [font=Courier New]Modules/NL4D/bin/Gaming.Desktop.x64_Shipping_Client[/font], right-click [font=Courier New]NL4D.dll[/font] -> Properties -> check [b]Unblock[/b].
[*][b]Load Order:[/b] Place [font=Courier New]Not Left 4 Dead[/font] after official modules (Native, SandBoxCore, Sandbox, StoryMode) and after MCM v5 if installed.
[/list]

[line]

[size=6][b]FAQ[/b][/size]

[b]Q: Does this require Harmony, ButterLib, or UIExtenderEx?[/b]
A: No! NL4D uses [b]no Harmony patches[/b] and has no hard dependencies outside the official game modules.

[b]Q: Do I need MCM?[/b]
A: No, it is completely optional. The mod works out of the box with default settings or manual [font=Courier New]config.json[/font] edits.

[b]Q: Is it safe to add mid-save?[/b]
A: Yes, adding NL4D to an existing Bannerlord v1.5.7+ campaign is safe. Removal mid-save is not recommended for any mod that creates heroes/save data; always keep a backup.

[line]

[size=6][b]Permissions and Credits[/b][/size]

[b]License:[/b]
[b]Creative Commons Attribution-NonCommercial-ShareAlike 4.0 (CC BY-NC-SA 4.0)[/b].
- Free to share/adapt with credit.
- Non-commercial use only.

[b]Credits To:[/b]
[list]
[*][b]TaleWorlds Entertainment[/b] for Mount & Blade II: Bannerlord and the v1.5.7 Battle Wreckage mechanic.
[*]The developers of [b]Mod Configuration Menu (MCM v5)[/b] for optional settings integration.
[*]Ciaodar - Lead Developer
[/list]
