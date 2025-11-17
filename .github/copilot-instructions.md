# Copilot Instructions for RG.CodeAnalyzer Repository

## Repository Overview

This repository contains **RG.CodeAnalyzer**, a Roslyn-based code analyzer package that provides custom C# diagnostics and code fixes. The analyzer enforces coding standards and best practices specific to this project's conventions.

## Project Structure

- **RG.CodeAnalyzer/RG.CodeAnalyzer/** - Main analyzer project (targets netstandard2.0)
  - `RGDiagnosticAnalyzer.cs` - Single file containing all analyzer implementations
  - `*CodeFixProvider.cs` - Individual code fix providers for each diagnostic
  
- **RG.CodeAnalyzer/RG.CodeAnalyzer.Test/** - Test project (targets net10.0)
  - Test files named `*UnitTests.cs` or `*Tests.cs`
  - Uses MSTest framework
  - Helper classes in `Verifiers/` and `Helpers/` directories

- **RG.Annotations/** - Companion attributes library
- **ProtoDummies/** - Protobuf-related test utilities
- **.github/workflows/dotnet.yml** - CI/CD pipeline

## Technology Stack

- **.NET SDK**: 10.0.x (as of latest update)
- **Target Framework**: netstandard2.0 (analyzer), net10.0 (tests)
- **Test Framework**: MSTest
- **Key Dependencies**:
  - Microsoft.CodeAnalysis.Analyzers: 3.11.0
  - Microsoft.CodeAnalysis.CSharp.Workspaces: 4.12.0
  - Microsoft.CodeAnalysis.NetAnalyzers: 10.0.100

## Analyzer Implementation Pattern

### Adding a New Analyzer

1. **Define Diagnostic ID and Descriptor** in `RGDiagnosticAnalyzer.cs`:
   ```csharp
   public const string YOUR_ANALYZER_ID = "RG00XX";
   
   private static readonly DiagnosticDescriptor YOUR_ANALYZER = new(
       id: YOUR_ANALYZER_ID,
       title: "Clear descriptive title",
       messageFormat: "Message with {0} placeholders",
       category: "Category", // e.g., "Naming", "Performance", "Code Quality"
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true,
       description: "Detailed description.");
   ```

2. **Add to SupportedDiagnostics** array in the same file

3. **Register the analyzer** in the `Initialize` method:
   ```csharp
   context.RegisterSyntaxNodeAction(AnalyzeYourSyntax, SyntaxKind.YourSyntaxKind);
   ```

4. **Implement analysis method**:
   ```csharp
   private static void AnalyzeYourSyntax(SyntaxNodeAnalysisContext context) {
       try {
           // Your analysis logic here
           if (shouldReportDiagnostic) {
               Diagnostic diagnostic = Diagnostic.Create(YOUR_ANALYZER, location, args);
               context.ReportDiagnostic(diagnostic);
           }
       } catch (Exception exc) {
           throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
       }
   }
   ```

5. **Create CodeFixProvider** (optional) in a new file named `YourFeatureCodeFixProvider.cs`:
   - Use `[ExportCodeFixProvider]` attribute
   - Implement `FixableDiagnosticIds` to return your diagnostic ID
   - Use `Renamer.RenameSymbolAsync` for renaming (note: currently deprecated - technical debt)

6. **Add unit tests** in a new file `YourFeatureUnitTests.cs`:
   - Extend `CodeFixVerifier`
   - Test both positive and negative cases
   - Test code fix if applicable
   - Use `VerifyCSharpDiagnostic` and `VerifyCSharpFix`

7. **Update README.md** with new analyzer documentation (section numbering follows RG00XX pattern)

## Coding Conventions

### Style
- **Tabs for indentation** (not spaces)
- **Brace style**: Opening brace on same line for most constructs
- **No comments** unless necessary for complex logic
- Prefer concise code over verbose explanations

### Naming
- DiagnosticDescriptor constants: ALL_CAPS_WITH_UNDERSCORES
- Diagnostic IDs: "RG00XX" format (sequential numbering)
- Test methods: Descriptive names starting with "Test"

### Error Handling
- Wrap analyzer methods in try-catch
- Rethrow with context: `throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);`

## Testing Guidelines

### Running Tests
```bash
dotnet build RG.CodeAnalyzer.sln
dotnet test RG.CodeAnalyzer.sln --no-build
```

### Test Patterns
- Use `DiagnosticResult` to define expected diagnostics
- Specify exact line and column numbers for diagnostic locations
- Test edge cases: empty code, no diagnostics, multiple diagnostics
- For code fixes: provide before and after code samples

### Known Test Issues (Technical Debt)
The following tests are currently skipped with `[Ignore]` attributes:
- **ArgumentMustBeLockedTests** (4 tests): RG0030 not implemented
- **LocalIsReadonlyTests** (2 tests): ref/ref readonly not supported
- **ParameterIsReadonlyTests** (1 test): ref parameters not supported

See README.md section 21 for note about work in progress features.

## Build and CI/CD

- **GitHub Actions**: `.github/workflows/dotnet.yml`
- Workflow runs on: push to master, PRs to master
- Uses .NET 10.0.x SDK
- Steps: restore → build → test

## Common Pitfalls

1. **Analyzer Release Tracking**: Currently disabled (technical debt - RS2008 warnings)
2. **Renamer.RenameSymbolAsync**: Deprecated but still in use (technical debt)
3. **Generic Type Analysis**: Be aware Roslyn 4.12.0 changed behavior for generic type arguments in records
4. **Test Expectations**: Ensure diagnostic counts match actual analyzer behavior (not duplicates)

## Documentation Standards

- Always update **README.md** when adding new analyzers
- Include code examples showing the warning
- Show the code fix behavior if applicable
- Use consistent formatting matching existing sections

## Version Management

- Package version in `RG.CodeAnalyzer.csproj`: Update for releases
- Keep dependencies up to date with latest stable versions
- Test thoroughly after Roslyn version upgrades (API changes occur)

## Future Considerations

### Known Technical Debt
1. Enable analyzer release tracking (RS2008)
2. Migrate from deprecated `Renamer.RenameSymbolAsync` API
3. Implement RG0030 (ArgumentMustBeLocked)
4. Support ref/ref readonly locals and parameters (RG0021, RG0022, RG0024)

### Best Practices Going Forward
- Don't introduce new test failures - fix them immediately
- Skip pre-existing failing tests with `[Ignore]` and clear reasons
- Maintain 100% passing test rate (excluding ignored tests)
- Update infrastructure (SDK, packages) regularly but test thoroughly
- Document all analyzers in README.md

## Quick Reference

### Diagnostic Categories
- **Performance**: Avoid performance anti-patterns
- **Reliability**: Prevent runtime issues
- **Code Quality**: Maintain code standards
- **Security**: Enforce security policies
- **Naming**: Naming conventions
- **Code Style**: Formatting and style
- **Maintainability**: Keep code maintainable
- **Convention**: Project-specific conventions

### Current Analyzer Count
As of this document: RG0001 through RG0032 (with some gaps/reserved IDs)

## Contact and Resources

- **Issues**: Track on GitHub Issues
- **Documentation**: README.md is the source of truth
- **License**: MIT License
