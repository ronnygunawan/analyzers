using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RG.CodeAnalyzer {
	using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RefactorExpressionBodiedPropertyCodeFixProvider)), Shared]
	public class RefactorExpressionBodiedPropertyCodeFixProvider : CodeFixProvider {
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RGDiagnosticAnalyzer.REFACTOR_EXPRESSION_BODIED_PROPERTY_TO_AUTO_PROPERTY_ID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			if (context.Diagnostics.FirstOrDefault() is { Location: { SourceSpan: { Start: { } spanStart } } } diagnostic) {
				if (root.FindToken(spanStart).Parent?.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().FirstOrDefault() is PropertyDeclarationSyntax property) {
					string title = "Refactor to auto-property with initializer";
					context.RegisterCodeFix(
						CodeAction.Create(
							title: title,
							createChangedDocument: c => RefactorToAutoPropertyAsync(context.Document, property, c),
							equivalenceKey: title),
						diagnostic: diagnostic);
				}
			}
		}

		private static async Task<Document> RefactorToAutoPropertyAsync(Document document, PropertyDeclarationSyntax property, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			if (property.ExpressionBody is ArrowExpressionClauseSyntax { Expression: var expression }) {
				PropertyDeclarationSyntax newProperty = property
					.WithExpressionBody(null)
					.WithAccessorList(
						AccessorList(
							SingletonList(
								AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
									.WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
							)
						)
					)
					.WithInitializer(
						EqualsValueClause(expression.WithoutTrivia())
					)
					.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

				return document.WithSyntaxRoot(
					root: root.ReplaceNode(
						oldNode: property,
						newNode: newProperty
					)
				);
			}

			return document;
		}
	}
}
