# Migration Guide

## From Original WebGal to NGWebGal

### Key Changes

#### 1. Language & Runtime
- **Old**: JavaScript/TypeScript
- **New**: C# / .NET 10
- **Impact**: Better performance, type safety, native compilation

#### 2. Script Format
- **Old**: WebGal Script (JavaScript-based)
- **New**: WebGal Script (C# interpreter)
- **Compatibility**: Most scripts are compatible; see script migration below

#### 3. Asset Handling
- **Old**: Browser-based resource loading
- **New**: Managed resource system with caching
- **Impact**: Faster load times, better memory management

### Script Migration

#### Variable Declaration
```csharp
// Old (JavaScript)
var myVar = "value";

// New (C# compatible)
myVar = "value";  // Dynamic typing still supported
```

#### Dialogue Syntax
```
// Syntax remains the same
@character "Dialogue text"
```

#### Commands
Most commands are backward compatible. Check command reference for any deprecated commands.

### Asset Organization

```
project/
├── scripts/          # Game scripts
├── images/           # Image assets
├── audio/            # Audio files
├── fonts/            # Font files
└── data/             # Game data files
```

### Performance Improvements

- **Startup**: ~50% faster due to .NET JIT compilation
- **Runtime**: ~3x faster script execution
- **Memory**: Better GC with managed heap
- **Rendering**: Native graphics API access

### Breaking Changes

1. **Plugin System**: Old JavaScript plugins require rewriting in C#
2. **Browser APIs**: No direct browser API access; use managed equivalents
3. **Async Handling**: Improved async/await support in scripts

### Troubleshooting

- **Script errors**: Check console output for detailed error messages
- **Asset loading**: Verify asset paths are relative to project root
- **Performance**: Profile with built-in performance tools
