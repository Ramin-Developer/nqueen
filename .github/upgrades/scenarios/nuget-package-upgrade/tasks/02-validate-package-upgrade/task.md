# 02-validate-package-upgrade: Validate package upgrade compatibility

Restore, build, and run the test suites after the package changes so any compiler, analyzer, or test-platform changes are surfaced. Because the assessment ran in quick mode, use build and test results as the concrete compatibility signal for call-site issues.

Affects all projects that consume the upgraded packages, especially test projects using xUnit v3, Microsoft.NET.Test.Sdk, Moq, and Microsoft.Testing.Extensions.CodeCoverage.

**Done when**: The solution restores and builds with zero errors and zero warnings, and the available test suites pass.

## Research Findings

- Package updates were applied centrally in `Directory.Packages.props`; validation must cover the full solution because the package props file can affect every project.
- Quick assessment reported no breaking public API diffs or version divergence, so build and test results are the concrete compatibility signal.
- Test discovery identified `NQueen.TestShared`, `NQueen.UnitTests`, and `NQueen.ViewModelTests` as test projects; `NQueen.TestShared` is shared test infrastructure and may contain no runnable tests, but it is still included in solution-level validation.
- Build tool decision: use `dotnet restore`, `dotnet build`, and `dotnet test` for the SDK-style .NET 10 solution. The Visual Studio build tool endpoint was unavailable during task 1.
