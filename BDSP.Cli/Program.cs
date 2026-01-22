using BDSP.Criteria;
using BDSP.Core.Berries.Data;
using BDSP.Core.Poffins;
using BDSP.Core.Runner;
using BDSP.Core.Selection;


static (SortField field, SortDirection dir) ParseSort(string value)
{
    var parts = value.Split(':', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length != 2)
        throw new ArgumentException("Sort must be field:asc|desc");

    var field = Enum.Parse<SortField>(parts[0], ignoreCase: true);
    var dir = Enum.Parse<SortDirection>(parts[1], ignoreCase: true);

    return (field, dir);
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

var criteria = ParseCriteria(args);

var predicate = PoffinCriteriaCompiler.CompilePredicate(criteria);
var comparer = PoffinCriteriaCompiler.CompileComparer(criteria);
var pruning = PoffinCriteriaCompiler.CompilePruning(criteria);

// Build berry pool (all berries for now)
var berryPool = new BerryId[BerryTable.Count];
for (ushort i = 0; i < BerryTable.Count; i++)
    berryPool[i] = new BerryId(i);

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
