Found several build-breaking issues and a correctness bug in the core search logic. Here are the findings, ordered by severity.

**Findings**

- Build break: **ContestStats** uses field names (**Cool**, **Cute**, **Smart**, **Tough**) and a constructor that don’t exist, so both dominance checks and **+** operator won’t compile. **ContestStats.cs (line 61)**, **ContestStats.cs (line 75)**
- Build break: **ContestStatsCalculator** calls a missing **ContestStats(...)** constructor. **ContestStatsCalculator.cs (line 20)**
- Build break: **FeedingOptions** defines **Score** twice and references **ContestStats** without a namespace/import, so the file won’t compile as-is. **FeedingOptions.cs (line 22)**, **FeedingOptions.cs (line 25)**
- Build break: **FeedingOptimizer** references an undefined **bestNodes** list (and never uses **bestStates**), so the optimizer won’t compile. **FeedingOptimizer.cs (line 16)**, **FeedingOptimizer.cs (line 51)**
- Build break: **BerryTable** has two file-scoped namespace declarations and **using** directives after a namespace declaration, which is invalid C#. **BerryTable.cs (line 3)**, **BerryTable.cs (line 8)**
- Correctness: **PoffinSearchRunner** partitions the berry pool and only enumerates combinations within each partition, which drops all combinations that mix berries across partitions; results will be incomplete. **PoffinSearchRunner.cs (line 69)**, **PoffinSearchRunner.cs (line 84)**
- Runtime risk: **BuildBestPlan** assumes **nodes** has at least one entry; if candidates are empty or all pruned, it will throw. **FeedingOptimizer.cs (line 95)**
- Build/config gap: **BDSP.Core/Validation/CoreGuards** has no **.cs** extension, so the guard code is never compiled or used.

**Questions / Assumptions**

- Should **ContestStats** expose **Cool/Beauty/Cute/Smart/Tough** (as used by **FeedingOptions**) or **Coolness/Beauty/Cuteness/Cleverness/Toughness** (as currently defined)? The API looks split between two naming schemes.
- For parallel search, is the goal to partition the _combination space_ (e.g., by fixing the first berry index per worker) rather than partitioning the pool itself?

If you want, I can propose a concrete fix set for the compile errors and the search partitioning logic.

**Possible Next Steps**

1. I can draft a minimal patch to make **BDSP.Core** compile again (fix **ContestStats**, **FeedingOptions**, **FeedingOptimizer**, **BerryTable**).
2. I can redesign **PoffinSearchRunner** to parallelize over the full combination space without dropping mixes.
3. I can suggest a small test suite that locks in correct optimizer/search behavior.

Focused on BDSP.Core and cross-checked with NOTES.md. Most of the prior issues are still present, plus a couple of new ones.

Findings

Critical: ContestStats uses non-existent fields (Cool/Cute/Smart/Tough) and lacks the ctor used by the calculator, so the dominance check and + operator won’t compile; ContestStatsCalculator also won’t compile due to the missing ctor. ContestStats.cs (line 59), ContestStats.cs (line 76), ContestStatsCalculator.cs (line 20)
Critical: FeedingOptions defines Score twice and references ContestStats without a namespace/import, so it won’t compile. FeedingOptions.cs (line 22), FeedingOptions.cs (line 25)
Critical: FeedingOptimizer uses an undefined bestNodes list and never uses bestStates, so it won’t compile. FeedingOptimizer.cs (line 16), FeedingOptimizer.cs (line 51), FeedingOptimizer.cs (line 60)
Critical: BerryTable has using directives after a file-scoped namespace and duplicate namespace declarations, which is invalid C#. BerryTable.cs (line 1), BerryTable.cs (line 3)
High: PoffinSearchRunner partitions the berry pool and only enumerates combinations within each partition, dropping all mixed-partition combos; results are incomplete. PoffinSearchRunner.cs (line 69), PoffinSearchRunner.cs (line 84)
High: PoffinCooker divides by cookTimeSeconds without validation; a zero value will throw at runtime. PoffinCooker.cs (line 100)
Medium: BuildBestPlan assumes nodes[0] exists; if all candidates are pruned or candidates is empty, it throws. FeedingOptimizer.cs (line 90)
Medium: BDSP.Core/Validation/CoreGuards has no .cs extension, so the guards aren’t compiled or used. BDSP.Core/Validation/CoreGuards
Low: PoffinCooker uses Debug.Assert without System.Diagnostics; with implicit usings, this won’t resolve in net9 by default. PoffinCooker.cs (line 37)
Low: FlavorRules.WeakenedBy does not match its own cycle description; if later used, it will apply the wrong weakening. FlavorRules.cs (line 24)
Cleanup / Optimization Options

Option 1 (Compile-only): Align ContestStats naming and add the missing ctor, fix FeedingOptions.Score, define and use bestNodes, correct BerryTable namespace/using layout, rename CoreGuards to .cs, and add using System.Diagnostics; (or fully-qualify Debug) in PoffinCooker.
Option 2 (Correctness pass): Fix PoffinSearchRunner to partition the combination space (e.g., split by fixed first index per worker) so mixed-berry combos are included; validate cookTimeSeconds > 0, handle empty candidate lists in FeedingOptimizer, and add a couple of tests to lock these behaviors.
Option 3 (Performance pass): Precompute ContestStats (and score) per candidate Poffin, reduce dominance checks by keeping the Pareto front sorted by sheen (binary search pruning), and merge thread-local top‑K results once per worker to reduce lock contention.
If you want, pick an option and I can implement it; I can also propose a minimal set of tests for the runner and optimizer.

BDSP Poffin Optimization System – Technical Architecture & Code Audit

1. Project Structure & Module Relationships

The repository is organized as a multi-project C# solution (BDSP.sln) with distinct layers for core logic, criteria/filtering, serialization, UI, CLI, tests, and benchmarks. Key projects and their roles include:

BDSP.Core: The core library implementing Poffin logic, berry data, cooking algorithms, and contest stat calculations. It defines fundamental data types (e.g. Berry, Poffin, ContestStats) and algorithms for searching optimal Poffins and feeding strategies.

BDSP.Criteria: Encapsulates user-defined filters and sorting preferences. This module provides the PoffinCriteria record (to specify constraints like min flavor values, exclude foul Poffins, etc.) and compiles those into a predicate and comparer for use by the core search. For example, PoffinCriteriaCompiler produces a PoffinPredicate (for filtering cooked Poffins) and a DynamicPoffinComparer (implementing IPoffinComparer for ranking) based on the criteria.

BDSP.Serialization: Handles output formatting (e.g. writing results to JSON). It defines DTOs like PoffinDto and writer classes (PoffinJsonWriter) to serialize top Poffin results for external use.

BDSP.UI: A (presumably WPF or WinForms) user interface layer. It likely contains view-models (e.g. BerryViewModel), mapping utilities (e.g. BerryRadarMapping for plotting flavor profiles), and other UI components. This allows visualizing results (for instance, radar charts or lists of optimal Poffins) without duplicating logic – the UI consumes the core library’s outputs.

BDSP.Cli: A command-line interface providing a text-based way to run optimizations. The CLI parses arguments into a PoffinCriteria, invokes the core search, and prints or saves results. For example, the Program.cs parses flags (like --min-spicy=..., --sort= etc.), builds a criteria object, and runs the search via PoffinSearchRunner.Run.

BDSP.Core.Tests: (If populated) Unit tests to validate core logic (e.g. correct Poffin outcomes, feeding outcomes, filter behavior). Opportunity: Currently, no test methods are evident, suggesting tests are minimal or yet to be written – adding comprehensive tests would improve reliability.

BDSP.Core.Benchmarks: Contains performance benchmarks (likely using BenchmarkDotNet) to measure and tune the efficiency of algorithms (e.g. PoffinSearchBenchmarks.cs to time the search on various inputs). This signals an emphasis on optimizing for speed given the potentially large combination space.

The module relationships are cleanly separated: BDSP.Core implements all game logic and does not depend on UI or CLI, enabling reuse in different frontends (e.g. integrating into a Unity game or a web API). BDSP.Criteria and BDSP.Serialization act as adjuncts to core, providing filtering configuration and data export respectively. The CLI and UI projects depend on Core (and Criteria) to perform computations, demonstrating a clear separation of concerns: the core engine vs. the presentation/interface layers.

2. End-to-End Logic Flow (Berry Selection → Poffin Cooking → Contest Simulation)

Step 2.1 – Berry Filtering / Selection: The process begins by determining which berries are available or allowed for use. In code, the PoffinCriteria.AllowedBerries property can specify a restricted set of BerryIds to consider. In the current CLI usage, this isn’t explicitly exposed (the default is all 65 berry types), but the structure supports limiting the berry pool (e.g. excluding rare berries or those the user doesn’t have). The BerryTable defines a canonical list of all berry stats (flavor values, smoothness, rarity) indexed by BerryId. If no filter is applied, the berry pool is all berries from the table. If filtering is needed (e.g. user excludes “rarity > 7” berries), that would be applied to form a smaller berryPool array.

Step 2.2 – Generating Berry Combinations: Given the berry pool, the system generates all unique combinations of a certain size (number of berries per Poffin). By default, 4 berries per Poffin is used (the in-game maximum), but this is configurable (via PoffinCriteria.BerriesPerPoffin). The BerryCombinations static class efficiently enumerates combinations without allocation. It provides overloaded ForEach methods for combination sizes 1–4, each using nested loops and a stack-allocated buffer to yield combinations of Berry IDs. Notably, it inherently avoids duplicate berries in a combo (each inner loop starts from the next index) – ensuring combinations like (Cheri, Cheri, Oran, Pecha) are not produced. This aligns with game rules (using the same berry twice results in a foul Poffin). If AllowDuplicates were false in criteria, the search would also skip combos with repeats entirely (though by logic it already does).

Step 2.3 – Cooking a Poffin: For each combination of BerryIDs, the Poffin cooking formula is applied. The core uses PoffinCooker.Cook(...) to simulate the in-game cooking math. The cooking logic follows the BDSP game rules for flavor combination and output Poffin stats:

It first sums the five flavor values from all berries in the combo (Spicy, Dry, Sweet, Bitter, Sour) and the total smoothness. Any duplicate berry in the combo triggers an immediate foul result, returning a low-level “foul” Poffin placeholder.

It then performs “flavor weakening” in a specific sequence:

Spicy is reduced by Dry, Dry by Sweet, Sweet by Bitter, Bitter by Sour, and Sour by Spicy. This models how opposing flavors cancel out, as in the original Pokémon formula.

Next, it applies a negative-flavor penalty: if any flavor value went negative after weakening, it counts how many are negative and subtracts that count from all flavor values. This uniformly penalizes the mix if some flavors clash strongly (bringing some below 0).

A time factor is applied: All flavor values are scaled to a 60-second reference by value = value \* 60 / cookTimeSeconds. In BDSP, optimal cooking is achieved at 60 seconds; a shorter cooking time lowers the resulting flavors (the code defaults to 40 seconds as a typical cooking duration, meaning values are ⅔ of their max).

A mistake penalty subtracts the number of stirring errors from each flavor (by default errors=0 in the search, assuming perfect cooking).

Flavor values are then clamped between 0 and 100 (the game’s max for any single flavor contribution).

If all five flavors end up zero (i.e. the combination yields nothing), it’s treated as a foul Poffin (returned with level 2, type = Foul).

The Poffin’s Level is set to the highest flavor value among the five; the SecondLevel is the second-highest flavor (this is used for determining Poffin type and naming).

Smoothness of the resulting Poffin is computed from the average berry smoothness minus any Amity bonus: smoothness = (avg berry smoothness) - amityBonus (with a cap that bonus ≤9 and result ≥0). In practice, an Amity Square bonus of 9 reduces smoothness significantly (mimicking the in-game mechanic where high friendship yields smoother Poffins).

The Poffin is then classified by type based on its flavor profile:

SuperMild (if level ≥ 95 and no errors),

Mild (if level ≥ 50),

SingleFlavor (if exactly one flavor >0),

DualFlavor (if two flavors >0),

Rich (if three flavors >0),

Overripe (if four or five flavors >0).

Finally, the Poffin’s primary and secondary flavor are determined by finding the highest flavor value and the second-highest (if nonzero). This helps label the Poffin (e.g. “Dry-Sweet Poffin” if Dry is highest and Sweet second).

All these calculations produce an immutable Poffin struct containing its Level, Smoothness, each flavor value, type, and primary/secondary flavor attributes. The logic closely mirrors known Pokémon Poffin mechanics (with explicit weakening and penalties) ensuring the system’s output aligns with in-game expectations.

Step 2.4 – Filtering & Ranking Poffins: As each Poffin is cooked, it is immediately filtered and possibly stored in a running top-K list:

A predicate (if provided) screens out Poffins that don’t meet criteria (for example, exclude foul Poffins or those below a certain level/flavor threshold). The search runner calls the predicate right after cooking and skips adding the Poffin if it returns false. The predicate is compiled from PoffinCriteria: it checks flags like ExcludeFoul (skip if p.Type == PoffinType.Foul) and minimum stat requirements (p.Level < MinLevel, p.Spicy < MinSpicy, etc.). For example, if MinSpicy=40 in criteria, any Poffin with Spicy < 40 is filtered out.

Each Poffin that passes is assessed by a comparer to maintain the best topK Poffins. The code uses a TopKPoffinSelector (a min-heap or partial selection structure) to track the highest-ranked Poffins seen so far. The ranking is defined by the IPoffinComparer given, often a DynamicPoffinComparer built from the sort fields in criteria. By default, the primary sort is Level (descending) and secondary is none or Smoothness (ascending). The dynamic comparer’s IsBetter(a, b) implements a lexicographic order on the chosen fields: e.g. compare Poffin Level, and if equal, compare Smoothness, favoring lower smoothness if configured. (Notably, Smoothness is compared in reverse internally to treat lower smoothness as “better” when sorting descending, aligning with the idea that a Poffin that uses less sheen is more valuable.)

The search runs in parallel across the berry combination space for performance. The berry pool is partitioned among worker threads to generate combos in parallel. Each thread keeps a local topK and then merges results under a lock into the global topK selector. This parallel strategy, combined with the efficient combination generator and on-the-fly filtering, allows scaling to the entire berry set (677,040 combos for 4 out of 65) reasonably fast.

The output of this stage is a PoffinSearchResult containing the top N Poffins found (with N = TopK from criteria, e.g. top 50). These are the best individual Poffins according to the chosen sort metric.

Step 2.5 – Contest Stats Feeding Simulation: The final step simulates feeding those Poffins to a Pokémon to maximize its Contest conditions (Coolness, Beauty, etc.). This is handled by the Feeding Optimizer. The PoffinFeedingSearchRunner.Run orchestrates it in two phases: (1) search for top Poffins (already done above), then (2) find an optimal combination of those Poffins to feed.

The feeding optimization uses a bounded depth-first search over combinations of the candidate Poffins:

It starts with an “empty” feeding state (0 sheen, all contest stats 0).

It explores adding each Poffin (from the topK list) one by one, without repetition, accumulating the contest stats until the sheen (a measure of how “full” the Pokémon is) reaches the maximum (255 in BDSP). Each Poffin’s smoothness contributes to sheen, and each flavor contributes directly to a contest stat: Spicy to Coolness, Dry to Beauty, Sweet to Cuteness, Bitter to Cleverness, Sour to Toughness. (The ContestStatsCalculator.FromPoffin simply converts positive flavor values to stat points.)

The search prunes suboptimal states using Pareto dominance: If a partial feeding plan yields a set of stats that is worse or equal in every category (and not better in any) compared to another plan with ≤ the same sheen, it is dominated and not further explored. This keeps only the Pareto-optimal front of feeding outcomes at each step, drastically cutting down the combination explosion.

A limit MaxNodes (default 100,000) is set to prevent infinite or excessive branching. This is a safety for performance, though in practice the dominance pruning typically curtails the search early.

The objective is to maximize the contest stats and secondarily use as much sheen as possible (since using more sheen means feeding more Poffins, improving stats). The FeedingOptions.Score function defines how to score a final stat line – by default it sums all five stats (balanced approach). One plan dominates another if it has a higher score, or equal score but higher sheen usage (filling the Pokémon as much as possible). The code picks the best plan by comparing the final ContestStats of each non-dominated state via this score.

Once the best terminal node is found, the algorithm reconstructs the sequence of Poffins (the feeding plan) by tracing parent pointers from that node back to the root. The result is a FeedingPlan object containing the chosen Poffin list and the final FeedingState (with total Sheen and final stats).

Result: The end-to-end result is an optimized set of Poffins to feed that yields maximal contest stats. For example, the system might output the top 5 Poffins to feed a Pokémon to maximize all conditions, or if using a preset (e.g. Coolness-focused), it will find the Poffins that maximize Coolness within the sheen limit. The CLI prints the top Poffins and their stats, and if --json is specified, writes them to poffins.json via the serialization module.

In summary, the flow is: select berry pool → generate combinations → cook Poffins → filter/sort best Poffins → feed optimize those Poffins → produce final contest stat outcomes. This pipeline is well-defined in code with clear separation at each stage, allowing targeted improvements or alternative usage at any point (e.g. skipping feeding optimization if only a single Poffin recipe is needed).

3. Filter & Sort Implementations (Berries, Poffins, Contest Stats)

The system provides filtering and sorting mechanisms at multiple levels, each appropriate to its domain:

Berry-level Filtering: While not heavily used in the current CLI, the design allows specifying which berries or how many berries to include. The PoffinCriteria includes an AllowedBerries list and an AllowDuplicates flag. In principle, one could set AllowedBerries to only common berries, or exclude a specific berry. The combination generator and search would then naturally be restricted to that subset. Additionally, BerriesPerPoffin (default 4) controls the combination size – effectively filtering out any mixes not of that length. If the user wanted to explore smaller recipes (1, 2 or 3-berry Poffins), they can adjust this parameter. Another implicit filter is by rarity or other berry attributes – although no explicit criteria for rarity exists in code, the UI or CLI could easily allow “max rarity” as input, then derive an AllowedBerries list by filtering BerryTable where Berry.Rarity <= X.

Poffin Filtering (Post-cook): The primary filtering happens after a Poffin is cooked. The PoffinCriteria record defines a comprehensive set of filters:

A boolean ExcludeFoul (default true) to drop any foul results.

Minimum thresholds for Poffin Level and each flavor stat (Spicy/Dry/Sweet/Bitter/Sour). For example, MinDry=40 means only consider Poffins with Dry flavor ≥40.

An optional MaxSmoothness to ignore Poffins above a certain smoothness (useful if you only want low-smoothness Poffins that can be fed in bulk).

These criteria are compiled into a delegate PoffinPredicate that returns false if any condition is violated. The search uses this to filter out unwanted Poffins before ranking. For instance, with ExcludeFoul=true and MinSpicy=40, any Poffin that turned out foul or has Spicy < 40 is immediately skipped. This prevents “junk” Poffins from consuming memory or ranking slots. In practice, presets make heavy use of these filters – e.g. the “Cool” preset sets MinSpicy=40 and MaxSmoothness=25 to target only high-Spicy, low-smoothness Poffins for Coolness.

Poffin Sorting (Ranking): To identify the “optimal” Poffins, the code doesn’t just rely on Level; it allows dynamic multi-field sorting:

The SortField enum defines sortable attributes: Level, Smoothness, and each flavor stat.

The SortDirection enum allows Ascending or Descending order.

The PoffinCriteria holds a PrimarySort and optional SecondarySort with directions. By default this is Level (desc) and then none/implicit Smoothness.

The DynamicPoffinComparer uses these settings to implement IPoffinComparer.IsBetter(a,b). It compares the primary field on the two Poffins; if one is greater, that decides based on desired direction. If primary values are equal, it falls back to secondary field. For example, with primary=Spicy (desc) and secondary=Smoothness (asc), it will rank higher Spicy values first; ties in Spicy are broken by lower smoothness (the comparer’s logic inverts smoothness comparison so that a lower smoothness Poffin is considered “better” when sorting descending). If secondary is not provided or also ties, it treats them as equal rank.

This flexible sorter is leveraged by both CLI and presets: the CLI --sort field:asc|desc --then field:asc|desc flags feed into PrimarySort/SecondarySort, and presets like “Tough” set PrimarySort=Sour (desc) and SecondarySort=Smoothness (asc) to surface the highest Sour Poffins, preferring those that use less sheen. This approach allows different optimization goals (e.g. best single-stat Poffins vs overall highest level Poffins) without altering core logic.

Contest Stats / Feeding Filters: In the feeding optimization, the concept of filtering appears as state dominance pruning rather than user-defined criteria. The ContestStats struct defines a Dominates method to compare two stat sets. It returns true if every contest stat in one state is >= the other’s (with at least one strictly greater), ignoring sheen. The FeedingOptimizer uses this to drop any intermediate feeding state that is dominated by another. This is effectively a filter on the search space: it ensures the algorithm only explores non-dominated (potentially optimal) combinations of Poffins.

Additionally, scoring in FeedingOptions acts as a sorter for final outcomes. The default score sums all five contest stats, which biases the optimizer to maximize total condition points. This can be customized (e.g. one could provide options.Score = stats => stats.Coolness to only maximize Coolness). The best plan is chosen by comparing scores, with a tie-break on used Sheen. Thus, while the user doesn’t directly specify “filters” for contest stats, the system inherently focuses on Pareto-optimal stat outcomes and highest-scoring outcome, which is an implicit sorting of contest results. If needed, one could extend FeedingOptions to impose, say, a minimum required stat or prioritize one stat (via custom Score or modifying Dominates logic).

In summary, filtering is primarily handled by PoffinCriteria (for Poffin properties) and dominance rules (for contest stat states), and sorting is handled by the DynamicPoffinComparer (for Poffin ranking) and the score function (for final stat outcomes). The Berry selection stage could be seen as an initial filter on inputs. Each of these layers is cleanly separated: criteria for Poffins don’t bleed into feeding logic, and contest stat ranking is isolated in the FeedingOptions/optimizer. This design makes the optimization process highly configurable and allows easy adjustments (e.g. adding a new filter field or sort option) without touching the core algorithms.

4. Opportunities for Expansion, Optimization & Portability

Overall, the codebase is high-quality and performance-minded (use of Span, stackalloc, parallelism, etc.), but there are areas to consider for enhancement:

Scalability to Larger Datasets: The current solution efficiently handles the BDSP scenario (65 berries, combos up to 4). If we imagine scaling beyond – e.g. including all berries from later generations or allowing >4 berries – the combination count grows combinatorially. The code’s use of parallel threads and on-the-fly filtering is crucial. Potential optimizations:

Prune combination generation early: If certain criteria are known beforehand, integrate them into combination generation. For example, if MinSpicy=40, skip combos whose total Spicy couldn’t possibly reach 40 (by summing top Spicy values of chosen berries so far). Currently, filtering happens after cooking, but a smarter combo generator could eliminate some combos earlier.

Parallel tuning: The partitioning in PoffinSearchRunner uses a simple round-robin split of the berry list among threads. This yields equal-sized partitions but not necessarily equal workload (because combinations count varies with each partition’s berries). In practice with 65 berries this is fine, but for uneven pools, dynamic scheduling (e.g. using Parallel.ForEach with range-chunking) could better balance thread load. However, given the relatively small combination count (~677k for choose-4), the current approach is likely sufficient.

Memory and GC: The use of struct Poffin and stackalloc means garbage generation is low. One improvement could be reusing TopKPoffinSelector across runs or object-pooling the combo buffers if memory churn becomes an issue. But again, current usage is modest and these are micro-optimizations.

Vectorization/GPU: For extreme scaling, one could consider vectorized operations for cooking (since formulas are arithmetic on five parallel values) or even offloading combo evaluation to a GPU. However, given the complexity of conditional logic (weakening, clamping, etc.), maintaining accuracy might be hard. It’s likely unnecessary for the domain sizes at hand.

Extending the Core Logic: The architecture allows relatively easy extension:

Support for Different Games/Rules: If one wanted to adapt this to another Pokémon game’s system (e.g. Poffins in original D/P, Pokéblocks in Gen III, or Poffin-like mechanics in future titles), one could create a parallel Cook method or a parameterized formula. The current PoffinCooker is specific to BDSP Gen8 rules, but by abstracting “cooking rules” (perhaps via strategy pattern or an interface), the core search could be reused. For now, a comment or design note indicating this possibility could future-proof the project.

Nature or Affection Effects: In BDSP contests, a Pokémon’s nature affects which flavors it likes/dislikes, which historically gave +10 or -10 bonus to contest stats when feeding. This system currently doesn’t model that. An expansion could include a parameter for Pokémon nature to adjust ContestStatsCalculator.FromPoffin (e.g. if nature dislikes Bitter, maybe subtract some Cleverness points). This would integrate with FeedingOptimizer’s score for a more personalized optimization. Implementing this cleanly would involve passing a context (nature preferences) to the ContestStats calculation or adjusting the score function accordingly.

Combinatorial Berry Selection (“Factory” features): The separate BDSP-Poffin-Factory repository (Python) suggests an interest in brute-forcing combinations or generating recipe databases. The C# core could add functionality to enumerate all Poffins (not just top K), perhaps outputting them to a file (which the CLI already partially does with --json for top results). With the existing code, setting TopK equal to the total combination count and a trivial comparer would effectively list all combos. However, that might be millions of results – instead, one could implement targeted generation (e.g. “list all Poffins above Level X”).

Refinement of Presets: The preset definitions in PoffinPresets might need tweaks (there appears to be a minor mix-up: “Beauty” preset uses MinSweet but likely should use MinDry, and “Cute” uses MinDry but likely meant MinSweet). Clarifying and testing these presets would be a small polish. Beyond that, adding more presets (e.g. a “Balanced” preset that aims for all stats equally, or “MaxTotal” preset focusing purely on sum of stats) could be useful. These presets simply produce different criteria and thus leverage the same engine.

Portability to a C# Core Library: The project is already structured as a reusable C# library (BDSP.Core), which is excellent for integration (e.g. Unity game modding, a .NET API, etc.). To further polish for a standalone library:

Ensure thread-safety for all static methods if they might be called concurrently in different contexts. Currently, most state is local or immutable. One area to check is BerryTable.\_berries – it’s a static array. It’s never modified after initialization, so reading it concurrently is fine. The combination generators use local stack spans, also fine. Global singletons like BerryTable and the static random (if any used for foul randomization, though right now foul Poffins are deterministic with fixed values) should remain read-only or be protected if writing.

API design: Provide clear entry points for library consumers. For example, a single method that takes a PoffinCriteria and returns a FeedingPlan would be convenient. Right now, the CLI essentially does this by calling PoffinFeedingSearchRunner.Run with criteria-derived parameters. Exposing a similar high-level API (maybe a facade class or static helper in Core) would make the library easier to consume without understanding all internals.

Consider packaging the core logic as a NuGet library. This would require adding proper documentation comments (many of which exist) and perhaps refactoring the Program.cs example usage into documentation or a sample.

Performance Benchmarks & Optimizations: The existence of BDSP.Core.Benchmarks indicates a focus on optimization. Some potential additional optimizations:

FeedingOptimizer algorithm: It currently does a DFS with dominance pruning. This is akin to a depth-bounded knapSack solver with 5-dimensional value. The approach is good, but it can explode if topK is large (imagine top 200 Poffins – combinations of them would be huge). If needed, one could implement heuristic pruning, e.g. sort candidates by a specific stat and try greedy fills for upper bounds, or use dynamic programming for an approximate solution. However, given topK default is 50, the search space is manageable with pruning. Still, providing a maxPoffinsToFeed limit or optimizing the search order (currently it’s by index order) could be explored. The code uses LastIndex to avoid repeats and to ensure combinations rather than permutations, which is correct.

Parallel feeding optimization: The feeding search is single-threaded DFS. Since this is a tree search, parallelizing it is tricky but not impossible (e.g. different threads exploring different branches). The complexity might outweigh benefits due to the need for thread-safe management of the bestNodes list. Given feeding optimization is quite fast for top 50 items, parallelization is likely unnecessary.

Use of newer .NET features: If targeting .NET 5/6+, one could use Parallel.ForEach with spans directly, use BitOperations for some math, or even consider Vector<int> for batch flavor arithmetic. Such micro-optimizations should be guided by benchmark data. The current code already leverages AggressiveInlining hints on critical small methods and avoids unnecessary allocations, which is great.

Polish & Maintainability: Some expansion points are more about code quality:

Increase comments and docs where complex logic occurs. The PoffinCooker and FeedingOptimizer have brief summaries, but in-line comments explaining the rationale of the formula (especially the weakening steps and negative penalty) would aid future maintainers. The formula is non-trivial; documenting its origin (likely from community or official sources) in /// <summary> would be helpful.

Unit tests: It’s crucial to validate that the PoffinCooker outputs correct values by comparing to known in-game examples (if available). For instance, craft some test combinations of berries where the expected Poffin result is known (e.g. from a guide) and assert the Poffin struct matches those values. Similarly, tests for the FeedingOptimizer could simulate a simple scenario (like only two candidate Poffins) and verify it chooses the right one(s) under a sheen cap. Edge cases like “all foul berries” or “no possible Poffin meets criteria” (should return empty results) should be tested. Currently, the separation of concerns is good, which will make writing tests for each piece (cooking, filtering, sorting, feeding) easier. For example, one can test PoffinCriteriaCompiler by feeding a criteria and checking that the resulting predicate and comparer behave as expected for some sample Poffins.

Logging or Debugging Aids: For a complex search, having an option to log progress or stats (how many combos tried, how many Poffins filtered, etc.) might be useful. This could be behind a debug flag or using Debug.WriteLine in conditional compilation. It helps in diagnosing performance issues or verifying that filters are working as intended (e.g. see that foul Poffins count dropped to zero when exclude is true).

Error Handling: The CLI currently throws exceptions on bad input (e.g. if --sort value is invalid, it uses Enum.Parse which will throw). It could be made more user-friendly by catching and printing usage guidance. Similarly, feeding optimization silently truncates if MaxNodes is exceeded; it could output a warning or at least document that the result might be suboptimal if the search was cut short. Ensuring the library surfaces such conditions (maybe via a flag in the result) would be valuable for correctness.

In short, the codebase is already efficient and modular. Scaling it further or translating it to a production-grade C# library would involve tuning performance parameters, extending flexibility (game rules, objectives), and hardening via tests and documentation. Given it was likely ported from a Python prototype (the “Factory”), verifying that the C# implementation produces identical results to the Python version on sample data would be a good validation step as well.

5. Improving Maintainability: Tests, Documentation, and Clarity

While the code is generally clean, adding the following would greatly improve maintainability:

Unit and Integration Tests: As noted, there are currently few if any tests checked in. Key areas to test:

Berry Data Integrity: Ensure BerryTable.Get(id) returns expected values for a known berry (e.g. Oran Berry has specific flavors). This ensures no typos in the data constants.

PoffinCooker: Test specific known recipes. For example, in Gen4 a mix of Wiki, Kelpsy, Cornn, Pamtre (all Dry) should yield a high-Level Dry Poffin – we can verify the Level and flavors match expectations. If exact expected outcomes aren’t known, at least consistency tests (e.g. cooking the same combination twice yields the same result, changing cooking time or errors affects output in sensible ways).

PoffinCriteria filtering: Construct a small list of Poffins programmatically and apply a PoffinPredicate to ensure it filters correctly. E.g. make a list of Poffins with various Smoothness and verify that setting MaxSmoothness=20 filters out those above 20.

DynamicPoffinComparer: Test comparisons in isolation. E.g. given two Poffins, ensure that for each SortField the comparer orders them correctly (especially for Smoothness asc/desc logic). This could catch issues like if Smoothness handling was inverted or if secondary sort is not applied when primary equal.

FeedingOptimizer outcome: Use a contrived small scenario: say only two candidate Poffins – one that boosts only Coolness a lot with high smoothness, another that boosts a bit of each stat with low smoothness. Verify that if focusing on total points, it picks the latter (if that yields higher sum) or if focusing on one stat (via custom Score) it picks accordingly. Also test that sheen capping works (no sheen > 255 in result, and if we set MaxSheen to a smaller number in FeedingOptions, the plan respects that).

End-to-End: Using the CLI or a direct call, run the full optimization with a known preset (like “Cool”) and check that the output Poffins indeed all have high Spicy and low smoothness (criteria satisfied) and that the feeding plan’s final stats reflect maximizing Coolness primarily.

Adding these tests will increase confidence when refactoring or extending features. They could also catch the preset swap bug mentioned (a test expecting the “Beauty” preset to focus on Dry would fail, revealing the criteria mix-up).

Documentation & Comments: Many classes have XML <summary> comments, which is great for generating docs. Some could be elaborated:

For BerryTable and Berry struct, note the meaning of each field (units or ranges). E.g. “Flavor values range 0–40 typically; Smoothness ~0–60 (lower means less sheen), Rarity 1–15 (lower is common).”

In PoffinCooker.Cook, step through the formula in comments. Each block of code (duplicate check, weakening, negative penalty, time scaling, etc.) can have a brief comment referencing it to game mechanics or why it’s done in that order. This will help new contributors (or your future self) recall the reasoning without re-deriving it.

Document the preset rationale: e.g. “Maximize Cool – requires Spicy ≥40 (roughly corresponds to Poffin Level ~40+ in one stat) and cap Smoothness to 25 to allow feeding ~10 such Poffins (25\*10=250 sheen).”

Add usage examples in README.md or project wiki. The README is currently empty or minimal; filling it with an overview of how to run the CLI, what the output means, and how to use the library in code would be valuable. Given the target of a C# core library, showing a snippet of calling PoffinFeedingSearchRunner.Run with a custom criteria in a C# project would attract potential users/developers.

Code Style & Consistency: The code already follows modern C# conventions (use of static for utility classes, record for Criteria, etc.). Minor suggestions:

The project uses record for PoffinCriteria, which is immutable (init-only props) – this is great for assembling criteria via with expressions. We should ensure all comparable config structures follow immutability for safety (it appears so).

Some internal variables or methods could have more descriptive names. For instance, in FeedingOptimizer, bestNodes is actually storing FeedingNode states (each node has parent link and state). It might be clearer to call it frontier or paretoFront instead of bestNodes, but this is subjective. Comments can clarify its purpose (stores all non-dominated states found so far).

Ensure error messages are informative. The CLI throws ArgumentException("Sort must be field:asc|desc") if parsing fails; that’s fine. Perhaps also handle unknown presets gracefully (currently throws in ApplyPreset) – could list available presets in the message.

Version Control & Collaboration: As the codebase grows or if multiple contributors join, having clear documentation will aid onboarding. The structured separation into projects is already beneficial. To further support collaboration:

Write down the algorithmic approach in a docs/notes file (maybe what NOTES.md was intended for). For example, describe the choice of DFS + dominance for feeding, or the complexity considerations that guided using top-K selection instead of generating all Poffins. This can justify design decisions and help future optimizations stick to the original intent.

Track known issues or desired features via GitHub Issues (for example, an issue noting “Confirm and correct preset criteria values” or “Add nature effect support”).

If planning to port this to Unity or another platform, note any Unity-specific concerns (like Unity’s lack of parallel LINQ on older versions, etc.). Currently everything is standard .NET, so portability to Unity (especially since it’s .NET Standard) or to a Xamarin app is feasible.

In essence, by solidifying tests and documentation, the project will be easier to extend (or debug if outputs ever seem off). Given the complex math, having those safety nets is important to avoid regressions when optimizing further.

6. Separation of Concerns & Future UI/API Integration

The existing architecture already demonstrates good separation of concerns, which is beneficial for adding new interfaces (such as a GUI tool or a web API):

Core Logic vs Presentation: All game-specific logic resides in BDSP.Core (and partly BDSP.Criteria for config). Neither the CLI nor UI contains any hard-coded logic about flavors or Poffin math – they strictly call into the core. This means a future GUI (e.g. a Xamarin or MAUI app for mobile, or a Blazor web UI) can reuse the core library exactly as the CLI does, by constructing a PoffinCriteria from user inputs and then invoking the search. The UI project already suggests some MVVM structure (ViewModels), indicating the intent to bind UI controls to core outputs. For example, BerryViewModel likely exposes berry properties for display, using data from BerryTable. Ensuring the ViewModel only formats data (and does not recalculate stats) will keep the concerns separate – it should call core functions for any calculations needed when a user interacts (e.g. if a user selects different berries, the ViewModel can gather BerryIds and call the core search).

API Integration: For a web API, one might expose endpoints like “GET /poffins?minSweet=40&topK=10” etc. This would internally call the core library. The core being free of static global state (aside from constant tables) means it’s inherently thread-safe for use in an API (multiple requests can run optimizations in parallel threads, limited by CPU). The design of stateless static methods and pure functions (e.g. Cook always yields same output for same input) is ideal for reproducibility and testing in an API environment.

Clean Interfaces: To support integration, consider exposing higher-level interfaces or simplifying usage:

A method like Optimizer.FindTopPoffins(PoffinCriteria criteria) -> PoffinSearchResult and Optimizer.OptimizeFeeding(Poffin[] candidates, FeedingOptions options) -> FeedingPlan would encapsulate the multi-step process. Right now, PoffinFeedingSearchRunner.Run already does both steps. We might refactor naming (e.g. OptimizationRunner.RunAll(...) or simply make it clear in docs that this is the main entry point for one-shot use).

The output types (PoffinSearchResult and FeedingPlan) should be simple DTOs or contain mostly data. FeedingPlan currently holds a list of Poffins and final state. It could be useful to add convenience methods or properties, e.g. TotalContestStats or UsedSheen so that UIs don’t have to manually sum or interpret the result.

Maintain the one-way dependency: Core knows nothing of Criteria or UI. Currently, PoffinCriteria is in a separate assembly but it does reference BDSP.Core types (e.g. uses BerryId from core, and returns IPoffinComparer from core). This is fine as long as Core doesn’t depend on Criteria. It might even be possible to merge Criteria into Core without harm (they are tightly related), but keeping them separate allows swapping out criteria logic if needed.

Serialization is separate, which is good. For a clean separation, the core library shouldn’t inherently depend on JSON or any specific output format. Currently, it doesn’t – PoffinJsonWriter is isolated in BDSP.Serialization and the CLI calls it when needed. This modularity is great for an API: the API layer can choose to format results as JSON or XML using its own tools (or even directly return the FeedingPlan object to a JSON serializer).

Future UI Considerations: If a rich UI is planned (perhaps showing interactive charts of flavor profiles or allowing berry selection via checkboxes):

The UI should use data-binding to present results from the core. E.g. after running an optimization, bind a list control to FeedingPlan.Poffins to show each Poffin with its stats, and maybe a radar chart that uses Poffin.PrimaryFlavor/SecondaryFlavor for coloring. The existence of chart_1.PNG and BerryRadarMapping.cs suggests such visualizations are in progress.

Keep heavy computations off the UI thread. Since the core can run on background threads (especially the parallel search), a UI should dispatch the call asynchronously and then marshal the results back for display. This is straightforward given the core’s static methods – just ensure to call them in a Task or background worker from the UI.

Real-time interaction: If planning features like “as the user toggles berry inclusion, recompute immediately,” consider performance. The current engine might be fast enough for near-real-time updates with a subset of berries (hundreds of thousands of combos can compute in a fraction of a second in optimized C#). But if it becomes slow, one could precompute or incrementally update results. A clear separation makes it possible to use caching strategies externally (e.g. cache last results and diff if criteria changes slightly) without complicating the core.

Separation of Data Concerns: The berry data (names, flavors) is currently embedded in code (BerryTable and BerryNames). For integration, one might consider loading this from a resource file or database if extending to other games or allowing user edits. While not urgent, abstracting the data source could be an improvement: e.g. have IBerryProvider interface that BerryTable implements, so that in future one could plug in a different dataset (if someone modded berry values or wanted to simulate a different contest environment). Right now, this is hard-coded, which is fine for BDSP but less flexible. A middle-ground is to at least clearly document BerryTable as BDSP-specific constants.

Porting to Other Languages (C# Core Library focus): The question mentions translating to a C# core library – presumably this repository is that translation (likely from Python). If there’s a desire to port to C++ or another language for performance, having well-defined modules helps. For instance, one could reimplement the PoffinCooker and FeedingOptimizer in C++ and keep the high-level structure similar. However, given the current performance of C#, this might not be needed. If targeting Unity (which prefers C#), the code is already in the right language.

Plugin Architecture: Envisioning an ecosystem where UI or API might want to plug in new criteria or scoring functions – the current design supports this to an extent:

New criteria fields can be added to PoffinCriteria and handled in CompilePredicate easily (just extend the if checks).

A completely new ranking scheme (say multi-objective beyond two fields) could require writing a new IPoffinComparer implementation, which the search runner can take as long as it’s passed in. The CLI only uses DynamicPoffinComparer, but an API integration could create a custom comparer (perhaps to rank by a weighted combination of stats).

The feeding optimization’s Score function is already a delegate in FeedingOptions, so one can supply any custom scoring. This is excellent for separation – e.g. a user could decide that getting at least 200 in each stat is the goal, and implement a score function that heavily penalizes any stat below 200. The core will then optimize according to that. We should ensure this flexibility is documented for advanced users.

In conclusion, the code is modular and ready for integration. The main recommendations are to preserve and strengthen these separations: avoid any temptation to put UI logic in the core (which hasn’t been done, good), keep criteria and output formatting as add-ons, and document the interfaces between them. With thorough documentation and testing, adding a new UI or API layer should require zero changes to the core – only using the core’s public API. This will allow the BDSP Poffin optimizer to serve as a true core engine that can power multiple frontends (CLI, GUI, web, etc.) with consistent results and minimal duplication of logic

Roadmap to Complete BDSP.Core Poffin Logic Parity
Missing Features & Gaps in BDSP.Core vs. Python Factory

Incomplete Combination Search: The C# search splits the berry pool across threads, missing mixed-berry combinations (partitions don’t cover combos spanning partitions). This yields incomplete results and needs redesign.

No Support for Duplicate Berries: BDSP.Core currently generates combinations without repetition (each combo uses unique berries). The Python factory allows duplicate berries in a recipe (though those yield foul Poffins). The C# core needs a way to include duplicate-berry combos when allowed.

Limited Filtering Criteria: The C# PoffinCriteria only supports a few filters (exclude foul, min level, max smoothness, min each flavor). In contrast, the Python system can filter by Poffin flavor count, specific flavor type, name category, etc. (e.g. filtering by number of flavors or Poffin name). These additional filters are not yet exposed in BDSP.Core.

Limited Sorting Options: The current SortField enum covers only Level, Smoothness, and individual flavors. Python’s PoffinSort supports sorting by secondary flavor strength, flavor count, Poffin name/type, and custom ratios. BDSP.Core lacks equivalents for “second level”, “num_flavors”, or “level/smoothness ratio” sorts.

Contest Stats Evaluation Output: The Python factory can generate and filter a list of multi-Poffin ContestStats results (feeding outcomes) with various criteria (min stats, max berries used, etc.) and sort them. BDSP.Core’s FeedingOptimizer finds only a single optimal plan and provides no built-in filtering/sorting of alternate feeding plans.

Miscellaneous Incomplete Implementations: Several core classes are partially implemented or contain bugs. For example, ContestStats naming mismatches (Coolness vs. Cool, etc.), missing constructors, and PoffinCooker lacking input validation (e.g. zero cookTime causing divide-by-zero). These need fixes to align with the Python logic and ensure correctness.

Implementation Plan by Module

1. Berry Combination Generation (BDSP.Core.Berries)

Enable Duplicate-Combinations: Extend BerryCombinations to generate combinations with replacement when duplicates are allowed. This could be a new method or a parameter in ForEach (e.g. allowDuplicates flag). If AllowDuplicates is true in criteria, use loops where inner indices start from the current index (i ≤ j) instead of i+1 for combination generation, so the same berry can appear multiple times.

Parallel Search Refactor: Refactor PoffinSearchRunner.Run to partition the combination space instead of the berry pool. For example, assign each worker a range of first-berry indices (so one thread handles combos starting with berries 0-3, next with 4-7, etc.) rather than disjoint pools. This ensures combinations that mix berries from different index ranges are included. Remove or redesign the current Partition() logic so that all N-choose-K combinations are covered across threads without omission.

Use Allowed Berry List: Filter the input berryPool based on PoffinCriteria.AllowedBerries before running combinations. The CLI or UI should construct the berryPool from the allowed IDs (or exclude disallowed ones). This ensures the combination generator only uses permitted berries.

Early Pruning of Foul Combos: As an optimization, skip generating combinations that will definitely be foul when the user excludes foul Poffins. Since any recipe with a duplicate berry yields a foul Poffin in the current rules, the combination loop can avoid duplicates entirely if ExcludeFoul is true (which it is by default). This prevents wasted cooking of combos that will be filtered out anyway.

Design Note: We might implement duplicate support with a separate set of combination functions (ForEachWithReplacement for K=2..4). Alternatively, we generate duplicates by inflating the berry pool (e.g. include multiple instances of each berry if duplicates allowed). The chosen approach should maintain determinism (consistent ordering of combos) to keep the search results stable.

2. Poffin Cooking & Data Model (BDSP.Core.Poffins)

Finalize PoffinCooker Logic: Complete the PoffinCooker.Cook implementation to mirror the Python’s Poffin calculation. Ensure that it correctly computes each flavor’s value, the Poffin’s Level (highest flavor), SecondLevel (second highest flavor), Smoothness, and assigns the proper PoffinType. This includes replicating the naming rules from the Python version (e.g. mark Poffin as Foul if any duplicate berry used, set Super Mild if Level ≥ 100, Mild if Level ≥ 50, Rich for 3 flavors, Overripe for 4 flavors, etc.). The PoffinType enum already covers these categories – ensure the Cook method chooses the correct type and that the PrimaryFlavor and SecondaryFlavor fields are set (e.g. primary flavor = flavor with Level value, secondary = flavor with SecondLevel value).

Input Validation: Add guard checks in Cook for parameters like cookTimeSeconds. The Python uses a decorator to measure time but does not pass zero; in C#, explicitly throw or handle if cookTime is 0 (to avoid division by zero when calculating burn adjustments, etc.). Similarly, validate the range of errors and amityBonus if those affect the formula. Use CoreGuards (after renaming to .cs and including it) to enforce valid ranges.

Negative Flavor Handling: Confirm that no flavor value goes negative after cooking (the Python logic likely prevents negatives). Add an invariant check or clamp to 0 if needed, so that all Poffin.Spicy/Dry/... are ≥ 0. This keeps outcomes consistent with game rules (no negative contest stats).

Expose Flavor Count: To support filters like “number of flavors,” consider adding a derived property or method on Poffin to get the count of flavors > 0. Python computes this as num_flavors. In C#, we could compute on the fly in the predicate, but having it readily available (or even storing in the Poffin if memory is not a concern) would simplify sorting/filtering by flavor count.

Design Note: The Poffin formula should be thoroughly tested against known outputs (possibly using GoldenPoffinTests.cs). Pay special attention to foul Poffin output – the invariant test expects foul Poffins to have exactly three flavor values equal to 2, indicating a very specific outcome. Reproduce these quirks in C# so that the core logic matches the Python reference and game behavior.

3. Filtering & Sorting System (BDSP.Criteria)

Extend PoffinCriteria Filters: Add new fields to PoffinCriteria to cover the full range of Python filters. For example, MinFlavors and MaxFlavors (to require a minimum/maximum number of flavor types in the Poffin), and possibly a way to filter by Poffin name/type (e.g. exclude “Mild” Poffins). Since PoffinType encapsulates categories, we could add flags like AllowRich, AllowMild, etc., or a set of disallowed types. Also consider a MinSecondLevel filter if users want to ensure a strong secondary flavor. These additions close the gap with Python’s filters (e.g. the Python RemovePoffinsWith_NumberOfFlavors_LessThan corresponds to MinFlavors in criteria).

Compile New Predicate Rules: Update PoffinCriteriaCompiler.CompilePredicate to enforce the new filters. For example: if MinFlavors is set, include a check that (count of p’s flavors > 0) >= MinFlavors; if MaxFlavors is set, check count <= MaxFlavors. If filtering by PoffinType (name), e.g. exclude Mild, add if (p.Type == PoffinType.Mild) return false when the user opts to remove mild Poffins. These conditions should short-circuit just like the existing ones.

Expand SortField Options: Add entries to the SortField enum for SecondLevel, NumFlavors, and any other metric we want to sort by (e.g. perhaps Name if sorting alphabetically by Poffin name, or Ratio for level-to-smoothness ratio). The Python list includes these fields, so extending our enum makes them accessible.

Dynamic Comparer Enhancement: Modify DynamicPoffinComparer.CompareField to handle the new sort fields. For SecondLevel and NumFlavors, the comparison is straightforward (compute a.SecondLevel vs b.SecondLevel, etc.). For sorting by Poffin “Name”, we might derive an ordering using PoffinType and perhaps primary flavor as a tiebreaker – for instance, sort by Type enum ordinal or a predefined string if needed. For ratio sorts (Level/Smoothness), compute the float ratio on the fly; note that we should be consistent with Python’s interpretation (they define level_to_smoothness_ratio as level/smoothness and ...\_sum as (level+second)/smoothness). We can compute these in comparison (though doing floating-point compare inside CompareField is acceptable given the small data set of top Poffins). Document that ratio sorts are available for expert use.

Secondary/Tertiary Sorts: The current design supports primary and one secondary sort. Python’s system allows chaining multiple sort criteria arbitrarily. To mirror this flexibility, we could extend PoffinCriteria with a list of sort fields or allow specifying more than two sorts (e.g. tertiary). A simpler approach is to permit compound sorts as needed (the two-level sort often suffices). For now, implement the additional SortFields and secondary sort; optionally note in documentation that more complex ordering can be achieved by custom comparer implementations if needed.

Design Note: Ensure that default sorting remains by Level descending (primary) and Smoothness ascending (secondary) – this matches typical “best Poffin” criteria (highest level, tie-break by lowest smoothness) and is essentially what LevelThenSmoothnessComparer did. After adding new sort fields, verify that the CLI parsing (e.g. --sort spicy:desc --then smoothness:asc) correctly maps to the extended enum. Add unit tests for the new predicate filters and comparers to avoid regression in the top-K selection logic.

4. Poffin Search Execution (BDSP.Core.Runner)

Integrate New Criteria in SearchRunner: Pass the compiled predicate and comparer from criteria into PoffinSearchRunner.Run (the CLI already does this). After expanding criteria, ensure all filters (like exclude foul, min stats) are being honored during the search. The predicate is applied immediately after cooking each Poffin – this is good for performance (skips adding filtered-out Poffins to selectors). One improvement is to apply simple filters even earlier: e.g., if MaxSmoothness is set, we can check the sum of berry smoothness values before cooking (an upper bound for that combo’s smoothness) and skip cooking if it exceeds the max. Such pre-checks (for smoothness or even for min flavor using a quick sum of berry flavors) can prune invalid combos before expensive cooking.

Top-K Selection & Merging: The core search logic uses TopKPoffinSelector per thread and merges results under a lock. We should ensure that after fixing combination generation, the number of combos might increase (especially if duplicates allowed), so keep an eye on performance. Consider optimizing the merge: e.g., gather all thread-local top-K lists and perform a k-way merge sort to produce the global top K, instead of locking for each local result. Given K is small (default 50) and thread count not huge, the current approach is acceptable, but a single merge outside the parallel loop could reduce locking overhead.

Determinism and Reproducibility: After refactoring, run multiple search passes to confirm the results are stable (the same best Poffins in the same order). The search should be deterministic as long as combination generation order and merging are fixed. The core currently uses a deterministic foul output (Spicy/Dry/Sweet = 2) and truncates time scaling for speed; keep that documented. Document that allowing duplicates will include foul Poffins (unless excluded) – which might flood the top-K if not handled. We might want to always exclude foul in ranking (since they are usually very low level), or ensure the predicate defaults to exclude them unless explicitly disabled (which is already the case).

Design Note: The search runner currently only uses up to 4-berry recipes (Gen4 Poffins). If in the future BerriesPerPoffin is changed (the criteria allows 1–4), the combination generator and cooker already handle those cases. We should test edge cases like 1-berry Poffins and 4-berry Poffins to ensure no off-by-one errors in loops or partition logic. Add tests for duplicates (e.g. 2 of the same berry + others) to ensure they produce foul Poffin outputs as expected and respect filtering.

5. Feeding Optimization (BDSP.Core.Feeding)

Validate ContestStats Calculation: Ensure ContestStatsCalculator.FromPoffin correctly converts a Poffin’s flavors to contest stat gains (currently it takes each flavor value directly, with no contribution from 0 or negative flavors). This aligns with BDSP rules (flavor points translate one-to-one to contest stats). No changes needed unless the formula is incomplete (e.g., if multiple Poffins interact in a nonlinear way, which they do not – stats just sum until capped at 255).

Use Full Sheen Capacity: The FeedingOptimizer.Optimize algorithm already navigates combinations of candidate Poffins using DFS and prunes dominated states. One thing to double-check is that it explores using all available sheen (255). It stops either when the stack is exhausted or MaxNodes reached. We should verify that the default MaxNodes (100k) is sufficient for exploring most combinations of top Poffins – it likely is, given the branching factor is limited by sheen constraints. If needed, expose MaxNodes or search depth as a tunable parameter in FeedingOptions.

Extend Feeding Options (Optional): To mirror Python’s flexibility in evaluating feeding plans, consider adding filters/sorting for contest stats results. For example, a user might want only plans that max out all five stats or plans using ≤ N Poffins. We could introduce a ContestStatsCriteria analogous to PoffinCriteria. In the feeding phase, after obtaining the Pareto-optimal set of feeding states (bestNodes list in the optimizer), apply such criteria to filter out undesired plans. We can then either return the single best plan among those or even return a list of top plans. (This would parallel Python’s ContestStatsFactory.filtered_sorted_contest_stats which returns a list of ContestStats meeting the filters.)

Output Multiple Plans (Optional): The current PoffinFeedingSearchRunner returns only the single optimal FeedingPlan. For greater flexibility, we could create a variant that returns multiple plans (e.g. all non-dominated plans or the top K by score). This would allow users to see alternatives (for instance, a plan that nearly maxes all stats with fewer Poffins vs. one that exactly maxes 3 stats but uses all 10 Poffins). Implementing this would involve modifying FeedingOptimizer to output the full bestNodes list (perhaps sorted by the Score function) instead of just building one best plan. We would then wrap those in FeedingPlan objects and allow filtering/sorting similar to Python’s approach (e.g. sort by total stat sum, then by Poffins used, etc. which Python does via sort interfaces).

Optimize Pruning Strategy: The dominance pruning in FeedingOptimizer can be further optimized by keeping the list of bestNodes sorted by sheen or by a heuristic. Currently, each new node is checked against all existing best nodes. Given the relatively small state space, this is fine, but we could improve it by maintaining separate lists keyed by sheen or partial stat sums to avoid unnecessary comparisons. Another micro-optimization: stop exploring a branch early if even adding all remaining Poffins cannot surpass the current best score (requires a bound on max attainable stats from remaining sheen). This is complex to calculate accurately but could use an optimistic assumption (e.g., each remaining sheen point yields a stat point in the category we care most about). Such bounds can prune branches in advance.

Design Note: The feeding system in C# is geared toward the “optimize everything” use-case. If we add user criteria for specific contest categories (as in presets like Cool/Beauty, etc.), that is handled by adjusting the Score function (e.g. a preset might weight one stat higher). This is different from Python’s approach of sorting the ContestStats list by a specific stat. Our approach is to incorporate the user’s objective into the optimization scoring itself (via FeedingOptions.Score). We should document this clearly: to maximize a specific stat, the user can supply a Score function that returns (e.g.) just that stat value, and the optimizer will find the plan that maximizes it. This gives flexibility without needing separate sort pipelines for contest stats.

6. Testing & Performance Considerations

Feature Parity Verification: After implementing the above, cross-verify against the Python bdsp-poffin-factory outputs for a few scenarios. For example, generate top Poffins with certain filters (min flavors, exclude mild, etc.) in both systems and compare results. They should match in content and ordering. Also test feeding outcomes: if the Python can list the best contest stats for feeding X Poffins, ensure the C# optimal plan matches the best entry.

Unit Tests for New Features: Add tests for duplicate berry combinations (expect foul Poffin outputs), for new filter criteria (e.g. MinFlavors = 2 should filter out single-flavor Poffins), and new sort orders (e.g. sorting by Smoothness ascending, by NumFlavors descending, etc., comparing that the list comes out in the intended order). Testing edge cases like all berries allowed vs. a restricted subset will ensure AllowedBerries logic is correct.

Benchmarking: Use the BDSP.Core.Benchmarks project to measure performance before and after changes. Key things to watch: combination explosion when allowing duplicates (even though many will be foul and filtered, cooking them incurs cost). If performance suffers, consider pruning duplicate combos early or adding a limit on duplicates (e.g. at most 2 of the same berry if we know >2 always yields foul anyway in BDSP – which seems to be the case that any duplicate triggers foul, so 2 vs 3 vs 4 duplicates all are just “foul” outcomes). Also benchmark the feeding optimizer with a larger candidate list to ensure the dominance pruning scales. Optimize data structures if needed (e.g. use a set or bit mask for dominated-state checks).

Refactor for Clarity: As the core logic becomes more complex, ensure code readability and maintainability. For instance, consider refactoring the search runners to use smaller helper methods (the parallel loop could be extracted for clarity). Document the expected behavior of each major step (cooking, filtering, selecting top K, feeding optimization) in the code comments, as done in Python. This will help future contributors understand the intent (the included NOTES.md already provides an architectural overview which we can update to reflect the new changes).

Modules to Extend/Refactor Summary

BerryCombinations – add support for combination with replacement (duplicate berries) and possibly a unified interface for both modes.

PoffinSearchRunner – refactor parallelization strategy to avoid missing cross-partition combos; integrate new filtering logic prior to cooking (simple skips); ensure it uses the updated criteria (AllowedBerries, etc.).

PoffinCooker – complete the recipe-to-Poffin formula, enforce input guards, and mirror naming/category rules from Python. Make sure PoffinType assignment (Foul, Mild, etc.) matches the conditions in the Python implementation.

Poffin (struct) – possibly enrich it with computed metadata (flavor count, maybe a precomputed Name string if needed for sorting by name, although generating the name on the fly from PoffinType and flavors is trivial).

PoffinCriteria & PoffinCriteriaCompiler – extend with new filter fields (min/max flavors, etc.) and incorporate those into the predicate logic.

SortField enum & DynamicPoffinComparer – add new sort keys (SecondLevel, NumFlavors, etc.) and implement their comparisons. Double-check the comparer’s logic for Smoothness (currently inverts compare to make lower smoothness rank higher by default) and apply similar reasoning for any new fields (e.g. for NumFlavors, higher is usually better; for Name, alphabetical ascending makes sense).

FeedingOptimizer – largely complete; just ensure it’s using ContestStats.Dominates correctly and consider exposing more results or criteria. No fundamental refactor needed, but could add an option to retrieve all Pareto-optimal plans instead of just one optimal.

ContestStats / FeedingPlan – if we provide multiple plans, we might create a new container (e.g. FeedingResults containing a list of plans). Also, implement any remaining TODOs in ContestStats (e.g., a method to compute PerfectCount (number of 255 stats) and perhaps a computed “rank” as the Python uses for filtering). These can be done after the core logic is solid, as polish for feature parity.

CoreGuards and Validation – fix the project file or filename so that CoreGuards (preconditions) are included in compilation and use them to assert invariants (e.g. non-negative flavors, sheen not exceeded 255 in a plan, etc.). While not directly a feature, this improves robustness.

Each of the above changes will bring BDSP.Core’s capabilities in line with the Python bdsp-poffin-factory, with potential improvements in performance and extensibility. By the end, the C# core should be able to generate all possible Poffins (with or without duplicates), filter and sort them on any criteria the Python version supports, and find optimal feeding combinations with equal flexibility. The result will be a more powerful and reliable Poffin optimization library, ready for integration into the UI and other layers.

Sources:

BDSP.Core code audit notes highlighting current issues

BDSP.Core implementation of combination generation and filtering

bdsp-poffin-factory (Python) filtering and sorting rules for reference

BDSP.Core tests confirming foul Poffin conditions (duplicate berries) and flavor non-negativity.

####Findings

Medium: BerryQuery’s Compare recomputes derived fields (MainFlavor, WeakenedMainFlavor, etc.) for every comparison in insertion sort; even with N=65, repeated calls can add up if you call this many times per second. Caching per‑berry derived values would help. BerryQuery.cs
Low: GetWeakenedMainFlavorValue can return negative values for berries with weak profiles; if you later use this in filters/sorts as a “quality” metric, be sure callers expect negative values. BerryFacts.cs
Optimization Ideas (if you expect millions of combos)

Precompute Berry metrics once: build arrays for MainFlavorValue, WeakenedMainFlavorValue, NumFlavors, etc., indexed by BerryId. Then PassesFilters and sorting can be key‑lookups rather than recomputation. This would turn the sort comparator into mostly integer comparisons. (BerryQuery.cs, BerryFacts.cs)
Keyed sort: For each BerryId, compute a packed sort key (e.g., 64‑bit) per sort spec and sort on keys, not by calling CompareField repeatedly. This removes repeated BerryTable.Get and BerryFacts calls. (BerryQuery.cs)
Avoid name comparisons in hot paths: If name sorting is needed, precompute NameOrder (array of BerryId sorted by name) and intersect it with filtered results (stable merge). This is O(n) vs. O(n²) comparisons for small n but avoids string compares entirely. (BerryFacts.cs, BerryQuery.cs)
Questions / Assumptions

Are these filters/sorts called per search run (once) or per combo? If per combo, we should push filtering earlier and cache everything; if once per run, current approach is likely fine.
If you want, I can implement a cached‑metrics path and a keyed sort that keeps the API unchanged while reducing per‑call CPU.
