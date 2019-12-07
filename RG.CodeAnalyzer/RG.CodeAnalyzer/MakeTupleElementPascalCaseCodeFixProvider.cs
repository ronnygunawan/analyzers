using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RG.CodeAnalyzer {
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeTupleElementPascalCaseCodeFixProvider)), Shared]
	public class MakeTupleElementPascalCaseCodeFixProvider : CodeFixProvider {
		private const string MakeElementNamePascalCaseTitle = "Rename to pascal case";

		public override ImmutableArray<string> FixableDiagnosticIds {
			get {
				return ImmutableArray.Create(RGDiagnosticAnalyzer.TupleElementNamesMustBeInPascalCaseId);
			}
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			// Disabled until following issue is fixed:
			// https://github.com/dotnet/roslyn/issues/14115
			//
			//var diagnostic = context.Diagnostics.First();
			//var diagnosticSpan = diagnostic.Location.SourceSpan;

			//if (root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TupleElementSyntax>().FirstOrDefault() is TupleElementSyntax tupleElement) {
			//	if (tupleElement.Identifier is { ValueText: var elementName }
			//		&& RGDiagnosticAnalyzer.IsInCamelCase(elementName)) {
			//		context.RegisterCodeFix(
			//			CodeAction.Create(
			//				title: MakeElementNamePascalCaseTitle,
			//				createChangedSolution: c => MakeElementNamePascalCaseAsync(context.Document, tupleElement, c),
			//				equivalenceKey: MakeElementNamePascalCaseTitle),
			//			diagnostic: diagnostic);
			//	}
			//}
		}

		private async Task<Solution> MakeElementNamePascalCaseAsync(Document document, TupleElementSyntax tupleElement, CancellationToken cancellationToken) {
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var elementName = tupleElement.Identifier.ValueText;
			if (RGDiagnosticAnalyzer.IsInCamelCase(elementName)
				&& tupleElement.Ancestors().OfType<TupleTypeSyntax>().FirstOrDefault() is TupleTypeSyntax tupleTypeSyntax) {
				var pascalCase = RGDiagnosticAnalyzer.ToPascalCase(elementName);
				var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
				var tupleSymbol = semanticModel.GetSymbolInfo(tupleTypeSyntax, cancellationToken).Symbol as INamedTypeSymbol;
				var fieldSymbol = tupleSymbol.TupleElements.Single(element => element.Name == elementName);
				var solution = document.Project.Solution;
				return await Renamer.RenameSymbolAsync(solution, fieldSymbol, pascalCase, solution.Workspace.Options, cancellationToken).ConfigureAwait(false);
			} else {
				return document.Project.Solution;
			}
		}
	}
}
