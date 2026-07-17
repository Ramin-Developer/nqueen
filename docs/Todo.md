### TODO: Configure line wrapping in Visual Studio 2026

**Goal**  
Ensure consistent, reviewer‑friendly line wrapping for prose in README and docs.

**Decision to make**  
Choose whether to enforce *soft wrap only* (editor preference) or *hard wrap* (repo‑level formatting). Recommend a `printWidth` of **80** (or **100** if you prefer fewer line breaks for long paths/badges).

**Tasks**
- Add guidance to `CONTRIBUTING.md` linking to this TODO.
- Recommend VS 2026 editor settings:
  - Tools → Options → Text Editor → All Languages → General → **Word wrap** = enabled
  - Optionally enable **Show visual glyphs for word wrap** and **Indent wrapped lines**
- If hard wrap is chosen:
  - Add a formatter config (e.g., `.prettierrc` with `proseWrap` and `printWidth`) to the repo.
  - Create branch `docs/readme-wrap`, run the formatter to reflow README and docs, open a PR for review.
- If soft wrap only:
  - Add a short note in `CONTRIBUTING.md` recommending developers enable word wrap in their editors.
- Add a short CI or pre-commit step later if the team wants to enforce formatting automatically.

**Acceptance criteria**
- `docs/TODO.md` exists and is linked from `CONTRIBUTING.md`.
- Team agrees on `printWidth` and wrap policy (soft vs hard).
- If hard wrap is chosen, a reviewed PR with reflowed docs is merged.
