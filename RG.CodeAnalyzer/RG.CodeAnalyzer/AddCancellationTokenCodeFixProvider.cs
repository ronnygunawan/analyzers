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
	using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddCancellationTokenCodeFixProvider)), Shared]
	public class AddCancellationTokenCodeFixProvider : CodeFixProvider {
		private const string ADD_CANCELLATION_TOKEN_TITLE = "Add cancellation token";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RGDiagnosticAnalyzer.NOT_USING_OVERLOAD_WITH_CANCELLATION_TOKEN_ID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			if (context.Diagnostics.FirstOrDefault() is { Location: { SourceSpan: { Start: { } spanStart } } } diagnostic) {
				if (root.FindToken(spanStart).Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault() is InvocationExpressionSyntax invocation) {
					context.RegisterCodeFix(
						CodeAction.Create(
							title: ADD_CANCELLATION_TOKEN_TITLE,
							createChangedDocument: c => AddCancellationTokenAsync(context.Document, invocation, c),
							equivalenceKey: ADD_CANCELLATION_TOKEN_TITLE),
						diagnostic: diagnostic);
				}
			}
		}

		private static async Task<Document> AddCancellationTokenAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			if (invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault() is { ParameterList: { Parameters: { } callerParameters } }
				&& callerParameters.FirstOrDefault(parameter => parameter.Type?.ToString() is string type
					&& (type is "CancellationToken" or "System.Threading.CancellationToken")
				) is { } parameter) {
				return document.WithSyntaxRoot(
					root: root.ReplaceNode(
						oldNode: invocation,
						newNode: invocation.WithArgumentList(invocation.ArgumentList.AddArguments(Argument(ParseExpression(parameter.Identifier.ValueText))))
					)
				);
			} else {
				return document;
			}
		}
	}
}
