# Walkthrough - Developer UX Improvements

We have completed the implementation of layout undo, camelCase support, path slash exclusions, and a visual layout switcher notification panel.

## Summary of Changes

- **Visual Notification**: Created [NotificationForm.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/NotificationForm.cs) and registered it in [KeyboardLayoutSwitcher.csproj](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyboardLayoutSwitcher.csproj). Exposed static `Instance` in [MainForm.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/MainForm.cs).
- **Undo Layout Changes**: Implemented `UndoState` in [KeyboardHook.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyboardHook.cs) and `QueueUndo` in [InputReplacer.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/InputReplacer.cs) to revert layout swaps on immediate Backspace (`VK_BACK`).
- **camelCase Support**: Intercepted case transitions in [KeyboardHook.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyboardHook.cs), added helper `GetLastChar` in [WordTracker.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/WordTracker.cs), and ignored suffixes in [KeyMapper.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyMapper.cs).
- **Paths / URLs**: Filtered out slashes `/` and `\` in [KeyMapper.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyMapper.cs) to prevent correcting file paths.
- **Tests**: Expanded [KeyMapperTests.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/Tests/KeyMapperTests.cs) with camelCase and path tests.

## Test Verification

Run:
```powershell
dotnet run --project Tests/KeyboardLayoutSwitcher.Tests.csproj
```
Result: **All test suites passed.**
