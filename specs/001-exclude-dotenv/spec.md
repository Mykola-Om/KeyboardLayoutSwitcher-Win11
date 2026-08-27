# Feature Specification: Exclude Technical Terms from Auto-Correction

**Feature Branch**: `001-exclude-dotenv`

**Created**: 2026-07-15

**Status**: Draft

**Input**: User description: "Exclude .env files, env variables, and ALL_CAPS constants from being auto-corrected."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Exclude .env and Dot-Prefixed Configurations (Priority: P1)

As a developer typing in code editors or command lines, I want the switcher to ignore `.env` files (e.g., `.env`, `.env.local`) and other dot-prefixed configuration files (e.g., `.gitignore`, `.babelrc`) so that they are never auto-corrected to Ukrainian layout characters.

**Why this priority**: Highly critical because `.env` is a very common config file filename and its correction disrupts developer workflow.

**Independent Test**: Can be verified by typing `.env` in any layout and asserting that the switcher does not attempt to change the layout or rewrite the string.

**Acceptance Scenarios**:
1. **Given** the active layout is English, **When** I type `.env`, **Then** the switcher does not alter it.
2. **Given** the active layout is Ukrainian, **When** I type `.уні` (which corresponds to `.env` in Ukrainian layout), **Then** the switcher does not alter it.
3. **Given** the active layout is English, **When** I type `.gitignore`, **Then** the switcher does not alter it.

---

### User Story 2 - Exclude ALL_CAPS and Underscore-Based Environment Variables (Priority: P1)

As a developer, I want to use uppercase constants and environment variables containing underscores (e.g., `DATABASE_URL`, `PORT`, `API_KEY`) without them being auto-corrected.

**Why this priority**: Essential to avoid unwanted auto-correction when typing environment variables in terminal or code files.

**Independent Test**: Assert that words matching `^[A-Z0-9_]+$` (in English) or their Ukrainian keyboard layouts are skipped from layout auto-correction.

**Acceptance Scenarios**:
1. **Given** the active layout is English, **When** I type `DATABASE_URL`, **Then** the switcher does not alter it.
2. **Given** the active layout is Ukrainian, **When** I type `ВАТАВАСЕ_УКД` (which corresponds to `DATABASE_URL` in Ukrainian layout), **Then** the switcher does not alter it.

---

### User Story 3 - Add "env" and Common Technical Abbreviations to Dictionary (Priority: P2)

As a user, I want the short abbreviation `env` to be treated as a valid English word (like `api`, `app`, `json`) so that it is never auto-corrected.

**Why this priority**: Standardized abbreviation widely used in CLI commands and configuration paths.

**Independent Test**: Type `env` or its layout counterpart `утм` and verify no translation occurs.

**Acceptance Scenarios**:
1. **Given** the active layout is English, **When** I type `env`, **Then** the switcher does not alter it.
2. **Given** the active layout is Ukrainian, **When** I type `утм` (which corresponds to `env` in Ukrainian layout), **Then** the switcher does not alter it.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST treat the dot (`.`), underscore (`_`), and digits (`0-9`) as layout word characters in `IsLayoutWordCharacter` so that strings containing them (like `.env` or `API_KEY`) are accumulated as single words.
- **FR-002**: The system MUST detect and exclude strings starting with a dot followed by letters (e.g., `.env`, `.gitignore`) from being considered for auto-correction.
- **FR-003**: The system MUST detect and exclude uppercase letters with underscores/numbers (e.g. `DATABASE_URL`, `PORT`) from auto-correction.
- **FR-004**: The system MUST treat `env` as a valid technical word. This should be added to `tech.txt` or a common technical words list.
- **FR-005**: Any word containing mixed letters and digits (e.g., `v1`, `oauth2`) MUST be ignored by layout correction heuristics.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Typing `.env`, `.env.local`, `DATABASE_URL`, `PORT`, and `env` in either English or Ukrainian layout does not trigger layout correction or rewrite the word.
- **SC-002**: 100% of unit tests verifying these exclusions pass.

## Assumptions

- We assume that the user's settings or ignore list doesn't need to be manually updated by the user for these standard developer exclusions; they should be built-in defaults.
