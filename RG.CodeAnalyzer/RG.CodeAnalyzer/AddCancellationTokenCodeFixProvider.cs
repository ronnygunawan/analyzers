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
		private const string AddCancellationTokenTitle = "Add cancellation token";

		public override ImmutableArray<string> FixableDiagnosticIds {
			get {
				return ImmutableArray.Create(RGDiagnosticAnalyzer.NotUsingOverloadWithCancellationTokenId);
			}
		}

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (context.Diagnostics.FirstOrDefault() is { Location: { SourceSpan: { Start: { } spanStart } } } diagnostic) {
				if (root.FindToken(spanStart).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault() is InvocationExpressionSyntax invocation) {
					context.RegisterCodeFix(
						CodeAction.Create(
							title: AddCancellationTokenTitle,
							createChangedDocument: c => AddCancellationTokenAsync(context.Document, invocation, c),
							equivalenceKey: AddCancellationTokenTitle),
						diagnostic: diagnostic);
				}
			}
		}

		private async Task<Document> AddCancellationTokenAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken) {
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			if (invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault() is { ParameterList: { Parameters: { } callerParameters } }
				&& callerParameters.FirstOrDefault(parameter => parameter?.Type?.ToString() is string type
					&& (type == "CancellationToken" || type == "System.Threading.CancellationToken")
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
