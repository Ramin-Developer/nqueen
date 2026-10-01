## Summary

- Updated centrally managed NuGet packages in `Directory.Packages.props` to the latest compatible stable versions selected by assessment.
- Removed the unused `xunit.runner.console` central package version entry because no project references it.
- Updated `CHANGELOG.md` and `docs/ROADMAP.md` for the package maintenance and validation state.

## Package Changes

- `Microsoft.Extensions.DependencyInjection`: `10.0.11` → `10.0.12`
- `Microsoft.Extensions.Hosting`: `10.0.11` → `10.0.12`
- `Microsoft.NET.Test.Sdk`: `18.9.0` → `18.10.1`
- `Microsoft.Testing.Extensions.CodeCoverage`: `18.11.0` → `18.11.2`
- `Moq`: `4.20.72` → `4.21.0`
- `xunit.v3.core`: `4.0.0` → `4.0.1`
- `xunit.v3.assert`: `4.0.0` → `4.0.1`
- Removed unused central `xunit.runner.console` entry.

## Assessment

- Quick NuGet package assessment completed for `NQueen.slnx`.
- No version divergence found.
- No unsupported requested-version findings found.
- No source-breaking public API diffs found for the selected package updates.

## Validation

- [x] `dotnet restore D:\repos\Code\nqueen\NQueen.slnx`
- [x] `dotnet build D:\repos\Code\nqueen\NQueen.slnx --no-restore`
- [x] `dotnet test D:\repos\Code\nqueen\NQueen.slnx --no-build --verbosity minimal`

Latest full test result:

- Total: 702
- Passed: 702
- Failed: 0
- Skipped: 0

## Notes

- Changes are centralized through `Directory.Packages.props` per repository package-management convention.
- Commercial-package policy was honored: no commercial package was upgraded beyond a non-commercial version.
- No solver behavior, GUI behavior, or performance-sensitive code paths were changed.
