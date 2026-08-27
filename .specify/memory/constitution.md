# KeyboardLayoutSwitcher-Win11 Constitution

## Core Principles

### I. Modularity & Testability
All core logic must be decoupled from Windows API hooks and OS-specific input/output. 
* Key mapper logic ([KeyMapper.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyMapper.cs)) and word tracking/state machine ([WordTracker.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/WordTracker.cs)) must remain pure, side-effect-free, and fully testable without launching the Windows message loop or registering hooks.
* Global hooks ([KeyboardHook.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/KeyboardHook.cs)) only orchestrate events between the OS and the core logical components.

### II. Native Integration & Modern Aesthetics
The application must blend seamlessly into the Windows 11 environment.
* Windows Forms controls must support native Dark Mode and follow Windows 11 dark design guidelines (colors: `#202020` for background, `#3c3c3c` for buttons/inputs, Segoe UI fonts, rounded layouts where possible).
* Enable native attributes like `DWMWA_USE_IMMERSIVE_DARK_MODE` and window themes using [Win32Interop.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/Win32Interop.cs).

### III. Centralized Win32 Interop
All low-level WinAPI P/Invoke signatures, structs, and constants must be centralized in [Win32Interop.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/Win32Interop.cs). 
* Do not duplicate Win32 declarations or magic constants across multiple files.
* Keep imports clean, document their purposes, and handle thread safety when registering/unregistering hooks.

### IV. Safety & Reliability
The application operates in the background as a keyboard layout switcher, which requires high reliability and low resource footprint.
* Prevent multiple running instances of the application using a global named Mutex (`Global\KeyboardLayoutSwitcher-Win11-instance`).
* Log tracing ([TraceLogger.cs](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/TraceLogger.cs)) must be safe, catch all output exceptions, and should be easily enabled/disabled from the settings.

### V. Test-First Verification
Every functional change, heuristic update (e.g., ignoring specific files or layouts), or settings logic modification must be accompanied by relevant unit tests in the [Tests/](file:///d:/pet%20project/KeyboardLayoutSwitcher-Win11/Tests) project.
* The test suite must be runnable standalone via `dotnet run --project Tests/` and target `.NET Framework 4.7.2`.

## Technology Stack & Constraints
* **Language/Framework:** C# / .NET Framework 4.7.2 (WinForms).
* **Target OS:** Windows 11 (with backward compatibility to Windows 10 for basic features).
* **Settings Store:** XML serialization (`settings.xml`) saved automatically to local Application Data.

## Governance
* This constitution governs all code modifications, refactorings, and feature additions in this repository.
* Changes to core heuristics must be verified against the test suite to prevent regressions in existing auto-switching behavior.

**Version**: 1.0.0 | **Ratified**: 2026-07-15 | **Last Amended**: 2026-07-15

