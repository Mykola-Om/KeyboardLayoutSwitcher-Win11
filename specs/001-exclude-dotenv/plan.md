# Implementation Plan: Exclude Technical Terms from Auto-Correction

**Branch**: `001-exclude-dotenv` | **Date**: 2026-07-15 | **Spec**: [spec.md](file:///D:/pet%20project/KeyboardLayoutSwitcher-Win11/specs/001-exclude-dotenv/spec.md)

**Input**: Feature specification from `specs/001-exclude-dotenv/spec.md`

## Summary
Implement technical terms exclusion from layout switching (dotenv, env variables, ALL_CAPS constants) using a layout-dependent heuristic check that preserves correct Ukrainian-to-English translations.

## Proposed Changes

### 1. Dictionaries
#### [MODIFY] [tech.txt](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/Dictionaries/tech.txt)
- Add `env` as a new entry.

### 2. Core Heuristics
#### [MODIFY] [KeyMapper.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyMapper.cs)
- Update `IsWrongLayout` and `CalculateIsWrongLayout` to accept `char boundaryChar` and `char lastBoundaryChar`.
- Implement rules:
  - If active layout is English:
    - If `lastBoundaryChar == '.'` (preceded by dot), return `false`.
    - If `boundaryChar == '_'` or `lastBoundaryChar == '_'` (adjacent to underscore), return `false`.
    - If the word is `ALL_CAPS` (only contains English uppercase letters and digits), return `false`.
    - If the word contains both letters and digits, return `false`.
  - If active layout is Ukrainian:
    - If the word contains both letters and digits, return `false`.
    - If the word is `ALL_CAPS` (only contains Ukrainian uppercase letters), and the length is 3 or less (e.g. `ФОП`, `ТОВ`, `ЗСУ`), return `false`.

### 3. Hook Pipeline
#### [MODIFY] [KeyboardHook.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyboardHook.cs)
- Add `private char lastBoundaryChar = '\0';` field.
- In keyboard callback:
  - If a key is a character, do not reset `lastBoundaryChar`.
  - If a key is processed as a boundary, call `TryReplaceCurrentWordAtBoundary` and if it returns `false`, update `lastBoundaryChar = ch`.
- In `TryReplaceCurrentWordAtBoundary`:
  - Pass `boundaryChar` and `lastBoundaryChar` to `KeyMapper.IsWrongLayout`.
  - Reset `lastBoundaryChar = '\0';` on triggered replacement.

### 4. Tests
#### [MODIFY] [KeyMapperTests.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/Tests/KeyMapperTests.cs)
- Add unit tests for each new exclusion rule.

## Verification Plan
Run the standalone console test suite:
```powershell
dotnet run --project Tests/KeyboardLayoutSwitcher.Tests.csproj
```
Verify all new and existing tests pass.
