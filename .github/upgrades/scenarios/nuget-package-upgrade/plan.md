# NuGet Package Upgrade Plan

## Overview

**Target**: Centrally managed NuGet packages and safe solution modernization.
**Scope**: 9-project .NET 10 solution using Central Package Management through `Directory.Packages.props`.

## Tasks

### 01-update-central-package-versions: Update central package versions

Update `Directory.Packages.props` with the assessment-selected latest stable compatible versions for packages that have updates available. Keep packages with no newer compatible version unchanged, and keep any package with commercial licensing concerns pinned to the latest non-commercial version in accordance with the saved user preference.

Affects Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Hosting, Microsoft.NET.Test.Sdk, Microsoft.Testing.Extensions.CodeCoverage, Moq, xunit.v3.assert, and xunit.v3.core. The assessment found no version divergence and no source-breaking public API diffs for these updates.

**Done when**: `Directory.Packages.props` reflects the selected package versions and restore succeeds without introducing package downgrade or compatibility warnings.

---

### 02-validate-package-upgrade: Validate package upgrade compatibility

Restore, build, and run the test suites after the package changes so any compiler, analyzer, or test-platform changes are surfaced. Because the assessment ran in quick mode, use build and test results as the concrete compatibility signal for call-site issues.

Affects all projects that consume the upgraded packages, especially test projects using xUnit v3, Microsoft.NET.Test.Sdk, Moq, and Microsoft.Testing.Extensions.CodeCoverage.

**Done when**: The solution restores and builds with zero errors and zero warnings, and the available test suites pass.

---

### 03-modernize-maintainability: Apply safe readability and maintainability refactoring

Review the solution for low-risk modernization opportunities that improve readability or maintainability without changing solver behavior or pursuing performance micro-optimizations. Keep changes aligned with the repository conventions: C# 14/.NET 10, global usings per project, partial files for large view models, and no magic strings for property names.

Limit refactoring to clear, behavior-preserving cleanup discovered during the review. Avoid broad rewrites, public API churn, or algorithmic changes unless required by validation.

**Done when**: Safe modernization changes are applied, behavior remains unchanged, and affected projects build cleanly with tests passing.

---

### 04-update-docs-and-pr-materials: Update documentation and PR materials

Update `CHANGELOG.md` and `docs/ROADMAP.md` for the shipped package maintenance and modernization work, following repository guidance. Prepare a filled PR summary that captures what changed, validation results, and any follow-up notes.

**Done when**: Documentation reflects the change, the branch is ready to push, and a complete PR description is available for review.
