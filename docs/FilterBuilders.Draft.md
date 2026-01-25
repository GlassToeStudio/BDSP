# Filter Builder Drafts

This is a draft for possible fluent builder APIs for berry and poffin filters.
No implementation yet; use this for discussion and review.

## Goals
- More intuitive than a long constructor call.
- Zero allocations (builder as struct).
- Keep existing constructor available for low-level use.

## BerryFilterOptions Builder (Draft)

```csharp
var filter = BerryFilterOptionsBuilder
    .Create()
    .MinSpicy(10)
    .MaxSpicy(30)
    .MinRarity(3)
    .MaxRarity(9)
    .RequireMainFlavor(Flavor.Spicy)
    .ExcludeFlavors(FlavorMask.Sour)
    .Build();
```

### Suggested surface
- `Create()`
- `MinSpicy(int value)` / `MaxSpicy(int value)`
- `MinDry(int value)` / `MaxDry(int value)`
- `MinSweet(int value)` / `MaxSweet(int value)`
- `MinBitter(int value)` / `MaxBitter(int value)`
- `MinSour(int value)` / `MaxSour(int value)`
- `MinSmoothness(int value)` / `MaxSmoothness(int value)`
- `MinRarity(int value)` / `MaxRarity(int value)`
- `MinMainFlavorValue(int value)` / `MaxMainFlavorValue(int value)`
- `MinSecondaryFlavorValue(int value)` / `MaxSecondaryFlavorValue(int value)`
- `MinNumFlavors(int value)` / `MaxNumFlavors(int value)`
- `RequireMainFlavor(Flavor flavor)`
- `RequireSecondaryFlavor(Flavor flavor)`
- `RequireFlavors(byte flavorMask)` / `ExcludeFlavors(byte flavorMask)`
- `Build() -> BerryFilterOptions`

## PoffinFilterOptions Builder (Draft)

```csharp
var filter = PoffinFilterOptionsBuilder
    .Create()
    .MinLevel(25)
    .MaxSmoothness(10)
    .RequireMainFlavor(Flavor.Dry)
    .Build();
```

### Suggested surface
- `Create()`
- `MinSpicy(int value)` / `MaxSpicy(int value)`
- `MinDry(int value)` / `MaxDry(int value)`
- `MinSweet(int value)` / `MaxSweet(int value)`
- `MinBitter(int value)` / `MaxBitter(int value)`
- `MinSour(int value)` / `MaxSour(int value)`
- `MinSmoothness(int value)` / `MaxSmoothness(int value)`
- `MinLevel(int value)` / `MaxLevel(int value)`
- `MinNumFlavors(int value)` / `MaxNumFlavors(int value)`
- `RequireMainFlavor(Flavor flavor)`
- `RequireSecondaryFlavor(Flavor flavor)`
- `Build() -> PoffinFilterOptions`

## Notes
- Builders can internally carry a `*FilterMask` and field values, then emit the final struct.
- This is a UX layer; performance should remain identical to direct construction.
