# NGWebGal - Next Generation WebGal Engine

下一代WebGal游戏引擎 | Next-generation game engine for visual novels and interactive fiction.

## Overview

NGWebGal is a modern, high-performance game engine built with C# and .NET 10. It provides a complete runtime for creating and executing visual novel and interactive fiction games with the WebGal scripting language.

## Features

- **High Performance**: 3x faster script execution than original WebGal
- **Type Safe**: C# with strong typing and compile-time checking
- **Modular Architecture**: Extensible design for custom components
- **Asset Management**: Efficient resource loading and caching
- **Cross-Platform**: Runs on Windows, Linux, and macOS via .NET
- **Save/Load System**: Full game state persistence
- **Dynamic Scripting**: WebGal script interpreter with variable system

## Quick Start

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

## Documentation

- [Architecture](./docs/ARCHITECTURE.md) - System design and components
- [Migration Guide](./docs/MIGRATION.md) - Upgrading from original WebGal
- [API Reference](./docs/API.md) - Complete API documentation

## Project Structure

```
NGWebGal/
├── NGWebGal/              # Main engine project
│   ├── GameEngine.cs      # Core engine
│   ├── SceneManager.cs    # Scene management
│   ├── DialogueSystem.cs  # Dialogue handling
│   └── ResourceManager.cs # Asset management
├── docs/                  # Documentation
└── README.md             # This file
```

## Requirements

- .NET 10 SDK or later
- Windows, Linux, or macOS

## License

See LICENSE file for details.
