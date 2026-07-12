# ESO Log Filter

A desktop application for filtering Elder Scrolls Online (ESO) combat log files by unit type.

ESO combat logs can grow very large and contain entries for many different unit types. This tool lets you select a source log file, choose which unit types to keep (Players, Monsters, Objects, Siege Weapons, etc.), and write the filtered output to a new file.

## Features

- Filter ESO combat logs by unit type:
  - Players
  - Monster - Hostile
  - Monster - NPC Ally (Pet)
  - Monster - NPC Enemy (Pet) — enemy players' pets, distinguished from NPCs by their owner
  - Monster - Friendly
  - Monster - Neutral
  - Object
  - Siege Weapon
- Optionally also filter combat/effect events of hidden units (experimental) — see [docs/filtering.md](docs/filtering.md)
- Preview & compare: summarize the unfiltered and filtered log side by side (file size, lines, units per category, record types, fights) before uploading — see [docs/preview.md](docs/preview.md)
- Simple file picker for source and target files
- Async processing with cancel support

## Documentation

- [How the filter works](docs/filtering.md)
- [Preview & compare](docs/preview.md)
- [Architecture](docs/architecture.md)
- [Design decisions](docs/design-decisions.md)
- [Development and releases](docs/development.md)
- [Roadmap / ideas](docs/roadmap.md)

## Installation

Download the latest release for your platform from the [Releases](../../releases) page.

### Windows

1. Download `EsoLogFilter-win-x64.zip`
2. Extract the zip to any folder
3. Run `EsoLogFilter.exe`

No .NET runtime installation required — the application is fully self-contained.

> Windows SmartScreen may show a warning on first launch since the binary is not code-signed. Click **More info** then **Run anyway**.

### macOS (Intel)

1. Download `EsoLogFilter-osx-x64.zip`
2. Extract the zip
3. Open Terminal, navigate to the extracted folder, and run:
   ```bash
   chmod +x EsoLogFilter
   ./EsoLogFilter
   ```

### macOS (Apple Silicon)

1. Download `EsoLogFilter-osx-arm64.zip`
2. Extract the zip
3. Open Terminal, navigate to the extracted folder, and run:
   ```bash
   chmod +x EsoLogFilter
   ./EsoLogFilter
   ```

> macOS Gatekeeper may block the application since it is not signed or notarized. To allow it, go to **System Settings > Privacy & Security** and click **Open Anyway** after the first blocked launch attempt.

## Disclaimer

This is an open source project provided **as-is**, without warranty of any kind. There is no official support. Use at your own risk.

If you encounter a bug, you are welcome to [open an issue](../../issues), but there is no guarantee of a response or fix.

## License

This project is open source. See the [LICENSE](LICENSE) file for details.
