# Shrine of Repair
![Shrine Screenshot](https://raw.githubusercontent.com/viliger2/ShrineOfRepair/main/images/screenshot.jpg)

## Current version
Risk of Rain 2 mod, adds a shrine to repair broken items. Features scrapper-like UI for selecting which item to repair, showing price per stack. 
![GUI Screenshot](https://raw.githubusercontent.com/viliger2/ShrineOfRepair/main/images/scrapper_ui.jpg)
Price scales with time, starting price equals to half of the equal chest price. Starting price is customizable. Config files are not separated into 3 categories:
* General behavior where you setup model, interactable type and director rules
* AllInOne where you change pre-1.2.0 variant settings, like price and scaling
* PerItem where you chance base prices of each item tier.

Pre-1.2.0 behavior can be enabled in General config.

## Pre 1.2.0 version
Risk of Rain 2 mod, adds a shrine to repair broken items. Features custom price scaling, to account for number of broken items you **might** have in your inventory, so the shrine is cheaper than a single chest early on and gets sharper price increase in the late game compared to the rest of interactable. Everything is customizable (base price, director costs and weights, which formula to use, etc.)
Custom scaling formula is
![diffCoef^customsScalingModifier * BaseCost](https://raw.githubusercontent.com/viliger2/ShrineOfRepair/main/images/formula.png)  
where
* diffCoef - difficulty coefficient, scales with time
* customScalingModifier - custom modifier, used to make chest cheaper at the start and more expensive in late game
* BaseCost - base cost of the shrine

Non-custom formula is just game's default scaling formula with addition of its own coefficient.

Config also includes option to blacklist items via IDs and usage of different currencies. Currencies can be:
* Gold, enabled by default, scaling and formulas are applied only to gold, everything else does not scale with time or difficulty.
* Lunar coins. If  [Ephemeral Coins](https://thunderstore.io/package/VarnaScelestus/Ephemeral_Coins/) is installed and used, either by artifact or global option, lunar coins will automatically become ephemeral coins.
* Void coins. While can be enabled without [ReleasedFromTheVoid](https://thunderstore.io/package/Anreol/ReleasedFromTheVoid/) there is simply no way to earn them, so only to be used with that mod.

## Credits
programming, billboard icon, bad model - me  
good model, texturing - Extrabee  
help with testing and RoR2 disassembling - vox

## Version history
* 1.3.0 Added Custom Repair Config, Equipment Repair, Boss/Lunar/Equipment Cost Config and Void Lunar compat. It also added Regenerating Scrap and Trophy Hunter's Tricorn by default in addition to the other 4.
* 1.2.0 Added scrapper-like version (comes with config wipe, sorry).
* 1.1.1 Added shader to billboard icon, added sandy and snowy variants.
* 1.1.0 Added blacklist, lunar (optional [Ephemeral Coins](https://thunderstore.io/package/VarnaScelestus/Ephemeral_Coins/)) and void coins (with [ReleasedFromTheVoid](https://thunderstore.io/package/Anreol/ReleasedFromTheVoid/)) support
* 1.0.1 Reupload because r2modman is a good program
* 1.0.0 Initial release
