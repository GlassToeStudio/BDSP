Run:

```powershell
dotnet run --project BDSP.Core.Benchmarks -c Release
```

The benchmark suite includes both single-threaded and parallel variants for combo-base and span-based cooking.

### **Unified Search API**

Use `PoffinSearch.Run(...)` for a consistent call shape. It automatically selects:
- precomputed combo tables when the berry filter is empty (all berries), or
- subset enumeration when the user filters berries.

Parallel threshold for subsets (when `useParallel = true`):
- Use parallel when nCk >= 500 (based on combo count).

This keeps UI code simple while still using the fastest path.

Poffin filters also support exact flavor requirements via `RequireMainFlavor` and
`RequireSecondaryFlavor` in `PoffinFilterOptions`.

### **Cooking Rules (Summary)**

1. Add together the respective flavors of all berries used.
2. For each flavor total, subtract the total of that flavor's weakening flavor (spicy <- dry, dry <- sweet, sweet <- bitter, bitter <- sour, sour <- spicy).
3. For each flavor that is negative, subtract 1 from all five flavors.
4. Multiply all flavors by `60 / cookTimeSeconds`, then subtract `burns + spills`.
   - Implementation note: the core library uses integer truncation.
5. Set any negative flavors to 0.
6. Cap flavor values to the generation limit (Gen IV = 99, Gen VIII = 100). The core library clamps to 100.

### **Implementation Layout**
- `BDSP.Core/Poffins/Cooking`: combo tables + cooker
- `BDSP.Core/Poffins/Filters`: poffin filters
- `BDSP.Core/Poffins/Search`: unified search API + TopK
- `BDSP.Core/Optimization/Core`: contest stats + feeding plan result models
- `BDSP.Core/Optimization/Filters`: pruning & dedup helpers
- `BDSP.Core/Optimization/Search`: feeding search + contest stats search
- `BDSP.Core/Optimization/Enumeration`: permutation enumerators

### **Subset Precompute (Advanced)**

`PoffinSearch` does not precompute subsets by default because we do not track reuse yet.
Use `PoffinComboBuilder.CreateFromSubset(...)` only if you explicitly reuse the same
subset in multiple runs or want to manage caching yourself.

`PoffinComboEnumerator` (BDSP.Core.Poffins.Enumeration) is a public, non-allocating helper for UI workflows.
`PoffinSearch` uses inlined loops instead for lower overhead in the hot path.

To measure your local crossover points, run:
```powershell
dotnet run --project BDSP.Core.Benchmarks -c Release -- --filter *SubsetCookingBenchmarks*
```
To measure dedup/prune impact, run:
```powershell
dotnet run --project BDSP.Core.Benchmarks -c Release -- --filter *DedupPruneBenchmarks*
```

### **Feeding Plan Optimization (Baseline)**

Before feeding search, prune dominated poffins:
- A candidate is removed if another poffin is better or equal in all flavors,
  with lower (or equal) smoothness and rarity cost, and at least one strict improvement.

Identical poffin stat sets are deduplicated before pruning; the lowest rarity-cost
recipe is kept and `DuplicateCount` tracks how many recipes produced the same poffin.

Baseline plan uses low-smoothness first, then lower rarity cost, then higher level.
The baseline plan stops when sheen reaches 255 or all 5 stats reach 255.
Results include:
- `NumPerfectValues` (0-5)
- `Rank` (1 = perfect stats + max sheen, 2 = perfect stats, 3 = otherwise)
- `UniqueBerries` (distinct berries across the plan)

For UI/workflows, prefer `OptimizationPipeline` to run the full chain from berries
to candidates and then into feeding or contest searches.

For exhaustive exploration, use ordered poffin permutations (no repetition) on a pruned candidate set.

Contest stats search uses inlined permutation loops for performance (see `ContestStatsSearch`).

### **Contest Scoring Mode**

Contest scoring defaults to a balance-aware mode that rewards the weakest stat
in addition to the total stat sum. This helps avoid spiky results with 0s in
some categories.

CLI flag: `--score balanced|sum` (default: `balanced`).
The CLI now defaults to `--candidates 5000` for broader candidate pools.
Use `--progress` to display an outer-loop progress indicator during contest searches.
When `--progress` is set, the CLI also prints stage messages and counts
(filtered berries, nCk combinations, candidate count, and estimated permutations).

### **CLI Filters & Scoring**

Berry filters (subset selection before cooking):
- `--berry-min-rarity`, `--berry-max-rarity`
- `--berry-min-smoothness`, `--berry-max-smoothness`
- `--berry-min-spicy` / `--berry-max-spicy` (and Dry/Sweet/Bitter/Sour)
- `--berry-main-flavor`, `--berry-secondary-flavor`
- `--berry-required-flavors`, `--berry-excluded-flavors` (comma list)
- `--berry-any-flavor-min`, `--berry-any-flavor-max`
- `--berry-weak-main-min`, `--berry-weak-main-max`, `--berry-weak-main-flavor`
- `--berry-id`, `--berry-exclude-id`

Poffin candidate filters:
- `--poffin-min-level`, `--poffin-max-level`
- `--poffin-min-second-level`, `--poffin-max-second-level`
- `--poffin-min-smoothness`, `--poffin-max-smoothness`
- `--poffin-min-spicy` / `--poffin-max-spicy` (and Dry/Sweet/Bitter/Sour)
- `--poffin-main-flavor`, `--poffin-secondary-flavor`
- `--poffin-exclude-main-flavor`, `--poffin-exclude-secondary-flavor`
- `--poffin-any-flavor-min`, `--poffin-any-flavor-max`
- `--poffin-name`, `--poffin-exclude-name`
- `--poffin-id`, `--poffin-exclude-id`
- `--poffin-min-rarity`, `--poffin-max-rarity`
- `--poffin-max-similar`

Scoring weights:
- `--poffin-level-weight`, `--poffin-total-flavor-weight`, `--poffin-smoothness-penalty`
- `--poffin-preferred-main-flavor`, `--poffin-preferred-main-bonus`
- `--stats-weight`, `--poffin-penalty`, `--sheen-penalty`, `--rarity-penalty`
- `--rarity-mode max|sum`, `--score balanced|sum`, `--min-stat-weight`

Contest result filters (CLI post-filter):
- `--min-rank`, `--max-rank`
- `--min-poffins`, `--max-poffins`
- `--min-rarity`, `--max-rarity`
- `--min-perfect`, `--max-perfect`
Sort output with `--stats-sort` (comma list, optional `:desc`), e.g. `rank,poffins,rarity`.

Note: `--max-poffins` now also caps feeding during contest search (variable-length feeding).
Contest results now include an extra count: additional poffins needed to reach sheen 255
after maxing all stats (within the cap).

### **Usage Examples**

Full-berry search (fastest path):
```csharp
var options = new PoffinSearchOptions(choose: 4, cookTimeSeconds: 40, useParallel: true);
var results = PoffinSearch.Run(default, options, topK: 200);
```

Subset-driven search (UI workflow):
```csharp
var berryFilter = new BerryFilterOptions(minRarity: 1, maxRarity: 5);
var options = new PoffinSearchOptions(choose: 2, cookTimeSeconds: 60, useParallel: true);
var results = PoffinSearch.Run(berryFilter, options, topK: 50);
```

Score tuning (favor smoothness):
```csharp
var score = new PoffinScoreOptions(levelWeight: 1000, totalFlavorWeight: 1, smoothnessPenalty: 10);
var options = new PoffinSearchOptions(choose: 3, cookTimeSeconds: 40, useParallel: true, scoreOptions: score);
var results = PoffinSearch.Run(default, options, topK: 100);
```

### **Poffin Level**

A Poffin's level is simply the value of its single strongest flavor.

- **Example:** A Poffin with 12 Spiciness and 8 Dryness is a **Level 12** Poffin.
- **Improving Level:** The level is increased by cooking faster and avoiding burns or spills.
- **Implementation note:** The core library derives level from the max flavor value and clamps flavors to 100.

### **Poffin Calculation Example**

This example shows how the flavors are calculated for a Poffin made with a **Bluk Berry**, a **Nanab Berry**, and a **Wepear Berry**.

---

#### **Scenario 1: 60-Second Cook Time**

Assume the Poffin was cooked in the maximum 60 seconds with no burns or spills.

##### **Initial Berry Flavors**

| Berry            | Spicy | Dry | Sweet | Bitter | Sour |
| :--------------- | :---- | :-- | :---- | :----- | :--- |
| **Bluk Berry**   | 0     | 10  | 10    | 0      | 0    |
| **Nanab Berry**  | 0     | 0   | 10    | 10     | 0    |
| **Wepear Berry** | 0     | 0   | 0     | 10     | 10   |

##### **Calculation Steps**

| Step                         | Spicy | Dry   | Sweet | Bitter | Sour  |
| :--------------------------- | :---- | :---- | :---- | :----- | :---- |
| **1. Sum Flavors**           | 0     | 10    | 20    | 20     | 10    |
| **2. Subtract Weaknesses**   | -10   | -10   | 0     | 10     | 10    |
| **3. Negative Penalty (-2)** | -12   | -12   | -2    | 8      | 8     |
| **4. Time & Error Modifier** | -12   | -12   | -2    | 8      | 8     |
| **5. Set Negatives to 0**    | 0     | 0     | 0     | 8      | 8     |
| **Final Flavors**            | **0** | **0** | **0** | **8**  | **8** |

**Result:** Each player receives three **Level 8 Bitter-Sour Poffins** (Smoothness 17).

---

#### **Scenario 2: 40-Second Cook Time**

Now, assume the same Poffin was cooked in just 40 seconds, again with no errors. The first three steps of the calculation are identical.

##### **Modified Calculation Steps**

| Step                         | Spicy | Dry   | Sweet | Bitter | Sour   |
| :--------------------------- | :---- | :---- | :---- | :----- | :----- |
| **3. Negative Penalty (-2)** | -12   | -12   | -2    | 8      | 8      |
| **4. Time Modifier (x1.5)**  | -18   | -18   | -3    | 12     | 12     |
| **5. Set Negatives to 0**    | 0     | 0     | 0     | 12     | 12     |
| **Final Flavors**            | **0** | **0** | **0** | **12** | **12** |

**Result:** By cooking faster, each player receives three **Level 12 Bitter-Sour Poffins** (Smoothness 17).

### **Base Flavor Levels (Single Berry Cooking)**

When cooking with only one Berry (as is always the case when cooking alone in Generation IV), the process can be simplified. Each Berry has a "base level" for its primary flavor.

The final level of the Poffin can be calculated with the following formula:

`Level = floor( (60 / Time in Seconds) * Base Level ) - (Number of Burns + Number of Spills)`

If the result is a negative number, the final level is 0.

---

#### **Example: Aguav Berry**

An Aguav Berry has a base Bitter level of **14**.

| Cook Time      | Errors | Calculation                 | Final Level |
| :------------- | :----- | :-------------------------- | :---------- |
| **60 seconds** | 0      | `floor( (60/60) * 14 ) - 0` | **14**      |
| **40 seconds** | 0      | `floor( (60/40) * 14 ) - 0` | **21**      |
| **40 seconds** | 2      | `floor( (60/40) * 14 ) - 2` | **19**      |

### **Poffin Types**

The type of Poffin is determined by the number and strength of its flavors.

| Poffin Type       | Flavors | Appearance                                                         | Notes                                                                                                  |
| ----------------- | ------- | ------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------ |
| **Foul**          | N/A     | Black                                                              | Results from using >=2 of the same berry or if all flavors are == 0. Has 3 random flavors at strength 2. |
| **Single-Flavor** | 1       | Solid color based on the flavor                                    | Named after the single flavor (e.g., Spicy Poffin).                                                    |
| **Two-Flavor**    | 2       | Primary flavor's color with sprinkles of the second flavor's color | Named after both flavors, with the stronger one listed first.                                          |
| **Rich**          | 3       | Medium gray                                                        |                                                                                                        |
| **Overripe**      | 4       | Light gray                                                         |                                                                                                        |
| **Mild**          | N/A     | Gold with yellow sprinkles                                         | Created when the Poffin's level is 50 or higher.                                                       |
| **Super Mild**    | N/A     | Rainbow-striped with yellow sprinkles                              | (Brilliant Diamond/Shining Pearl only) Level 95+, cooked with no overflows or burns.                   |

---

### **Poffin Smoothness**

The smoothness of a Poffin determines how many a Pokemon can eat before it gets full. It is calculated from the Berries used and any bonus reduction earned during cooking.

#### **Smoothness Formula**

`Smoothness = floor( Average Berry Smoothness ) - Number of Berries Used - Bonus Reduction`

---

#### **Bonus Smoothness Reduction**

A bonus can be earned to make the Poffin smoother (lower value), allowing a Pokemon to eat more.

| Generation      | Method                            | Details                                                                                     | Max Reduction |
| :-------------- | :-------------------------------- | :------------------------------------------------------------------------------------------ | :------------ |
| **IV (DPPt)**   | Multiplayer synchronized stirring | Players stir together in sync, creating sparkles. More sparkles lead to a larger reduction. | **10 points** |
| **VIII (BDSP)** | Cooking in Amity Square           | Based on the number and friendship level of your party Pokemon that can enter the park.     | **9 points**  |

**Implementation note:** The core library currently models the BDSP cap (max bonus 9). Gen IV's max bonus (10) is not modeled in code.

## **CLI Examples**

Contest search with berry + poffin filters:
```powershell
dotnet run --project BDSP.Core.CLI -- contest-search --choose 4 --time 40 --topk 50 --candidates 5000 --parallel ^
  --berry-min-rarity 3 --berry-max-rarity 11 ^
  --poffin-min-level 90 --poffin-min-num-flavors 1 ^
  --score balanced --progress
```

Contest search with strict rank/poffin limits:
```powershell
dotnet run --project BDSP.Core.CLI -- contest-search --choose 3 --time 40 --topk 50 --candidates 5000 ^
  --max-rank 2 --max-poffins 12
```

---

#### **Smoothness Range**

- **Lowest Possible (core library):** **0** (smoothness is clamped to 0 after reductions).
- **Highest Possible:** Depends on recipe, cook time, and bonus reduction; see the formula above.

---

#### **Berry Smoothness Values**

The following table lists the base smoothness for each Berry type.

| Smoothness | Berries                                                                                                     |
| :--------- | :---------------------------------------------------------------------------------------------------------- |
| **20**     | Leppa, Oran, Persim, Lum, Sitrus, Razz, Bluk, Nanab, Wepear, Pinap, Pomeg, Kelpsy, Qualot, Hondew, Grepa    |
| **25**     | Cheri, Chesto, Pecha, Rawst, Aspear, Figy, Wiki, Mago, Aguav, Iapapa                                        |
| **30**     | Tamato, Cornn, Magost, Rabuta, Nomel, Occa, Passho, Wacan, Rindo, Yache, Chople, Kebia, Shuca, Coba, Payapa |
| **35**     | Spelon, Pamtre, Watmel, Durin, Belue, Tanga, Charti, Kasib, Haban, Colbur, Babiri, Chilan, Roseli (BDSP)    |
| **40**     | Liechi, Ganlon, Salac, Petaya, Apicot                                                                       |
| **50**     | Lansat, Starf                                                                                               |
| **60**     | Enigma, Micle, Custap, Jaboca, Rowap                                                                        |

### **Feeding Poffins & Condition Boosts**

When a Pokemon eats a Poffin, its contest conditions increase based on the Poffin's flavors and the Pokemon's Nature.

#### **Flavor → Condition Mapping**

| Condition  | Flavor | Color  |
| ---------- | ------ | ------ |
| Coolness   | Spicy  | Red    |
| Beauty     | Dry    | Blue   |
| Cuteness   | Sweet  | Pink   |
| Cleverness | Bitter | Green  |
| Toughness  | Sour   | Yellow |

#### **Calculating Condition Gains**

- **Base Gain:** A Pokemon's condition increases by an amount equal to the Poffin's corresponding flavor strength.
- **Flavor Preference Bonus:** This gain is modified if the Poffin's primary flavor matches the Pokemon's liked or disliked flavor.
  - **Liked Flavor:** The Pokemon "happily" eats it. All condition gains from that Poffin are multiplied by **1.1x** (rounded down).
  - **Disliked Flavor:** The Pokemon "disdainfully" eats it. All condition gains from that Poffin are multiplied by **0.9x** (rounded down).

---

#### **Pokemon Flavor Preferences by Nature**

A Pokemon's liked and disliked flavors are determined by its Nature. Natures in _italics_ have no flavor preference.
**Implementation note:** The core library does not apply nature modifiers yet.

| v Liked Flavor | **Spicy** | **Dry**  | **Sweet** | **Bitter** | **Sour**  |
| :------------- | :-------- | :------- | :-------- | :--------- | :-------- |
| **Spicy**      | _Bashful_ | Adamant  | Brave     | Naughty    | Lonely    |
| **Dry**        | Modest    | _Docile_ | Quiet     | Rash       | Mild      |
| **Sweet**      | Timid     | Jolly    | _Hardy_   | Naive      | Hasty     |
| **Bitter**     | Calm      | Careful  | Sassy     | _Quirky_   | Gentle    |
| **Sour**       | Bold      | Impish   | Relaxed   | Lax        | _Serious_ |

### **Sheen**

A Pokemon's sheen determines how "full" it is and limits the number of Poffins it can eat.

#### **Core Rules**

| Attribute               | Rule                                                                                           |
| :---------------------- | :--------------------------------------------------------------------------------------------- |
| **How it Increases**    | When a Pokemon eats a Poffin, the Poffin's `smoothness` value is added to the Pokemon's sheen. |
| **Initial Sheen**       | 0                                                                                              |
| **Maximum Sheen**       | **255**                                                                                        |
| **Effect of Max Sheen** | A Pokemon with 255 sheen can no longer eat any Poffins.                                        |
| **Permanence**          | Sheen is permanent and cannot be removed or reset.                                             |

---

#### **Exceeding the Limit**

If a Pokemon eats a Poffin that would cause its sheen to go _above_ 255, its sheen is simply set to 255. The Pokemon still receives the full condition boosts from that final Poffin.

#### **Example**

A Poffin made from a single Wiki Berry has **24 smoothness**.

- After eating **10** of these Poffins, a Pokemon's sheen will be **240**.
- After eating the **11th** Poffin, its sheen becomes **255** (not 264). It can now no longer eat any Poffins.

### **Checking Sheen**

A Pokemon's sheen value is not displayed as a number in-game. Instead, it is represented by a series of sparkles on the Pokemon's status screen.

#### **Where to Check Sheen**

| Game Series                    | Location                 |
| :----------------------------- | :----------------------- |
| Ruby, Sapphire, Emerald        | PokeNav                  |
| Diamond, Pearl, Platinum, BDSP | Pokemon's Summary Screen |

---

#### **Sheen-to-Sparkle Conversion**

The number of sparkles shown corresponds to a specific range of sheen values, which differs by game generation. A fully maxed-out sheen has a special indicator.

##### **Generation III**

| Sparkles          | Sheen Range |
| :---------------- | :---------- |
| 1                 | 0 - 28      |
| 2                 | 29 - 57     |
| 3                 | 58 - 86     |
| 4                 | 87 - 115    |
| 5                 | 116 - 144   |
| 6                 | 145 - 173   |
| 7                 | 174 - 202   |
| 8                 | 203 - 231   |
| 9                 | 232 - 254   |
| **10 (Flashing)** | **255**     |

##### **Generation IV & VIII**

| Sparkles          | Sheen Range |
| :---------------- | :---------- |
| 0                 | 0 - 21      |
| 1                 | 22 - 42     |
| 2                 | 43 - 63     |
| 3                 | 64 - 85     |
| 4                 | 86 - 106    |
| 5                 | 107 - 127   |
| 6                 | 128 - 149   |
| 7                 | 150 - 170   |
| 8                 | 171 - 191   |
| 9                 | 192 - 213   |
| 10                | 214 - 234   |
| 11                | 235 - 254   |
| **12 (Flashing)** | **255**     |

---

#### **Special Case: Pokemon Box**

In **Pokemon Box Ruby & Sapphire**, while the exact sheen is not viewable, the condition stars for a Pokemon will flash if its sheen is maxed out at 255.



