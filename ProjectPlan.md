# BDSP Poffin Factory (C#) - Capability and Performance Plan

## Goals
- New Core library from scratch: berries, poffins, cooking, contest stats, eating plans.
- Hard-coded berry table with compile-time data and precomputed derived fields.
- Fast filtering/sorting on any attribute for berries, poffins, and contest stats.
- Low allocations and low memory: struct data, precomputed tables, pooled buffers in hot paths.
- CLI/UI only after Core is complete and validated.

## Rules to Implement (from README.md)
- Flavor weakness cycle:
  - Spicy weakened by Dry, Dry by Sweet, Sweet by Bitter, Bitter by Sour, Sour by Spicy.
- Cooking formula for unique-berry recipes:
  - Sum flavor vectors across berries.
  - Apply weakness subtraction per flavor.
  - For each negative flavor, subtract 1 from all five flavors.
  - Apply time/error modifier: flavor = trunc( flavor * (60 / cookTimeSeconds) - (burns + spills) ).
  - Clamp negatives to 0 and cap to max flavor (Gen VIII cap = 100).
- Foul poffin:
  - Triggered by duplicate berries or if all flavors <= 0.
  - Deterministic foul flavors: Spicy/Dry/Sweet = 2, Bitter/Sour = 0.
  - Excluded from eating plans.
- Poffin level: strongest flavor value; max 100.
- Smoothness: floor(average berry smoothness) - number of berries - bonus reduction (BDSP cap 9).
- Two-flavor naming priority: Spicy > Dry > Sweet > Bitter > Sour.
- Eating plan:
  - Each poffin adds its flavor values to contest stats and smoothness to sheen.
  - Sheen starts at 0, caps at 255, and blocks further eating when maxed.
  - Final contest stats cap at 255.
  - Nature modifiers: liked flavor 1.1x, disliked flavor 0.9x (truncation).

## Performance Principles
- Prefer readonly structs and fixed-size storage for flavor vectors.
- Precompute and store:
  - weakened flavor vectors
  - main flavor and priority order
  - rarity and smoothness
  - sort keys for common queries
- Avoid allocations in inner loops; use `Span<int>` and pooled buffers where safe.
- Ensure filters compile into simple predicates over arrays.
- No LINQ in hot paths.

## Core Architecture (C#)
- `BDSP.Core`
  - Data: `BerryId` enum, `Berry` readonly struct, `BerryTable` static arrays.
  - Flavor math: `Flavor` enum, weaken-cycle helpers, vector utilities.
  - Cooking: `PoffinCooker`, `Poffin` readonly struct, `PoffinRecipe` as fixed-size value type.
  - Contest: `ContestStats` readonly struct and `EatingPlanRunner`.
  - Query: `BerryQuery`, `PoffinQuery`, `ContestStatsQuery` with compiled predicates.
  - Search: combination/permutation enumerators, top-K selection, pruning hooks.
- `BDSP.Core.Tests` with rule parity and invariants.
- `BDSP.Core.Benchmarks` for cooking, filtering, sorting, and search.

## Step-by-Step Plan (Small, Verifiable)

### Phase 0: Data and Golden Vectors
1) Build the berry table for BDSP.
   - Outputs: `BerryTable` static arrays for all 65 berries.
   - Tests: count, hash stability, and sample berries (Ganlon, Enigma).
2) Produce golden vectors aligned with README rules.
   - Outputs: JSON fixtures for cooking and eating-plan results.
   - Tests: README examples and edge cases (foul, cap, negative penalty).

### Phase 1: Berry Model and Tables
3) Implement `BerryId`, `Berry` struct, and `BerryTable`.
   - Compile-time arrays for flavor values, smoothness, rarity, main flavor, weakened values, and sort keys.
   - Tests: Ganlon values, weakened values, rarity, main flavor, smoothness.
4) Implement `BerryQuery` (filter + sort).
   - Filters: rarity range, smoothness range, main flavor, num flavors, weakened main flavor, any flavor value bounds.
   - Sorting: multi-key sort by any attribute.
   - Tests: count and ordering parity with reference cases.
   - Benchmarks: filter throughput vs list size with zero allocations in hot path.

### Phase 2: Cooking and Poffin Model
5) Implement `PoffinCooker` with README rules.
   - Steps: weaken, negative penalty, cook-time modifier with truncation, spills/burns, clamp, smoothness.
   - Tests: README examples and foul rules.
6) Implement `Poffin` struct and naming.
   - Fields: flavor vector, smoothness, level, second level, main flavor, rarity, num flavors.
   - Tests: name classification and two-flavor priority.

### Phase 3: Poffin Query and Sorting
7) Implement `PoffinQuery` with compiled predicates.
   - Filters: level/second level bounds, num flavors, rarity, any flavor min, max-N similar.
   - Sorting: multi-key sort by any attribute.
   - Tests: parity with sample sets and edge cases.
   - Benchmarks: filter+sort throughput.

### Phase 4: Contest Stats and Eating Plans
8) Implement `ContestStats` and `EatingPlanRunner`.
   - Feeding loop: stop at sheen >= 255, still apply final poffin boosts, cap at 255.
   - Include nature modifiers (1.1x/0.9x with truncation).
   - Tests: golden vectors and invariants (caps, sheen, ordering).

### Phase 5: Search and Enumeration
9) Implement combination/permutation enumerators for berries and poffins.
   - Tests: combination counts and deterministic ordering.
9.1) Precompute combo bases (2-4 berries) for high-volume cooking.
   - Outputs: `PoffinComboBase` + `PoffinComboTable`.
   - Tests: count, bounds, spot-checks vs on-the-fly sums.
9.2) Add subset combo enumerator for UI scenarios.
   - Outputs: `PoffinComboEnumerator`.
   - Tests: ordering and count for small inputs.
9.3) Add unified poffin search API (auto-selects full vs subset path).
   - Outputs: `PoffinSearch`, `PoffinSearchOptions`, `PoffinFilterOptions`, `TopK`.
   - Tests: route selection, filter application, top-K ordering.
10) Implement search helpers (top-K, pruning hooks).
   - Benchmarks: cooking throughput and search speed baseline.

### Phase 6: Performance Pass
11) Optimize hot paths.
   - Use `Span<int>`, fixed-size arrays, static tables, and pooled buffers.
   - Avoid allocations in loops; precompute weakened values and sort keys.
   - Benchmarks: compare before/after and set targets.

### Phase 7: CLI/UI (After Core)
12) CLI with query arguments and output formats.
13) UI after CLI validation.

## Test and Benchmark Strategy
- Unit tests: correctness of rules, edge cases, and golden fixtures.
- Property tests: invariants like non-negative values, caps, and ordering rules.
- Benchmarks: cooking 1-4 berry recipes, filter+sort, and search end-to-end.

## Open Questions to Resolve Before Coding
- Should cook time be modeled as seconds or 1/30 sec ticks for BDSP?
- Is smoothness bonus a fixed input parameter or always maxed at 9?
- Do we want optional Gen IV caps (99) and bonus (10), or BDSP-only?

## Progress Log
- 2026-01-24: Added `PoffinComboBase` and `PoffinComboTable` (precomputed 2-4 berry bases).
- 2026-01-24: Added `PoffinComboEnumerator` (non-alloc subset combo enumeration).
- 2026-01-24: Added `PoffinCooker` overload for `PoffinComboBase` and combo enumeration tests.
- 2026-01-24: Added `BDSP.Core.Benchmarks` with cooking benchmarks (combo base vs span).
- 2026-01-24: Added parallel cooking benchmarks for combo base and span.
- 2026-01-24: Added unified `PoffinSearch` API with filters and scoring options.
