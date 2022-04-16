# Shrine of Repair
![alt text](https://raw.githubusercontent.com/viliger2/ShrineOfRepair/main/images/screenshot.jpg)

Risk of Rain 2 mod, adds a shrine to repair broken items. Features custom price scaling, to account for number of broken items you **might** have in your inventory, so the shrine is cheaper than a single chest early on and gets sharper price increase in the late game compared to the rest of interactables. Everything is customizable (base price, director costs and weights, which formula to use, etc.)
Custom scaling formula is
![diffCoef^customsScalingModifier * BaseCost](https://raw.githubusercontent.com/viliger2/ShrineOfRepair/main/images/formula.png)  
where
* diffCoef - difficulty coefficient, scales with time
* customScalingModifier - custom modifier, used to make chest cheaper at the start and more expensive in late game
* BaseCost - base cost of the shrine

Non-custom formula is just game's default scaling formula with addition of its own coefficient.

## Credits
programming, billboard icon, bad model - me  
good model, texturing - Extrabee

## TODO
* add snowy and sandy variant if someone explains how hoopo does it
* make a good billboard icon, preferably with shader similar to other in-game shrines
* maybe make a version that accounts to which items you repair, so I don't have to use custom formulas and hack game design solutions

## Version history
* 1.0.0 Initial release
