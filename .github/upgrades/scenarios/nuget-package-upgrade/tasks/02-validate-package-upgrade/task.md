# 02-validate-package-upgrade: Validate package upgrade compatibility

Restore, build, and run the test suites after the package changes so any compiler, analyzer, or test-platform changes are surfaced. Because the assessment ran in quick mode, use build and test results as the concrete compatibility signal for call-site issues.

Affects all projects that consume the upgraded packages, especially test projects using xUnit v3, Microsoft.NET.Test.Sdk, Moq, and Microsoft.Testing.Extensions.CodeCoverage.

**Done when**: The solution restores and builds with zero errors and zero warnings, and the available test suites pass.
