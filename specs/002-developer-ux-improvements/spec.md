# Feature Specification: Developer UX Improvements

**Feature Branch**: `002-developer-ux-improvements`

**Created**: 2026-07-15

**Status**: Draft

**Input**: User selected ideas:
1. **Undo (Double Backspace / Ctrl+Z)** to revert layout swaps.
2. **camelCase / CamelCase** boundary detection and suffix protection.
3. **Path and URL ignoring** (slashes, domains/protocols).
4. **Subtle notification overlay** when layout auto-switches.

---

## User Scenarios & Testing

### User Story 1 - Undo Incorrect Auto-Correction (Priority: P1)
As a developer, if the switcher auto-corrects a word incorrectly (e.g., a technical term not in the dictionary), I want to press `Backspace` immediately to revert the layout switch and restore the original text.

**Independent Test**:
Type a word that gets corrected, then press `Backspace` as the next keystroke and assert that the word reverts to its original typed letters and the keyboard layout reverts to the original layout.

**Acceptance Scenarios**:
1. **Given** active layout is Ukrainian, **When** I type `ЗСУ` (with a space at the end) and it gets corrected to `pys `, **When** I press `Backspace` immediately, **Then** the switcher deletes `pys`, types `ЗСУ`, and switches layout back to Ukrainian.
2. **Given** a correction occurred, **When** I press another key (like `a` or `Space`) and then press `Backspace`, **Then** the undo state is cleared and normal backspace deleting occurs.

---

### User Story 2 - camelCase Boundary Detection & Protection (Priority: P1)
As a developer, I want to type camelCase variables (e.g. `getDatabaseConnection`) naturally. If I start typing in the wrong layout (e.g. `пуеВфефифыу`), it should correct the first part (`пуе` -> `get`) at the case transition boundary, switch layout, and then protect the rest of the variable from further corrections.

**Independent Test**:
1. Type a word in Ukrainian layout with a case transition (e.g., `пуеВ` where `е` is lower and `В` is upper) and verify that the layout-switcher auto-corrects `пуе` to `get` and switches layout.
2. Verify that typing CamelCase suffixes in English layout does not trigger correction.

**Acceptance Scenarios**:
1. **Given** active layout is Ukrainian, **When** I type `пуеВ` (where `В` is uppercase), **Then** `пуе` is corrected to `get`, the layout switches to English, and `V` is typed.
2. **Given** active layout is English, **When** I type `getDatabaseConnection`, **Then** no part of the variable is corrected.

---

### User Story 3 - Ignore Paths and URL Slashes (Priority: P1)
As a developer, I want path strings and URLs (e.g. `src/components`, `C:\Users`, `https://google.com`) to be ignored by auto-correction in English layout.

**Independent Test**:
Verify that words adjacent to `/` or `\` in English layout are skipped.

**Acceptance Scenarios**:
1. **Given** active layout is English, **When** I type `src/components` or `C:\Users`, **Then** no auto-correction occurs.
2. **Given** active layout is English, **When** I type `https://github.com`, **Then** no auto-correction occurs.

---

### User Story 4 - Subtle Layout Switch Overlay (Priority: P2)
As a user, when the switcher performs an auto-correction, I want to see a subtle, non-intrusive notification overlay near the system tray showing the layout swap (e.g. "EN → UA") so that I am aware the layout was changed.

**Independent Test**:
Trigger a layout correction and verify that a topmost, non-focusable window appears near the bottom-right corner of the active monitor, displaying the layout transition, and fades out within 1-2 seconds.

**Acceptance Scenarios**:
1. **Given** a layout swap is triggered, **When** the replacement finishes, **Then** a borderless visual notification panel appears in the bottom-right of the active monitor, displaying the transition, and automatically fades out without stealing input focus.

---

## Functional Requirements

- **FR-001 (Undo):** The system MUST store the `undoState` (original word, corrected word length, and original layout) after every successful replacement.
- **FR-002 (Undo Trigger):** If the next intercepted keyboard key is `VK_BACK` and `undoState` is active, the system MUST:
  1. Swallow the backspace key press.
  2. Send backspaces to delete the corrected word.
  3. Send Unicode keystrokes to restore the original word.
  4. Restore the original keyboard layout.
  5. Clear the `undoState`.
- **FR-003 (Undo Invalidation):** Any key intercept other than `VK_BACK` (or mouse click/focus shift) MUST clear the `undoState`.
- **FR-004 (camelCase Boundary):** The system MUST treat a transition from a lowercase letter to an uppercase letter as a word boundary, triggering replacement checks for the accumulated buffer.
- **FR-005 (camelCase Suffix Protection):** In English layout, if a word is preceded by a case transition boundary, it MUST be ignored for auto-correction.
- **FR-006 (Path Slashes):** In English layout, if a word is preceded or followed by a slash (`/` or `\`), it MUST be ignored for auto-correction.
- **FR-007 (Notification Toast):** When a layout switch occurs, the system MUST display a borderless, semi-transparent, topmost, non-activating window at the bottom-right corner of the screen showing the layout swap direction (e.g., "EN → UA" or "UA → EN") that fades out after 1 second.

---

## Success Criteria

- **SC-001:** Pressing backspace immediately after an auto-correction successfully restores the original text and layout.
- **SC-002:** Typing `пуеВ` in Ukrainian layout corrects `пуе` to `get` and switches layout.
- **SC-003:** Slashes (`/`, `\`) protect words from auto-correction in English layout.
- **SC-004:** A modern, premium-looking borderless overlay is displayed on auto-switch and fades out correctly without disrupting active window focus.
