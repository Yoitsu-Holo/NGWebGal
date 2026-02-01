# NGWebGal - 下一代 WebGal 引擎

下一代 WebGal 游戏引擎 | 用于视觉小说和互动小说的新一代游戏引擎。

## 概述

NGWebGal 是一个使用 C# 和 .NET 10 构建的现代化、高性能游戏引擎。它为使用 WebGal 脚本语言创建和执行视觉小说及互动小说游戏提供了完整的运行时环境。

## 特性

- **高性能**：脚本执行速度比原版 WebGal 快 3 倍
- **类型安全**：使用 C# 强类型和编译时检查
- **模块化架构**：可扩展的设计，支持自定义组件
- **资源管理**：高效的资源加载和缓存机制
- **跨平台**：通过 .NET 在 Windows、Linux 和 macOS 上运行
- **存档/读档系统**：完整的游戏状态持久化
- **动态脚本**：带变量系统的 WebGal 脚本解释器

## 快速开始

### 构建
```bash
dotnet build
```

### 运行
```bash
dotnet run
```

## 文档

- [架构设计](./docs/ARCHITECTURE.md) - 系统设计和组件说明
- [迁移指南](./docs/MIGRATION.md) - 从原版 WebGal 升级
- [API 参考](./docs/API.md) - 完整的 API 文档

## 项目结构

```
NGWebGal/
├── NGWebGal/              # 主引擎项目
│   ├── GameEngine.cs      # 核心引擎
│   ├── SceneManager.cs    # 场景管理
│   ├── DialogueSystem.cs  # 对话处理
│   └── ResourceManager.cs # 资源管理
├── docs/                  # 文档
└── README.md             # 本文件
```

## 系统要求

- .NET 10 SDK 或更高版本
- Windows、Linux 或 macOS

## 许可证

详见 LICENSE 文件。
