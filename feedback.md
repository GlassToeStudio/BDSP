# Berry Library Review (Performance + Clean Code)

## 1) Should `Berry` contain a `BerryBase` field?
**Recommendation: No (keep flat fields in `Berry`, keep `BerryBase` separate).**

**Reasons:**
- `BerryBase` is already a minimal struct for cooking (5 flavors + smoothness). Embedding it in `Berry` would duplicate those 6 bytes anyway.
- Embedding would add an extra field and indirection with no lookup benefit; the data already sits in `Berry`.
- Keeping `Berry` flat avoids extra struct nesting (better for JIT inlining and memory layout).
- `BerryBase` as a separate table makes it explicit which data is needed for cooking; avoids accidental use of extra fields in hot paths.

**Alternative (if you want a single canonical source):**
- Use a `BerryBase[]` table and a `BerryMeta[]` table (rarity, main/secondary, name index, etc.) and combine by `BerryId`.
- This improves data locality for cooking (use only the base table), at the cost of more indirect lookups for metadata.

## 2) Duplicate `GetFlavor` in `Berry` and `BerryBase`
**Recommendation: Keep both, but make them trivially inlined.**

**Reasons:**
- Method duplication here is small and offers maximum inlining in each struct.
- Alternatives (extension methods or shared helper) usually add indirection or force boxing for interface usage; not worth it in hot paths.
- The current `switch` is optimal and likely inlines into calling code.

**Optional clean-up:**
- Add a small internal helper for shared bitmask creation if needed by both, but only if profiling shows repeated code size matters.

## 3) Should `Berry` store name / enum name?
**Recommendation: Keep name lookup via `BerryNames` and `BerryId`.**

**Reasons:**
- Names are not used in cooking or pruning; storing them in `Berry` adds memory and prevents struct from staying lean.
- `BerryId` + `BerryNames` is O(1), cache-friendly, and avoids per-berry string storage in the main table.
- String fields in a large array increase GC pressure and cache misses.

**If you want type safety without string storage:**
- Keep `BerryNames` and add `GetName(BerryId)` only. Use `BerryId` as identity everywhere.

## 4) Doc comments for all berry files
**Recommendation: Yes, add XML docs, but keep them concise.**

**Scope to cover:**
- `Flavor` enum: purpose and ordering priority.
- `BerryId`: identity and stability.
- `BerryBase`: used for cooking; minimal fields.
- `Berry`: full metadata; explain derived fields (main/secondary flavor, values, num flavors).
- `BerryTable`: canonical data, ID indexing, base table accessors.
- `BerryNames`: name table, indexed by `BerryId`.
- `BerryFilterOptions`, `BerryQuery`, `BerrySortKey/Field`, `BerrySorter`: user-facing behavior and examples.

**Why:**
- Improves maintainability and helps future optimization work.
- XML docs on public APIs are free at runtime and low cost in the codebase.

## 5) Exhaustive Unit Test List (Berry Library)
**Data integrity**
- `BerryTable.Count == 65`.
- `BerryTable.All.Length == 65`.
- `BerryNames` length == 65.
- `BerryId` values map to correct index (spot checks on first/last).

**Derived field correctness**
- `MainFlavor`, `MainFlavorValue` computed correctly for known berries.
- `SecondaryFlavor`, `SecondaryFlavorValue` computed correctly for known berries.
- `NumFlavors` equals count of non-zero flavors.
- `BerryTable` derived fields match recomputation (already covered in current test).

**Name mapping**
- `BerryNames.GetName(BerryId)` returns expected name for 3–5 spot checks (Aguav, Enigma, Yache).

**Filtering correctness**
- Each filter range bound works: min-only, max-only, min+max.
- Main flavor filter: only berries with that main flavor.
- Secondary flavor filter: only berries with that secondary flavor (if you add this).
- Flavor mask include/exclude: mask logic matches expected.
- Combined filters: intersection behaves correctly.

**Sorting correctness**
- Sort by each field individually (ascending and descending).
- Multi-key sort stable ordering across same primary key.
- Sorting by name uses ordinal ordering from `BerryNames`.

**Performance/allocations (optional)**
- `BerryQuery.Execute` filter phase does not allocate (verify with `GC.GetAllocatedBytesForCurrentThread`).
- Sorting allocation behavior documented (currently allocates; acceptable for now or replace with pooled version).

## Summary Actions (Optional)
- Keep `Berry` and `BerryBase` separate; no `BerryBase` field inside `Berry`.
- Keep duplicate `GetFlavor` for optimal inlining.
- Keep `BerryNames` lookup via `BerryId`, no name field in `Berry`.
- Add XML docs to all berry files (short and focused).
- Add tests for name mapping and filter/sort bounds; keep perf tests optional.
