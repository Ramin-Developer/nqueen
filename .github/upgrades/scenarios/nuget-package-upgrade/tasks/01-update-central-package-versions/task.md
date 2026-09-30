# 01-update-central-package-versions: Update central package versions

Update `Directory.Packages.props` with the assessment-selected latest stable compatible versions for packages that have updates available. Keep packages with no newer compatible version unchanged, and keep any package with commercial licensing concerns pinned to the latest non-commercial version in accordance with the saved user preference.

Affects Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Hosting, Microsoft.NET.Test.Sdk, Microsoft.Testing.Extensions.CodeCoverage, Moq, xunit.v3.assert, and xunit.v3.core. The assessment found no version divergence and no source-breaking public API diffs for these updates.

**Done when**: `Directory.Packages.props` reflects the selected package versions and restore succeeds without introducing package downgrade or compatibility warnings.

## Research Findings

- CPM is enabled through the repository-root `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`). Project `PackageReference` items omit versions; package versions must be changed centrally.
- Verified CPM/package usage with `get_project_dependencies` for `NQueen.UnitTests`, `NQueen.GUI`, and `NQueen.Benchmarking`.
- Assessment-selected updates: `Microsoft.Extensions.DependencyInjection` 10.0.11 → 10.0.12, `Microsoft.Extensions.Hosting` 10.0.11 → 10.0.12, `Microsoft.NET.Test.Sdk` 18.9.0 → 18.10.1, `Microsoft.Testing.Extensions.CodeCoverage` 18.11.0 → 18.11.2, `Moq` 4.20.72 → 4.21.0, `xunit.v3.core` 4.0.0 → 4.0.1, `xunit.v3.assert` 4.0.0 → 4.0.1.
- No version divergence, unsupported-version findings, or source-breaking public API diffs were reported for these selected updates.
