This is a project I made in python.

Its purpose is to find the best berry combination to cook a Poffin that gives the highest max stats with the least effort. There is the Berry class, which is the ingredient to a Poffin. The Poffin class, which is made of 1 or more berries, and the Poffin cooker, which cooks a Poffin based on some rules. There is then a contest stats class that ranks each poffin. Every main class, berry, poffin, stats has a similar filtering system to reduce the total number of poffins tested. I am converting this to c# 9. I want it to be as fast as possible, working with millions of permutations of berries and combinations, and reduce memory footprint. I want it to be a core class library that will integrate, later, with a UI.

Here is some text about the rules of poffin making and how they affect a pokemon

Flavors
The Poffin's flavors depend on the flavors of the Berries used to produce it. Each flavor is weakened by another flavor, so the presence of one flavor will decrease the strength of another. The strength of each flavor determines how much a Poffin will affect each of a Pokémon's conditions.

Flavor Spicy Sour Bitter Sweet Dry
Weakened by... Dry Spicy Sour Bitter Sweet
If each Berry used in the Poffin is different from the others, the flavors in the resulting Poffin are calculated as follows:

Add together the respective flavors of all Berries being used (sum all spicy values, all dry values, and so on).
For each Berry flavor total, subtract the Berry flavor total of that flavor's weakening flavor as per the table above (e.g. spicy is weakened by dry, sour is weakened by spicy, etc.).
Subtract 1 from each flavor for every flavor that is negative.
Multiply all flavors by 60 divided by the number of seconds taken to cook, then subtract the number of times the mixture was burned or spilled. Round to the nearest integer.
Set any flavors that are now negative to 0.
If any flavors are greater 100, set those flavors to that limit.
If two or more copies of the same Berry are used, or all flavors are 0 (due to a large number of burns or spills), the Poffin will instead become a Foul Poffin, which boosts three random conditions by 2.

Poffin level
The Poffin's level is equal to the strength of its strongest flavor. The level can be improved by making the Poffin in a shorter time, and by minimizing the number of burns and spills, as detailed in the above procedure. The highest possible level for a Poffin is thus 100.

Base flavor levels
When cooking with one Berry (which is always the case when cooking alone in Generation IV), each Berry can be viewed as having a base strength for each Poffin flavor, which is then boosted by cooking faster than the maximum and decreased by spilling or burning the mixture. Since flavors that are already negative can only become more negative, they can be treated as 0, even though this is not performed until later in the algorithm.

Poffin levels can be improved by making the Poffin in a shorter time. The level of the cooked Poffin is equal to
round⁡(base⋅60time−errors),

⁡(If this level would be less than 0, it becomes 0 instead.)

For example, when with a single Berry, an Aguav Berry has a base bitterness of 14 (other flavors have a base strength of 0). This means completing in 60 seconds with 0 burns and spills will yield a level 14 Poffin:

Base flavor levels

Poffin types
Poffin types depend on the number and strength of flavors present.

If a Poffin is created using two or more of the same Berry, or if all of its flavors end up being 0 or less, it becomes a Foul Poffin, which is black in color. A Foul Poffin has three random flavors with 2 strength, ignoring the flavors of the constituent Berries.

If the Poffin is Level 50 or higher, it is a Mild Poffin, which is gold with lighter yellow sprinkles on top. In Brilliant Diamond and Shining Pearl, if a Poffin is Level 95 or higher and was cooked without any overflows or burns, it is a Super Mild Poffin, which has a rainbow-striped coloration with the same yellow sprinkles as the Mild Poffin.

Otherwise, if there is only one flavor present, the Poffin will be named after that flavor. If there are two flavors, the Poffin will be named after both flavors, with the stronger flavor first; if both flavors have the same strength, they are prioritized in the following order: Spicy, Dry, Sweet, Bitter, Sour. These Poffins are colored based on their primary flavor, with sprinkles of the second flavor's color for two-flavored Poffins.

If there are three flavors, the Poffin is a Rich Poffin, which is a medium gray color. If there are four flavors, the Poffin is an Overripe Poffin, which is a light gray color.

Smoothness
This section is incomplete.
Please feel free to edit this section to add missing information and complete it.
Reason: Determine how different levels of synchronization/friendship affect the smoothness reduction
The smoothness of a Poffin is equal to the average smoothness of the Berries used (rounded down), minus the number of Berries used, minus any applicable bonus reduction. This can be expressed as the following formula (where n is the number of Berries used):

In Generation IV, if two or more players are cooking Poffins together and their stirring motions are all synchronized, a sparkling trail will appear on each game's touch screen.[1] The Poffin's smoothness will then be reduced by between 1 and 10 points, depending on how much of the stirring was synchronized enough to earn sparkles. The stirring itself is also more effective this way, allowing for lower cook times and thus higher-level Poffins that the Pokémon can eat more of before they get full.

In Generation VIII, the bonus reduction to smoothness is instead earned by cooking at Amity Square, and is based on how many of the player's party Pokémon are eligible for the park and have been herded up prior to cooking, as well as the friendship levels of those Pokémon. With a full party of Pokémon at maximum friendship which are all eligible for Amity Square, the largest possible smoothness reduction in this generation is 9 points.

The highest legally obtainable smoothness of a Poffin is 59, by using a single Berry with 60 smoothness. The lowest obtainable smoothness is 6 in Generation IV, or 7 in Generation VIII, by using four berries that each have 20 smoothness and getting the maximum available bonus reduction.

The Berries of each smoothness are listed below.

Feeding Poffins
Condition
When a Pokémon is fed a Poffin, its Contest conditions are normally increased by an amount equal to the strength of the corresponding flavor. If the primary flavor of a Poffin matches the favorite flavor of the Pokémon it is fed to, the Pokémon will "happily" eat the Poffin, and the value of all of that Poffin's condition increases, not just the one associated with the chosen flavor, will be multiplied by 1.1 (rounded down). If the primary flavor matches the Pokémon's disliked flavor instead, the Pokémon will "disdainfully" eat the Poffin, and all condition increases will be multiplied by 0.9 (rounded down).

A Pokémon's liked and disliked flavor are determined by its nature. Some Pokémon neither like nor dislike any flavors.

No preferences ↘ Disliked flavor
Spicy Dry Sweet Bitter Sour
Liked flavor Spicy Bashful Adamant Brave Naughty Lonely
Dry Modest Docile Quiet Rash Mild
Sweet Timid Jolly Hardy Naive Hasty
Bitter Calm Careful Sassy Quirky Gentle
Sour Bold Impish Relaxed Lax Serious
Sheen
Main article: Sheen
Whenever a Pokémon eats a Poffin, that Poffin's smoothness is added to the Pokémon's sheen, up to a maximum of 255 sheen. Pokémon initially have 0 sheen. If a Pokémon has 255 sheen, it can no longer eat any Poffins. Sheen is permanent and cannot be reset.

If a Pokémon eats a Poffin that would cause its sheen to exceed 255, instead its sheen becomes 255 (but the Pokémon's condition stats are still increased by the Poffin's full amount as normal).

```
Span<BerryId> poolBuf = stackalloc BerryId[BerryTable.Count];

var filter = BerryFilters.Tight(maxSmoothness: 25, maxRarity: 3, minMainFlavorValue: 10);

int count = BerryQuery.Filter(in filter, poolBuf);
var berryPool = poolBuf[..count]; // slice is allocation-free
```
