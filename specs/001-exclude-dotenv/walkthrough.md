# Walkthrough - Exclude Technical Terms from Auto-Correction

We have implemented the exclusions for technical terms in layout switching.

## Summary of Changes

- **Dictionaries**: Added `env` to [tech.txt](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/Dictionaries/tech.txt).
- **State tracking**: Implemented `lastBoundaryChar` in [KeyboardHook.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyboardHook.cs) to track dot-preceded and underscore-preceded words.
- **Rules logic**: Refactored `IsWrongLayout` and `CalculateIsWrongLayout` in [KeyMapper.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyMapper.cs) to process layout-dependent filters (dots, underscores, English `ALL_CAPS`, alphanumeric mixed, Ukrainian abbreviations like `ФОП`, `ТОВ`, `ЗСУ`).
- **Tests**: Expanded [KeyMapperTests.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/Tests/KeyMapperTests.cs) with 8 new test scenarios.

## Test Verification

Executed tests:
```powershell
dotnet run --project Tests/KeyboardLayoutSwitcher.Tests.csproj
```
Result: **All test suites passed.**
