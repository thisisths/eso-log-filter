# Development

## Prerequisites

- .NET 10 SDK (`dotnet --list-sdks` should show a `10.x` entry)

## Build and test

```bash
dotnet build EsoLogFilter.slnx
dotnet test src/tests/Tests.csproj
```

The tests are fast (synthetic log lines only) and cover the line parser
(`LogEntry`), the filter decisions (`FilterByUnitTypeService`), and `FileHandler`
end to end on small temp files.

## Run the GUI

```bash
dotnet run --project src/ui-avalonia/Ui.Avalonia.csproj
```

## Run the filter from the command line

The console harness drives the same `FileHandler` the GUI uses:

```bash
dotnet run --project src/ui-test-console/Ui.TestConsole.csproj -- <input.log> <output.log> [--filter-events] [--units=Player,MonsterNpcAlly,MonsterNpcEnemy]
```

- Default units are `Player,MonsterNpcAlly,MonsterNpcEnemy` (the GUI defaults).
- `--filter-events` enables the experimental event filtering
  (see [filtering.md](filtering.md)).
- Unit names are the `UnitTypes` enum values: `Player`, `MonsterHostile`,
  `MonsterNpcAlly`, `MonsterNpcEnemy`, `MonsterFriendly`, `MonsterNeutral`,
  `Object`, `SiegeWeapon`.

This is the easiest way to test changes against a real `Encounter.log`. Real logs
are hundreds of MB up to a few GB; a full run takes seconds. A useful regression
check after refactoring: filter the same input with the old and the new build and
compare file hashes — with unchanged settings the output must be identical.

## Release

Releases are built by `.github/workflows/release.yml`:

1. Update the version-worthy changes, merge to `main`.
2. Tag the commit: `git tag v1.2.3 && git push origin v1.2.3`.
3. The workflow publishes self-contained single-file builds for `win-x64`,
   `osx-x64`, and `osx-arm64` and attaches the zips to a GitHub Release with
   generated notes.

The workflow can also be started manually (workflow dispatch) with an explicit
version number; without a tag it produces build artifacts but no release.
