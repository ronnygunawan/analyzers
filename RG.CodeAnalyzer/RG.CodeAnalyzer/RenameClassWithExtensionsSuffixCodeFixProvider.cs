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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameClassWithExtensionsSuffixCodeFixProvider)), Shared]
	public class RenameClassWithExtensionsSuffixCodeFixProvider : CodeFixProvider {
		private const string RENAME_TO_EXTENSIONS_TITLE = "Add 'Extensions' suffix";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RGDiagnosticAnalyzer.STATIC_CLASS_WITH_EXTENSION_METHODS_SHOULD_HAVE_EXTENSIONS_SUFFIX_ID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			Diagnostic? diagnostic = context.Diagnostics.FirstOrDefault();

			if (diagnostic is null) return;

			var diagnosticSpan = diagnostic.Location.SourceSpan;

			if (root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault() is ClassDeclarationSyntax classDeclarationSyntax) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: RENAME_TO_EXTENSIONS_TITLE,
						createChangedSolution: c => RenameClassAsync(context.Document, classDeclarationSyntax, c),
						equivalenceKey: RENAME_TO_EXTENSIONS_TITLE),
					diagnostic: diagnostic);
			}
		}

		private static async Task<Solution> RenameClassAsync(Document document, ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken) {
			SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (semanticModel is null) return document.Project.Solution;

			if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken) is not INamedTypeSymbol classSymbol) return document.Project.Solution;

			string oldName = classSymbol.Name;
			string newName = oldName + "Extensions";

			Solution solution = document.Project.Solution;
			var renameOptions = new SymbolRenameOptions(
				RenameOverloads: false,
				RenameInStrings: false,
				RenameInComments: false,
				RenameFile: false
			);
			return await Renamer.RenameSymbolAsync(solution, classSymbol, renameOptions, newName, cancellationToken).ConfigureAwait(false);
		}
	}
}
