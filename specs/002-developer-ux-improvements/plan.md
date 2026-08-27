# Implementation Plan: Developer UX Improvements

**Branch**: `002-developer-ux-improvements` | **Date**: 2026-07-15 | **Spec**: [spec.md](file:///D:/pet%20project/KeyboardLayoutSwitcher-Win11/specs/002-developer-ux-improvements/spec.md)

**Input**: Feature specification from `specs/002-developer-ux-improvements/spec.md`

## Summary
Implement a set of developer-focused UX improvements: layout undo using Backspace, camelCase boundaries and protection, path and URL slash ignoring, and a transient visual layout notification panel.

## Proposed Changes

### 1. Visual Notification Overlay

#### [NEW] [NotificationForm.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/NotificationForm.cs)
- Implement a borderless, non-activating (`WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW`), topmost WinForms panel that displays the layout switch direction (e.g. "EN → UA").
- Positions itself at the bottom-right corner of the screen and fades out using a timer.

---

### 2. State & Boundary Core

#### [MODIFY] [KeyboardHook.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyboardHook.cs)
- **Undo implementation:**
  - Define `UndoState` nested class to hold original word, corrected length, and original layout.
  - Implement `currentUndoState` tracking.
  - Intercept `VK_BACK` when `currentUndoState != null`, swallow it, and call `ExecuteUndo()`.
- **camelCase boundary:**
  - In key intercept, check if `wordTracker` has lowercase last char and new character is uppercase. If so, trigger `TryReplaceCurrentWordAtBoundary` with a special code `\u0001`.
  - Re-evaluate the new character if replacement occurred.

#### [MODIFY] [InputReplacer.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/InputReplacer.cs)
- Skip sending boundary character if it matches `\u0001`.
- Implement `QueueUndo(int backspaceCount, string originalWord, bool targetLayoutIsEnglish)` to restore the original state.

---

### 3. Exclusions Heuristics

#### [MODIFY] [KeyMapper.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyMapper.cs)
- Add rules in `CalculateIsWrongLayout` for English layout:
  - Skip if `lastBoundaryChar == '\u0001'` (camelCase suffix protection).
  - Skip if `boundaryChar` or `lastBoundaryChar` is `/` or `\` (path slashes).

---

### 4. Tests

#### [MODIFY] [KeyMapperTests.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/Tests/KeyMapperTests.cs)
- Add unit tests validating:
  - camelCase suffix protection in English layout.
  - camelCase prefix correction in Ukrainian layout.
  - Path/URL slash exclusions.

## Verification Plan

### Automated Tests
Run:
```powershell
dotnet run --project Tests/KeyboardLayoutSwitcher.Tests.csproj
```

### Manual Verification
- Compile and run the app, type `ЗСУ` (gets corrected), press Backspace, and verify it reverts to `ЗСУ`.
- Type `пуеВфефифыу` in Ukrainian layout and verify it corrects `пуе` to `get` and types `V` (resulting in `getDatabase`).
- Verify visual switch indicator appears in bottom-right corner.
