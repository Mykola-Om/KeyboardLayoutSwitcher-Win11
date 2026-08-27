# Tasks: Exclude Technical Terms from Auto-Correction

**Input**: Design documents from `specs/001-exclude-dotenv/`

## Tasks List

- [x] T001 [US3] Add `env` to technical dictionary [tech.txt](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/Dictionaries/tech.txt)
- [x] T002 [US1, US2] Implement `lastBoundaryChar` state tracking in [KeyboardHook.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyboardHook.cs)
- [x] T003 [US1, US2] Update `IsWrongLayout` and `CalculateIsWrongLayout` method signatures in [KeyMapper.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyMapper.cs) to accept boundary context
- [x] T004 [US1, US2, US3] Implement the layout-dependent exclusion rules (dotenv, ALL_CAPS, underscores, alphanumeric mixed, Ukrainian abbreviations) in `CalculateIsWrongLayout` in [KeyMapper.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyMapper.cs)
- [x] T005 [US1, US2, US3] Implement test cases in [KeyMapperTests.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/Tests/KeyMapperTests.cs)
- [x] T006 [US1, US2, US3] Run and pass all tests
