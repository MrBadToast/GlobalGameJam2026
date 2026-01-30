# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Global Game Jam 2026 entry - a 2D top-down arena survival game built with Unity 6000.3.2f1. Players fight waves of enemies, collect gold, purchase upgrades in shop phases, and manage character expressions for stat bonuses.

## Build & Development

This is a Unity project - open in Unity Editor 6000.3.2f1 or later. No custom build scripts exist; use standard Unity build workflow (File > Build Settings).

**IDE Setup**: Visual Studio or Rider with Unity debugger configured. VSCode settings included with Unity debugger launch config.

## Architecture

### Singleton Pattern
The project uses a custom singleton pattern for managers:
- `StaticSerializedMonoBehaviour<T>` - Generic singleton base (Odin Inspector integration, NOT persistent across scenes)
- `SceneLoader` - Separate persistent singleton (DontDestroyOnLoad) for scene transitions with DOTween fade animations

New managers should inherit from `StaticSerializedMonoBehaviour<T>` following the pattern in `Player_Topdown.cs`.

### Scene Structure
1. **Title.unity** - Main menu with settings popup
2. **MainGame.unity** - Primary gameplay arena (30min battle phase + 5min shop phase cycle)
3. **Platformer.unity** - Legacy/alternate level

### Core Scripts (Assets/Scripts/)
- `Player_Topdown.cs` - Top-down movement controller with Rigidbody2D physics and animator integration
- `SceneLoader.cs` - Async scene loading with progress UI and fade transitions
- `StaticSerializedMonoBehaviour.cs` - Generic singleton base class
- `TitleBehaivor.cs` - Title screen UI controller

### Input System
Uses Unity's new Input System (com.unity.inputsystem). Actions defined in `Assets/Settings/InputSystem_Actions.inputactions`:
- Player ActionMap: Move, Look, Attack, Interact (hold), Crouch, Jump, Previous/Next

Code references generated class `InputSystem_Actions` for input reading.

## Key Dependencies

**Third-party plugins (Assets/Plugins/):**
- **Sirenix Odin Inspector** - Use `SerializedMonoBehaviour` instead of `MonoBehaviour` for complex serialization, leverage Odin attributes (`[Button]`, `[ShowInInspector]`, `[InfoBox]`, etc.)
- **Demigiant DOTween** - Animation tweening (see SceneLoader for usage pattern)
- **ChocDino UIFX** - UI effects framework (glow, blur)
- **ConsolePro** - In-game debug console

## Game Design Reference

See `DOCS.md` for full Korean game design document. Key mechanics:
- 4 character expressions with stat bonuses (+20% ATK, -20% DMG taken, +20% speed, +5% all)
- 4 weapon types matching 4 enemy types (weapon must match for damage)
- 4 enemy color tiers with escalating stats (+20%/+40%/+80% per tier)
- Shop phase with 8x3 item grid

## Code Conventions

- Physics in `FixedUpdate()`, input reading in `Update()`
- Use `[Header("Section")]` for inspector organization
- Use `[SerializeField]` for private fields exposed to inspector
- Mixed Korean/English comments acceptable (team is Korean)
