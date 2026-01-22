using BDSP.Criteria;
using BDSP.Cli.Formatting;
using BDSP.Core.Poffins;
using BDSP.Core.Runner;
using BDSP.Core.Selection;
using System.Collections.Generic;
using System.IO;
using BDSP.Core.Berries.Data;

static (SortField field, SortDirection dir) ParseSort(string value)
{
    var parts = value.Split(':', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length != 2)
        throw new ArgumentException("Sort must be field:asc|desc");

    var field = Enum.Parse<SortField>(parts[0], ignoreCase: true);
    var dir = Enum.Parse<SortDirection>(parts[1], ignoreCase: true);

    return (field, dir);
}

static void ShowHelp()
{
    Console.WriteLine("BDSP.Cli usage:");
    Console.WriteLine("  --berries=1|2|3|4");
    Console.WriteLine("  --topk=N");
    Console.WriteLine("  --min-level=N --max-smooth=N");
    Console.WriteLine("  --min-spicy=N --min-dry=N --min-sweet=N --min-bitter=N --min-sour=N");
    Console.WriteLine("  --min-berry-rarity=N --max-berry-rarity=N");
    Console.WriteLine("  --sort=field:asc|desc --then=field:asc|desc");
    Console.WriteLine("  --allowed-berries=cheri,pecha,37");
    Console.WriteLine("  --allowed-berries-file=inventory.txt");
    Console.WriteLine("  --feed");
    Console.WriteLine("  --json");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run --project BDSP.Cli -- --min-level=50 --max-smooth=20");
    Console.WriteLine("  dotnet run --project BDSP.Cli -- --allowed-berries=cheri,pecha,37");
    Console.WriteLine("  dotnet run --project BDSP.Cli -- --allowed-berries-file=inventory.txt --min-spicy=30");
}

static string NormalizeBerryName(string name)
{
    var trimmed = name.Trim();
    if (trimmed.EndsWith("Berry", StringComparison.OrdinalIgnoreCase))
        trimmed = trimmed[..^"Berry".Length].Trim();
    return trimmed.ToLowerInvariant();
}

static Dictionary<string, ushort> BuildBerryNameMap()
{
    var map = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase);
    for (ushort i = 0; i < BerryTable.Count; i++)
    {
        var name = BerryNames.GetName(new BerryId(i));
        map[NormalizeBerryName(name)] = i;
    }
    return map;
}

static IReadOnlyList<BerryId> ParseAllowedBerriesTokens(IEnumerable<string> tokens)
{
    var map = BuildBerryNameMap();
    var list = new List<BerryId>();

    foreach (var token in tokens)
    {
        if (ushort.TryParse(token, out var id))
        {
            if (id >= BerryTable.Count)
                throw new ArgumentOutOfRangeException(nameof(tokens), $"Berry id must be between 0 and {BerryTable.Count - 1}.");
            list.Add(new BerryId(id));
            continue;
        }

        var key = NormalizeBerryName(token);
        if (!map.TryGetValue(key, out var nameId))
            throw new ArgumentException($"Unknown berry '{token}'. Use id 0-{BerryTable.Count - 1} or a name like Cheri.", nameof(tokens));

        list.Add(new BerryId(nameId));
    }

    return list;
}

static IReadOnlyList<BerryId> ParseAllowedBerries(string value)
{
    var tokens = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    return ParseAllowedBerriesTokens(tokens);
}

static IReadOnlyList<BerryId> ParseAllowedBerriesFromFile(string path)
{
    var text = File.ReadAllText(path);
    var tokens = text.Split(
        ['\n', '\r', '\t', ' ', ','],
        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    return ParseAllowedBerriesTokens(tokens);
}

static IReadOnlyList<BerryId> MergeAllowedBerries(
    IReadOnlyList<BerryId>? existing,
    IReadOnlyList<BerryId> additional)
{
    if (existing is null || existing.Count == 0)
        return additional;

    var list = new List<BerryId>(existing.Count + additional.Count);
    list.AddRange(existing);
    list.AddRange(additional);
    return list;
}


static PoffinCriteria ParseCriteria(string[] args)
{
    var c = new PoffinCriteria();

    foreach (var arg in args)
    {
        if (!arg.StartsWith("--"))
            continue;

        var parts = arg.Split('=', 2);
        var key = parts[0].ToLowerInvariant();
        var value = parts.Length == 2 ? parts[1] : null;

        switch (key)
        {
            case "--allowed-berries":
            case "--berries-allowed":
                c = c with { AllowedBerries = MergeAllowedBerries(c.AllowedBerries, ParseAllowedBerries(value!)) };
                break;

            case "--allowed-berries-file":
                c = c with { AllowedBerries = MergeAllowedBerries(c.AllowedBerries, ParseAllowedBerriesFromFile(value!)) };
                break;

            case "--min-berry-rarity":
                c = c with { MinBerryRarity = int.Parse(value!) };
                break;

            case "--max-berry-rarity":
                c = c with { MaxBerryRarity = int.Parse(value!) };
                break;

            case "--feed":
                break;

            case "--berries":
                c = c with { BerriesPerPoffin = int.Parse(value!) };
                break;

            case "--topk":
                c = c with { TopK = int.Parse(value!) };
                break;

            case "--exclude-foul":
                c = c with { ExcludeFoul = true };
                break;

            case "--allow-foul":
                c = c with { ExcludeFoul = false };
                break;

            case "--min-level":
                c = c with { MinLevel = byte.Parse(value!) };
                break;

            case "--max-smooth":
                c = c with { MaxSmoothness = byte.Parse(value!) };
                break;

            case "--min-spicy":
                c = c with { MinSpicy = byte.Parse(value!) };
                break;

            case "--min-dry":
                c = c with { MinDry = byte.Parse(value!) };
                break;

            case "--min-sweet":
                c = c with { MinSweet = byte.Parse(value!) };
                break;

            case "--min-bitter":
                c = c with { MinBitter = byte.Parse(value!) };
                break;

            case "--min-sour":
                c = c with { MinSour = byte.Parse(value!) };
                break;

            case "--sort":
                {
                    var (f, d) = ParseSort(value!);
                    c = c with { PrimarySort = f, PrimaryDirection = (BDSP.Criteria.SortDirection ) d };
                    break;
                }

            case "--then":
                {
                    var (f, d) = ParseSort(value!);
                    c = c with { SecondarySort = f, SecondaryDirection = (BDSP.Criteria.SortDirection) d };
                    break;
                }
        }
    }

    return c;
}

static PoffinCriteria ApplyPreset(string name)
{
    return name.ToLowerInvariant() switch
    {
        "cool" => PoffinPresets.Cool,
        "beauty" => PoffinPresets.Beauty,
        "cute" => PoffinPresets.Cute,
        "smart" => PoffinPresets.Smart,
        "tough" => PoffinPresets.Tough,
        _ => throw new ArgumentException("Unknown preset")
    };
}

if (args.Any(a => a is "--help" or "-h" or "/?"))
{
    ShowHelp();
    return;
}

var criteria = ParseCriteria(args);

var predicate = PoffinCriteriaCompiler.CompilePredicate(criteria);
var comparer = PoffinCriteriaCompiler.CompileComparer(criteria);
var pruning = PoffinCriteriaCompiler.CompilePruning(criteria);
var berryPool = PoffinCriteriaCompiler.CompileBerryPool(criteria);

bool runFeed = args.Any(a => a.Equals("--feed", StringComparison.OrdinalIgnoreCase));
if (runFeed)
{
    var plan = PoffinFeedingSearchRunner.RunWithRecipes(
        berryPool: berryPool,
        berriesPerPoffin: criteria.BerriesPerPoffin,
        topK: criteria.TopK,
        cookTimeSeconds: 40,
        errors: 0,
        amityBonus: 9,
        comparer: comparer,
        predicate: predicate,
        pruning: pruning);

    Console.WriteLine(ContestPlanFormatter.Format(plan));
    return;
}

var result = PoffinSearchRunner.Run(
    berryPool: berryPool,
    berriesPerPoffin: criteria.BerriesPerPoffin,
    topK: criteria.TopK,
    cookTimeSeconds: 40,
    errors: 0,
    amityBonus: 9,
    comparer: comparer,
    predicate: predicate,
    pruning: pruning
);



Console.WriteLine();
Console.WriteLine($"Top {result.TopPoffins.Length} Poffins");
Console.WriteLine("===================================");

int rank = 1;
foreach (var p in result.TopPoffins)
{
    Console.WriteLine(
        $"{rank++,3}. " +
        $"Lvl {p.Level,3} | " +
        $"Sm {p.Smoothness,2} | " +
        $"{p.Type,-10} | " +
        $"({p.Spicy},{p.Dry},{p.Sweet},{p.Bitter},{p.Sour})"
    );
}

if (args.Contains("--json"))
{
    var json = BDSP.Serialization.PoffinJsonWriter
        .ToJson(result.TopPoffins);

    File.WriteAllText("poffins.json", json);
    Console.WriteLine("Saved poffins.json");
}

Console.WriteLine($"Criteria: {criteria}");


criteria = args.Any(a => a.StartsWith("--preset="))
    ? ApplyPreset(args.First(a => a.StartsWith("--preset=")).Split('=')[1])
    : new PoffinCriteria();

Console.WriteLine($"Criteria: {criteria}");
