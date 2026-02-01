# NGWebGal Architecture

## Overview

NGWebGal is a next-generation game engine for WebGal, built with C# and .NET 10. It provides a modular, extensible architecture for visual novel and interactive fiction games.

## Core Components

### 1. Game Engine (`NGWebGal.csproj`)
- **Purpose**: Main runtime engine for game execution
- **Key Classes**:
  - `GameEngine`: Core orchestrator managing game state and lifecycle
  - `SceneManager`: Handles scene loading, transitions, and state
  - `DialogueSystem`: Processes dialogue rendering and character interactions
  - `ResourceManager`: Manages asset loading and caching

### 2. Scripting System
- **Language**: WebGal Script (custom DSL)
- **Parser**: Tokenizes and parses game scripts
- **Executor**: Interprets and executes parsed commands
- **Variables**: Dynamic variable system with type coercion

### 3. Asset Management
- **Resource Types**: Images, audio, fonts, scripts
- **Caching**: In-memory cache with LRU eviction
- **Loading**: Async resource loading with progress tracking

### 4. State Management
- **Game State**: Persistent state across scenes
- **Save/Load**: Serialization of game progress
- **Variables**: Global and local variable scopes

## Design Patterns

- **Singleton**: GameEngine, ResourceManager
- **Observer**: Event system for game state changes
- **Factory**: Resource creation and caching
- **Strategy**: Pluggable rendering and audio backends

## Extension Points

- Custom command handlers
- Resource loaders for new asset types
- Rendering backends
- Audio system implementations

## Build & Deployment

- **Framework**: .NET 10
- **Output**: Standalone executable or library
- **Dependencies**: Minimal external dependencies for portability
