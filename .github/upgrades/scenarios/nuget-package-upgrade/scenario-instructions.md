# NuGet Package Upgrade

## Strategy
Upgrade centrally managed packages through `Directory.Packages.props`, validate the solution, then apply only safe behavior-preserving maintainability refactoring.

## Preferences
- **Flow Mode**: Automatic
- **Commit Strategy**: After Each Task
- **Scope**: Solution (`NQueen.slnx`)
- **Package Version Policy**: Use latest stable compatible versions unless a package has commercial licensing concerns.
- **Commercial Package Policy**: Commercial packages must be pinned/fixed to the latest non-commercial version.
- **Package Management**: Apply package version changes in `Directory.Packages.props`.
- **Refactoring Scope**: Safe modernization/readability/maintainability improvements only; no performance micro-optimizations unless explicitly requested.

## Decisions
- Use quick package assessment results for planning because no version divergence, unsupported-version findings, or source-breaking public API diffs were found.
- Apply unified assessment-selected versions for all packages with newer compatible stable releases.

## Source Control
- **Source Branch**: main
- **Working Branch**: upgrade-nuget-modernize
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Package Scope
- BenchmarkDotNet
- CommunityToolkit.Mvvm
- FluentValidation
- FluentValidation.DependencyInjectionExtensions
- Shouldly
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting
- Microsoft.NET.Test.Sdk
- Microsoft.VisualStudio.DiagnosticsHub.BenchmarkDotNetDiagnosers
- Microsoft.Testing.Extensions.CodeCoverage
- Moq
- xunit.v3.core
- xunit.v3.assert
- xunit.runner.console
- xunit.runner.visualstudio

## Selected Versions
- **Microsoft.Extensions.DependencyInjection**: 10.0.12
- **Microsoft.Extensions.Hosting**: 10.0.12
- **Microsoft.NET.Test.Sdk**: 18.10.1
- **Microsoft.Testing.Extensions.CodeCoverage**: 18.11.2
- **Moq**: 4.21.0
- **xunit.v3.assert**: 4.0.1
- **xunit.v3.core**: 4.0.1

## Custom Instructions
<!-- Task-specific overrides: "For {taskId}: {instruction}" -->
