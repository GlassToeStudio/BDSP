# BDSP.Core Review

## Findings
1. Build break: `BDSP.Core/Poffins/PoffinCooker.cs` still references `PoffinType.SingleFlavor` and `PoffinType.DualFlavor`, but those enum members were removed. This will not compile as-is.
2. Potential breaking change: `BDSP.Core/Poffins/PoffinType.cs` renames/reorders enum values. If `PoffinType` values are serialized, persisted, or compared across assemblies, this will change behavior/data compatibility. Consider keeping legacy names/values or adding explicit compatibility mapping.

## Other Observations
- `BDSP.Core/Poffins/PoffinType.cs` includes a stray comment (`// In Core/Poffins/PoffinType.cs`) and unused `using` directives; both can be removed to reduce noise.
- `BDSP.Core/Primitives/Flavor.cs` has unused `using` directives and no trailing newline (style-only).

## Optimization / Cleanup Ideas
- `BDSP.Core/Poffins/PoffinCooker.cs`: the flavor selection loop calls `GetFlavorValue` (switch) for each iteration. If this is hot, consider precomputing the five values into a small array/span and iterating to avoid repeated switch dispatch.
- Duplicate check in `BDSP.Core/Poffins/PoffinCooker.cs` is O(n^2) but fine for 1-4 berries; only revisit if you ever raise the max count.

## Potentially Unneeded Files
- `BDSP.Core/ProjectTemplate.txt` appears unused in code. If it is not part of any build/packaging step, consider moving it under a docs folder or removing it.
- `BDSP.Core/Poffins/Poffins.MD` is documentation. If it is not intended for packaging, consider relocating to a central `/docs` folder.

## Suggested Follow-up
- Update `BDSP.Core/Poffins/PoffinCooker.cs` to align with the new enum scheme (e.g., map 1–2 flavors to `PoffinType.Flavor`) or reintroduce `SingleFlavor`/`DualFlavor` for compatibility.
- Add/adjust tests around classification boundaries (level 49/50/95, errors 0/1, and flavor-count 1/2/3/4+).
