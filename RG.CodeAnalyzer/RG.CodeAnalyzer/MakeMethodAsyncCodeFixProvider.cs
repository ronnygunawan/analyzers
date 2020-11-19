using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RG.CodeAnalyzer {
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeMethodAsyncCodeFixProvider)), Shared]
	public class MakeMethodAsyncCodeFixProvider : CodeFixProvider {
		private const string MAKE_METHOD_ASYNC_TITLE = "Make method async";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RGDiagnosticAnalyzer.DONT_RETURN_TASK_IF_METHOD_DISPOSES_OBJECT_ID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			Diagnostic? diagnostic = context.Diagnostics.FirstOrDefault();

			if (diagnostic is null) return;

			TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

			if (root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault() is MethodDeclarationSyntax declaration) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: MAKE_METHOD_ASYNC_TITLE,
						createChangedDocument: c => MakeMethodAsyncAsync(context.Document, declaration, c),
						equivalenceKey: MAKE_METHOD_ASYNC_TITLE),
					diagnostic: diagnostic);
			}
		}

		private static async Task<Document> MakeMethodAsyncAsync(Document document, MethodDeclarationSyntax methodDecl, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			SyntaxTokenList newModifiers = SyntaxFactory.TokenList(methodDecl.Modifiers).Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));
			MethodDeclarationSyntax newMethodDecl = methodDecl.WithModifiers(newModifiers);
			SyntaxNode newRoot = root.ReplaceNode(methodDecl, newMethodDecl);

			while (newRoot.DescendantNodes().OfType<ReturnStatementSyntax>().FirstOrDefault() is ReturnStatementSyntax returnStatement
				&& returnStatement.Expression is { } returnExpression
				&& returnExpression.Kind() != SyntaxKind.AwaitExpression) {
				newRoot = newRoot.ReplaceNode(returnStatement, returnStatement.WithExpression(SyntaxFactory.AwaitExpression(returnExpression)));
			}

			Document newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}
	}
}
