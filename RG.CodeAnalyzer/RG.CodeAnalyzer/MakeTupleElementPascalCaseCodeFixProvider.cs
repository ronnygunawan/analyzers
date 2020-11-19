using Microsoft.CodeAnalysis;
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
		//private const string MAKE_ELEMENT_NAME_PASCAL_CASE_TITLE = "Rename to pascal case";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RGDiagnosticAnalyzer.TUPLE_ELEMENT_NAMES_MUST_BE_IN_PASCAL_CASE_ID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override Task RegisterCodeFixesAsync(CodeFixContext context) {
			return Task.CompletedTask;

			//SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

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

		private static async Task<Solution> MakeElementNamePascalCaseAsync(Document document, TupleElementSyntax tupleElement, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document.Project.Solution;

			string elementName = tupleElement.Identifier.ValueText;

			if (RGDiagnosticAnalyzer.IsInCamelCase(elementName)
				&& tupleElement.Ancestors().OfType<TupleTypeSyntax>().FirstOrDefault() is TupleTypeSyntax tupleTypeSyntax) {
				string pascalCase = RGDiagnosticAnalyzer.ToPascalCase(elementName);
				SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

				if (semanticModel is null) return document.Project.Solution;
				if (semanticModel.GetSymbolInfo(tupleTypeSyntax, cancellationToken).Symbol is not INamedTypeSymbol tupleSymbol) return document.Project.Solution;

				IFieldSymbol? fieldSymbol = tupleSymbol.TupleElements.FirstOrDefault(element => element.Name == elementName);

				if (fieldSymbol is null) return document.Project.Solution;

				Solution solution = document.Project.Solution;
				return await Renamer.RenameSymbolAsync(solution, fieldSymbol, pascalCase, solution.Workspace.Options, cancellationToken).ConfigureAwait(false);
			} else {
				return document.Project.Solution;
			}
		}
	}
}
