# 01-upgrade-packages: Upgrade Microsoft.Extensions and BenchmarkDotNetDiagnosers packages

Update `Directory.Packages.props`: Microsoft.Extensions.DependencyInjection and Microsoft.Extensions.Hosting 10.0.13 → 10.0.14; Microsoft.VisualStudio.DiagnosticsHub.BenchmarkDotNetDiagnosers 18.7.37220.1 → 18.12.37802.2. API diffs (`apidiff/*.apidiff.md`) show no breaking changes.

**Done when**: Versions updated, solution restores and builds with no errors or warnings, all tests pass.

## Research
- CPM file: `Directory.Packages.props` (root), lines 12, 13, 15 confirmed at 10.0.13 / 10.0.13 / 18.7.37220.1.
- No per-project VersionOverride; unified bump only.
