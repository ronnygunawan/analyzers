using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RG.CodeAnalyzer {
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveCancellationTokenCodeFixProvider)), Shared]
	public class RemoveCancellationTokenCodeFixProvider : CodeFixProvider {
		private const string REMOVE_CANCELLATION_TOKEN_TITLE = "Remove cancellation token argument";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RGDiagnosticAnalyzer.USE_OVERLOAD_WITHOUT_CANCELLATION_TOKEN_IF_ARGUMENT_IS_DEFAULT_ID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			if (context.Diagnostics.FirstOrDefault() is { Location: { SourceSpan: { Start: { } spanStart } } } diagnostic) {
				if (root.FindToken(spanStart).Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault() is InvocationExpressionSyntax invocation) {
					context.RegisterCodeFix(
						CodeAction.Create(
							title: REMOVE_CANCELLATION_TOKEN_TITLE,
							createChangedDocument: c => RemoveCancellationTokenAsync(context.Document, invocation, c),
							equivalenceKey: REMOVE_CANCELLATION_TOKEN_TITLE),
						diagnostic: diagnostic);
				}
			}
		}

		private static async Task<Document> RemoveCancellationTokenAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			if (invocation.ArgumentList.Arguments.Count > 0) {
				var newArgumentList = invocation.ArgumentList.RemoveNode(
					invocation.ArgumentList.Arguments.Last(),
					SyntaxRemoveOptions.KeepNoTrivia);
				
				if (newArgumentList is not null) {
					return document.WithSyntaxRoot(
						root: root.ReplaceNode(
							oldNode: invocation,
							newNode: invocation.WithArgumentList(newArgumentList)
						)
					);
				}
			}
			
			return document;
		}
	}
}
