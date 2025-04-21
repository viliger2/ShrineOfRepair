<details>
<summary>2.0.1 </summary>

* Added void coin support to Picker variant.
  * _This also removes "Use Lunar Coins by Default" option and replaces it with "Currency Type" option that allows you to set gold, void or lunar coins as general currency for the shrine. Bazaar and moon shrines are not affected by this and are still controlled by their respective config options._
</details>
<details>
<summary>2.0.0 </summary>

* Actual SoTS update, added support for all breakable items (Sale Star, Unstable Transmitter and Seed of Life).
* Mod is mostly rewritten with less jank, config and API should still be the same.
* Added option to repair void items.
  * _Repair list for void items is dynamically generated from ContagiousItemManager, so any modded item should automatically be added to the list without any input from the user. However, if void item has multiple "source" items, such as Singularity Band or Newly Hatched Zoea then it won't be added to the list due to inability to find the source (well, actually it is mostly implementation limitation, I would be fine with Zoea showing you every possible boss item in the game)_. 
</details>
<details>
<summary>1.4.4 </summary>

* Added Longstanding Solitude support.
</details>
<details>
<summary>1.4.3 </summary>

* Added option for shrine to use gold in bazaar.
</details>
<details>
<summary>1.4.2 </summary>

* SoTS update.
</details>
<details>
<summary>1.4.1 </summary>

* Restored original methods and marked them as depricated. Sorry about that.
</details>
<details>
<summary>1.4.0 </summary>

* Reworked list extension for mod developers, so if you decide to implement it, your mod won't throw errrors if ShrineOfRepair is not present. It does require a slight rewrite, example can be seen [here](https://github.com/viliger2/ExtradimensionaItems/blob/master/RoR2_ItemsMod/Modules/ShrineOfRepairCompat.cs).
</details>
<details>
<summary>1.3.0 </summary>

* Moved to split R2API assemblies
* Implemented optional RiskOfOptions support. Not everything is in there, since some things are initialized on game's start, like director costs.
</details>
<details>
<summary>1.2.3 </summary>

* Moved shrine in Bazaar so it doesn't collide with things spawned by BiggerBazaar
* Moved coordinates and angles of all static spawns (bazaar, moon and moon2) into config
</details>
<details>
<summary>1.2.2 </summary>

* Added Max Uses Config.
* Added Config to spawn the shrine in Bazaar Between Time and Commencement.
* Implemented Lunar Coins for Scrapper UI.
* Added Korean Support.
</details>
<details>
<summary>1.2.1 </summary>

* Added Repair List (replaces Blacklist), Equipment Repair, Boss/Lunar/Equipment Cost Config and Void Lunar compat (from [BubbetsItems](https://thunderstore.io/package/Bubbet/BubbetsItems/)). 
* Added Regenerating Scrap and Trophy Hunter's Tricorn by default.
* Fixed non-english clients having broken text strings.
* Implemented basic repair list extension support (via methods for mod developers and via Repair List for users)
</details>
<details>
<summary>1.2.0 </summary>

* Added scrapper-like version (comes with config wipe, sorry).
</details>
<details>
<summary>1.1.1 </summary>

* Added shader to billboard icon, added sandy and snowy variants.
</details>
<details>
<summary>1.1.0 </summary>

* Added blacklist, lunar (optional [Ephemeral Coins](https://thunderstore.io/package/VarnaScelestus/Ephemeral_Coins/)) and void coins (with [ReleasedFromTheVoid](https://thunderstore.io/package/Anreol/ReleasedFromTheVoid/)) support
</details>
<details>
<summary>1.0.1 </summary>

* Reupload because r2modman is a good program
</details>
<details>
<summary>1.0.0 </summary>

* Initial release
</details>