# NuGet package upgrade assessment

_Mode: **quick assessment** — package API diffs only; no per-project source scan was run._

## Recommended versions

- **BenchmarkDotNet**: **0.15.8** (unified across 1 project(s)).
- **CommunityToolkit.Mvvm**: **8.4.2** (unified across 1 project(s)).
- **FluentValidation**: **12.1.1** (unified across 1 project(s)).
- **FluentValidation.DependencyInjectionExtensions**: **12.1.1** (unified across 1 project(s)).
- **Microsoft.Extensions.DependencyInjection**: **10.0.12** (unified across 2 project(s)).
- **Microsoft.Extensions.Hosting**: **10.0.12** (unified across 2 project(s)).
- **Microsoft.NET.Test.Sdk**: **18.10.1** (unified across 3 project(s)).
- **Microsoft.Testing.Extensions.CodeCoverage**: **18.11.2** (unified across 2 project(s)).
- **Microsoft.VisualStudio.DiagnosticsHub.BenchmarkDotNetDiagnosers**: **18.7.37220.1** (unified across 1 project(s)).
- **Moq**: **4.21.0** (unified across 2 project(s)).
- **Shouldly**: **4.3.0** (unified across 3 project(s)).
- **xunit.runner.console**: not referenced by any scoped project, or no supported version was found.
- **xunit.runner.visualstudio**: **4.0.0** (unified across 3 project(s)).
- **xunit.v3.assert**: **4.0.1** (unified across 3 project(s)).
- **xunit.v3.core**: **4.0.1** (unified across 3 project(s)).

## Public API changes

> **Types moved (namespace changed) are not removals.** A moved type keeps its name and members;
> the fix is a `using`-directive change, not a rewrite. Do not treat a moved type as deleted.

- **Microsoft.NET.Test.Sdk**: no source-breaking public API changes detected — see [`apidiff/Microsoft.NET.Test.Sdk.apidiff.md`](apidiff/Microsoft.NET.Test.Sdk.apidiff.md).
- **xunit.v3.core**: no source-breaking public API changes detected — see [`apidiff/xunit.v3.core.apidiff.md`](apidiff/xunit.v3.core.apidiff.md).
- **xunit.v3.assert**: no source-breaking public API changes detected — see [`apidiff/xunit.v3.assert.apidiff.md`](apidiff/xunit.v3.assert.apidiff.md).
- **Microsoft.Extensions.DependencyInjection**: no source-breaking public API changes detected — see [`apidiff/Microsoft.Extensions.DependencyInjection.apidiff.md`](apidiff/Microsoft.Extensions.DependencyInjection.apidiff.md).
- **Microsoft.Extensions.Hosting**: no source-breaking public API changes detected — see [`apidiff/Microsoft.Extensions.Hosting.apidiff.md`](apidiff/Microsoft.Extensions.Hosting.apidiff.md).
- **Microsoft.Testing.Extensions.CodeCoverage**: no source-breaking public API changes detected — see [`apidiff/Microsoft.Testing.Extensions.CodeCoverage.apidiff.md`](apidiff/Microsoft.Testing.Extensions.CodeCoverage.apidiff.md).
- **Moq**: no source-breaking public API changes detected — see [`apidiff/Moq.apidiff.md`](apidiff/Moq.apidiff.md).

## Breaking-change findings

- Version divergence findings (Pkg.0003): 0
- Requested-version-unsupported findings (Pkg.0002): 0

- Quick mode does not scan source, so there are no per-line `PkgApi` usage findings. Review the
  per-package API diffs above and rely on build errors during execution to pinpoint affected code.
- A full code scan can locate the exact source location of every breaking-change usage across the repo.
  It is opt-in and slower — re-run the assessment with `fullScan=true` only if the user requests it.

## Next steps

1. Proceed to planning to triage the API changes above and plan the code fixes (if any).

