# Architecture

## Solution layout

The solution (`EsoLogFilter.slnx`) contains five projects under `src/`:

| Project | Type | Purpose |
|---|---|---|
| `core` | class library | Domain logic: log line model (`LogEntry`), enums, the filter decision service (`FilterByUnitTypeService`), the log summary service (`LogSummaryService` + `LogSummary` aggregate model), and the service interfaces. Has no I/O dependencies. |
| `infrastructure-file` | class library | File I/O: `FileHandler` streams the input log line by line, asks the core service what to keep, and writes the output file. `FileSummarizer` streams a log the same way and builds a `LogSummary` for the Preview tab. |
| `ui-avalonia` | WinExe (`EsoLogFilter`) | The desktop GUI (Avalonia, Fluent theme; follows the OS light/dark theme via `ThemeDictionaries` in `App.axaml` — custom colors must be defined for both variants there, never hardcoded in `MainWindow.axaml`). This is the application distributed via GitHub Releases. |
| `ui-test-console` | console exe | Developer harness to run the filter from the command line (see [development.md](development.md)). |
| `tests` | xunit | Unit tests for parsing and filter decisions plus end-to-end tests for `FileHandler` on small synthetic logs. |

The target framework (`net10.0`) is set once in `Directory.Build.props`.

## Dependency injection

All wiring uses `Microsoft.Extensions.DependencyInjection`:

- `core/ServiceCollectionExtensions.AddCore()` registers `IFilterByUnitTypeService` and `ILogSummaryService`.
- `infrastructure-file/ServiceCollectionExtensions.AddInfrastructureFile()` registers `IFileHandler` and `IFileSummarizer`.
- The Avalonia app builds the provider in `App.axaml.cs` and resolves `MainWindow`; the console harness does the same in its `Program.cs`.

## Data flow

```
input .log ──> FileHandler.FilterCore (streaming, line by line)
                 │  LogEntry(line)          parse: split on ',' and detect the record type
                 │  ShouldWrite(entry)      decide via FilterByUnitTypeService
                 ▼
              output .log (only kept lines, order preserved)
```

Design points:

- **Single streaming pass.** The filter never loads the file into memory; multi-GB logs work with constant memory. Only unit ids and dropped cast ids are held in `HashSet<string>`s.
- **Stateful within one run.** `FilterByUnitTypeService` remembers which unit ids were kept or dropped (decided at their `UNIT_ADDED` line) so later lines referencing those units get the same verdict. `Reset()` is called at the start of every run.
- **Fail-open for unknown input.** Record types, unit types, or reactions the parser does not recognize are passed through unchanged instead of aborting the run — a game update that adds new record types cannot break existing filtering.
- **Cancellation.** The async variant used by the GUI checks the `CancellationToken` per line, so Cancel reacts immediately even on huge files.
- **Progress.** The async variants accept an `IProgress<double>` (fraction 0–1, bytes read / file size, reported every 50k lines). The GUI maps it to real progress bars; Analyze shows the first file as 0–50 % and the second as 50–100 %.

The Preview tab uses a second, read-only flow (see [preview.md](preview.md)):

```
unfiltered .log ──> FileSummarizer.SummarizeCore ──> LogSummary ─┐
filtered .log   ──> FileSummarizer.SummarizeCore ──> LogSummary ─┴─> compared in the Preview tab
```

- **Read-only by construction.** The summarizer never touches the filter path, so
  the byte-identical filter guarantee cannot be affected by preview features.
- **Aggregates only.** `LogSummary` holds counters (lines per record type, units
  per category, fights), never lines — the same constant-memory discipline as the
  filter.

## Error handling

- `core/Exceptions/BusinessException` marks expected error conditions whose `Message` is written for the end user; the GUI and the console harness display it as-is, without technical details.
- `FileInUseException` (derived from it) is thrown by `FileHandler` when opening the source or target file fails with a sharing violation — typically the game still writing the encounter log. The message says which file is affected and, for the source, how to stop encounter logging.
- The source file is opened read-only but deliberately without shared write (`FileShare.Read`): filtering a log the game is still appending to would silently produce a truncated output, so failing fast with the message above is intended.
- Everything else is unexpected: the GUI shows a generic message and appends the full exception details to `error.log` next to the executable (temp folder as fallback) via `ui-avalonia/Helper/ErrorReporter`.

## Where to change what

- New record type that needs special handling: `core/Constants.cs`, `core/Model/Objects/LineTypes.cs`, `core/Model/LogEntry.cs` (`SetLineType`), and the `switch` in `infrastructure-file/Services/FileHandler.cs`.
- Filter semantics: `core/Services/FilterByUnitTypeService.cs` (covered by `src/tests`).
- Preview aggregates: `core/Model/Analysis/LogSummary.cs`, `core/Model/Analysis/FightSummary.cs` and `core/Services/LogSummaryService.cs` (covered by `src/tests`).
- GUI: `ui-avalonia/MainWindow.axaml(.cs)` — the Filter and Preview tabs; the preview tables bind preformatted row models from `ui-avalonia/Model/` (`SummaryRow`, `DamageRow`, `FightRow`).
