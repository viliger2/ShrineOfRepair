# Shrine of Repair
![alt text](https://raw.githubusercontent.com/viliger2/ShrineOfRepair/main/images/screenshot.jpg)

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

## TODO
* maybe make a version that accounts to which items you repair, so I don't have to use custom formulas and hack game design solutions

## Version history
* 1.1.0 Added blacklist, lunar (optional [Ephemeral Coins](https://thunderstore.io/package/VarnaScelestus/Ephemeral_Coins/)) and void coins (with [ReleasedFromTheVoid](https://thunderstore.io/package/Anreol/ReleasedFromTheVoid/)) support
* 1.0.1 Reupload because r2modman is a good program
* 1.0.0 Initial release
