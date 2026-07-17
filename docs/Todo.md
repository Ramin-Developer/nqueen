### TODO: Configure line wrapping in Visual Studio 2026

**Goal:** Ensure consistent line wrapping for prose in README and docs.

**Tasks:**
- Add this guidance to `CONTRIBUTING.md` and `docs/TODO.md`.
- Recommend developers enable Word Wrap in Visual Studio: Tools → Options → Text Editor → All Languages → General → Word wrap.
- Recommend enabling "Show visual glyphs for word wrap" and the indent option for wrapped lines (optional).
- Optionally add a formatting tool (Prettier or similar) to the repo to enforce prose wrapping in CI.
- Create a short PR that updates README formatting after the team agrees on `printWidth` (e.g., 80 or 100).
