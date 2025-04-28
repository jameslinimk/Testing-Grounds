# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands
- Build project: Use Unity Editor (File > Build Settings > Build)
- Play in editor: Press Play button or Ctrl+P
- Tests: Use Unity Test Framework within the Unity Editor

## Coding Standards

### Naming & Formatting
- Indentation: 4 spaces (no tabs)
- Braces: K&R style (opening brace on the same line as declaration)
- Classes/Methods: PascalCase (PlayerController, UpdateMovement)
- Fields/Variables: camelCase (moveSpeed, inputVector)
- Use [Header] attributes to organize inspector fields
- Max line length: Keep reasonable (~100 characters)

### Code Organization
- Group related functionality with section comments:
```csharp
/* -------------------------------------------------------------------------- */
/*                                Input events                                */
/* -------------------------------------------------------------------------- */
```
- Place Unity lifecycle methods (Awake, Start, Update) together
- Use expression-bodied properties for simple getters

### Best Practices
- Prefer Unity's new Input System over legacy input
- Use [SerializeField] for inspector-visible private fields
- Implement proper cleanup in MonoBehaviour lifecycle
- Use Debug.LogWarning for non-fatal issues
- Follow singleton pattern with caution and explicit cleanup
- Organize related scripts in folder groups (Player Scripts, Gun Scripts, etc.)