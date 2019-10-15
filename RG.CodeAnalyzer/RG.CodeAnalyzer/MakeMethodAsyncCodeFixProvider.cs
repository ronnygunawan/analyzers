using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RG.CodeAnalyzer {
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeMethodAsyncCodeFixProvider)), Shared]
	public class MakeMethodAsyncCodeFixProvider : CodeFixProvider {
		private const string MakeMethodAsyncTitle = "Make method async";

		public override ImmutableArray<string> FixableDiagnosticIds {
			get {
				return ImmutableArray.Create(RGDiagnosticAnalyzer.DontReturnTaskIfMethodDisposesObjectId);
			}
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			if (root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault() is MethodDeclarationSyntax declaration) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: MakeMethodAsyncTitle,
						createChangedDocument: c => MakeMethodAsyncAsync(context.Document, declaration, c),
						equivalenceKey: MakeMethodAsyncTitle),
					diagnostic: diagnostic);
			}
		}

		private async Task<Document> MakeMethodAsyncAsync(Document document, MethodDeclarationSyntax methodDecl, CancellationToken cancellationToken) {
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newModifiers = SyntaxFactory.TokenList(methodDecl.Modifiers).Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));
			var newMethodDecl = methodDecl.WithModifiers(newModifiers);
			var newRoot = root.ReplaceNode(methodDecl, newMethodDecl);
			while (newRoot.DescendantNodes().OfType<ReturnStatementSyntax>().FirstOrDefault() is ReturnStatementSyntax returnStatement
				&& returnStatement.Expression.Kind() != SyntaxKind.AwaitExpression) {
				newRoot = newRoot.ReplaceNode(returnStatement, returnStatement.WithExpression(SyntaxFactory.AwaitExpression(returnStatement.Expression)));
			}
			var newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}
	}
}
