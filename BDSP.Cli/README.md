BDSP.Cli

Intent:
- Command-line entry point for running poffin searches.
- Parses arguments, builds criteria, and prints results.
- Useful for quick validation and batch runs.

Usage examples:
```powershell
# Top 50, 4 berries, require level >= 50 and smoothness <= 20.
dotnet run --project BDSP.Cli -- --berries=4 --topk=50 --min-level=50 --max-smooth=20

# Focus on Spicy; exclude foul poffins (default).
dotnet run --project BDSP.Cli -- --min-spicy=30 --sort=level:desc --then=smoothness:asc
```
