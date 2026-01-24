## **Making Poffins**

### **How to Cook Poffins**

Poffins can be cooked alone or in groups of up to four players. The process involves stirring berry batter in a pot, with the goal of creating the strongest Poffin possible in the shortest time.

#### **Cooking Setup**

| Generation          | Players & Locations                                                                                | Berry Input                                 |
| :------------------ | :------------------------------------------------------------------------------------------------- | :------------------------------------------ |
| **Gen IV (DPPt)**   | • **Alone:** Poffin House<br>• **Local Wireless:** Poffin House<br>• **Wi-Fi:** Pokémon Wi-Fi Club | Each player adds one Berry.                 |
| **Gen VIII (BDSP)** | • **Alone:** Poffin House                                                                          | A single player can use up to four Berries. |

#### **The Cooking Process**

1.  **Stir the Batter:** Use the stylus (DS) or control sticks (Switch) to stir the batter in the pot.
2.  **Follow the Arrows:** An arrow will show which direction to stir (clockwise or counter-clockwise). Stirring in the correct direction lowers the cooking time.
3.  **Avoid Mistakes:**
    - **Burning:** Occurs if you stir too slowly.
    - **Spilling:** Occurs if you stir too quickly in the early stages.
4.  **Cooking Ends:** The process finishes after one minute, or sooner if you stir effectively.

---

#### **Cooking Stages**

The cooking process has three distinct stages as the batter thickens.

| Stage             | Flames | Batter Appearance | Stirring Notes                                    |
| :---------------- | :----- | :---------------- | :------------------------------------------------ |
| **1. Doughy**     | Orange | Pale and doughy   | Easy to spill, requires slow stirring.            |
| **2. Thickening** | Red    | Browning          | Harder to spill, requires faster stirring.        |
| **3. Final**      | Blue   | Dark brown        | Impossible to spill, requires very fast stirring. |

---

#### **Results**

Once finished, every player involved receives a number of identical Poffins equal to the total number of Berries used in the recipe.

#### **Two-Flavor Naming Priority**

If both flavors have equal strength, the name is prioritized in this order:

1. Spicy
2. Dry
3. Sweet
4. Bitter
5. Sour

### **Poffin Flavors & Strengths**

A Poffin's flavors are derived from the Berries used to make it. Each of the five flavors is weakened by one other flavor, which can reduce its final strength. The strength of each flavor in the final Poffin determines how much it will boost a Pokémon's contest conditions.

#### **Flavor Weaknesses**

The following table shows which flavor weakens another.

| Flavor     | Is Weakened By |
| :--------- | :------------- |
| **Spicy**  | Dry            |
| **Sour**   | Spicy          |
| **Bitter** | Sour           |
| **Sweet**  | Bitter         |
| **Dry**    | Sweet          |

### **Poffin Flavor Calculation**

This formula applies only when all Berries used in the recipe are unique.

#### **Step-by-Step Calculation**

1. **Sum Flavors:** Add the values for each of the five flavors from all Berries used. You will have five totals (one for Spicy, one for Dry, etc.).
2. **Apply Weaknesses:** For each flavor total, subtract the total of the flavor that weakens it.
   - **New Spicy** = Total Spicy - Total Dry
   - **New Dry** = Total Dry - Total Sweet
   - **New Sweet** = Total Sweet - Total Bitter
   - **New Bitter** = Total Bitter - Total Sour
   - **New Sour** = Total Sour - Total Spicy

3. **Negative Flavor Penalty:** For each flavor that is currently negative, subtract 1 from _all five_ flavors.
4. **Time & Error Modifier:** Apply the following formula to each of the five flavors:
   - `Final Flavor = round( (Flavor Value * (60 / Cook Time in Seconds)) - (Number of Burns + Number of Spills) )`
   - **Implementation note:** The core library truncates (integer division) instead of rounding for speed.

5. **Remove Negatives:** Set any flavor value that is now negative to 0.
6. **Apply Flavor Cap:** Any flavor value exceeding the maximum is reduced to the cap.
   - **Generation IV:** Max flavor is 99.
   - **Generation VIII:** Max flavor is 100.

---

#### **Exceptions**

- **Foul Poffin:** The recipe results in a **Foul Poffin** if two or more identical Berries are used, or if all flavors end up at 0. A Foul Poffin gives a +2 boost to three random conditions.
  - **Implementation note:** The core library uses a deterministic foul result (Spicy/Dry/Sweet set to 2, others 0).
  - **Implementation note:** Foul poffins are never included in feeding plans.
- **Cook Time Display:** The timer displays hundredths of a second, but the game only registers time in 1/30th of a second intervals. In Generation VIII, a time displayed as "0:47.100" is treated as 48 full seconds.

### **Optimization: Precomputed Combo Bases**

For high-volume search (millions of recipes), precompute unique 2–4 berry combinations into summed base values.
This avoids per-berry summation inside the hot cooking loop.

- Source: `PoffinComboTable.All`
- Each entry stores total weakened flavor sums, total smoothness, and berry count.
- Single-berry recipes are not included in this precompute path.

### **Subset Combo Enumeration (UI Scenarios)**

For UI workflows where users first filter berries and then cook only from that subset,
use the non-allocating enumerator:

- `PoffinComboEnumerator.ForEach(source, choose, action)`
- Supports 2–4 berry combinations in deterministic order (i &lt; j &lt; k &lt; l).
- Uses stackalloc buffers; the span is only valid for the duration of the callback.

### **Poffin Level**

A Poffin's level is simply the value of its single strongest flavor.

- **Example:** A Poffin with 12 Spiciness and 8 Dryness is a **Level 12** Poffin.
- **Improving Level:** The level is increased by cooking faster and avoiding burns or spills.
- **Maximum Level:**
  - **Generation IV:** 99
  - **Generation VIII:** 100

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
| **4. Time/Error Modifier**   | -12   | -12   | -2    | 8      | 8     |
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
| **Foul**          | N/A     | Black                                                              | Results from using ≥2 of the same berry or if all flavors are ≤ 0. Has 3 random flavors at strength 2. |
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

## **Usage Examples**

Run a fast search with min level and smoothness bounds:
```powershell
dotnet run --project BDSP.Cli -- --berries=4 --topk=50 --min-level=50 --max-smooth=20
```

Run a Spicy-focused search:
```powershell
dotnet run --project BDSP.Cli -- --min-spicy=30 --sort=level:desc --then=smoothness:asc
```

Run a pruning-heavy search (filters + smoothness cap):
```powershell
dotnet run --project BDSP.Cli -- --min-level=60 --min-spicy=25 --max-smooth=15
```

---

#### **Smoothness Range**

- **Highest Possible:** **59** (from a single Berry with 60 smoothness).
- **Lowest Possible:** **6** (in Gen IV) or **7** (in Gen VIII).

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

When a Pokémon eats a Poffin, its contest conditions increase based on the Poffin's flavors and the Pokémon's Nature.

#### **Calculating Condition Gains**

- **Base Gain:** A Pokémon's condition increases by an amount equal to the Poffin's corresponding flavor strength.
- **Flavor Preference Bonus:** This gain is modified if the Poffin's primary flavor matches the Pokémon's liked or disliked flavor.
  - **Liked Flavor:** The Pokémon "happily" eats it. All condition gains from that Poffin are multiplied by **1.1x** (rounded down).
  - **Disliked Flavor:** The Pokémon "disdainfully" eats it. All condition gains from that Poffin are multiplied by **0.9x** (rounded down).

---

#### **Pokémon Flavor Preferences by Nature**

A Pokémon's liked and disliked flavors are determined by its Nature. Natures in _italics_ have no flavor preference.

| ↓ Liked Flavor | **Spicy** | **Dry**  | **Sweet** | **Bitter** | **Sour**  |
| :------------- | :-------- | :------- | :-------- | :--------- | :-------- |
| **Spicy**      | _Bashful_ | Adamant  | Brave     | Naughty    | Lonely    |
| **Dry**        | Modest    | _Docile_ | Quiet     | Rash       | Mild      |
| **Sweet**      | Timid     | Jolly    | _Hardy_   | Naive      | Hasty     |
| **Bitter**     | Calm      | Careful  | Sassy     | _Quirky_   | Gentle    |
| **Sour**       | Bold      | Impish   | Relaxed   | Lax        | _Serious_ |

### **Sheen**

A Pokémon's sheen determines how "full" it is and limits the number of Poffins it can eat.

#### **Core Rules**

| Attribute               | Rule                                                                                           |
| :---------------------- | :--------------------------------------------------------------------------------------------- |
| **How it Increases**    | When a Pokémon eats a Poffin, the Poffin's `smoothness` value is added to the Pokémon's sheen. |
| **Initial Sheen**       | 0                                                                                              |
| **Maximum Sheen**       | **255**                                                                                        |
| **Effect of Max Sheen** | A Pokémon with 255 sheen can no longer eat any Poffins.                                        |
| **Permanence**          | Sheen is permanent and cannot be removed or reset.                                             |

---

#### **Exceeding the Limit**

If a Pokémon eats a Poffin that would cause its sheen to go _above_ 255, its sheen is simply set to 255. The Pokémon still receives the full condition boosts from that final Poffin.

#### **Example**

A Poffin made from a single Wiki Berry has **24 smoothness**.

- After eating **10** of these Poffins, a Pokémon's sheen will be **240**.
- After eating the **11th** Poffin, its sheen becomes **255** (not 264). It can now no longer eat any Poffins.

### **Checking Sheen**

A Pokémon's sheen value is not displayed as a number in-game. Instead, it is represented by a series of sparkles on the Pokémon's status screen.

#### **Where to Check Sheen**

| Game Series                    | Location                 |
| :----------------------------- | :----------------------- |
| Ruby, Sapphire, Emerald        | PokéNav                  |
| Diamond, Pearl, Platinum, BDSP | Pokémon's Summary Screen |

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

#### **Special Case: Pokémon Box**

In **Pokémon Box Ruby & Sapphire**, while the exact sheen is not viewable, the condition stars for a Pokémon will flash if its sheen is maxed out at 255.
