using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RG.CodeAnalyzer {
	using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(GenerateGuidCodeRefactoringProvider)), Shared]
	public class GenerateGuidCodeRefactoringProvider : CodeRefactoringProvider {
		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			// Get the syntax node at the cursor position
			var node = root.FindNode(context.Span);
			
			// Check if we're dealing with a string literal
			if (node is LiteralExpressionSyntax literalExpression && 
			    literalExpression.IsKind(SyntaxKind.StringLiteralExpression)) {
				// Check if the string is empty
				var token = literalExpression.Token;
				if (token.IsKind(SyntaxKind.StringLiteralToken) && token.ValueText == string.Empty) {
					var action = CodeAction.Create(
						title: "Generate GUID",
						createChangedDocument: c => GenerateGuidAsync(context.Document, literalExpression, c),
						equivalenceKey: "GenerateGuid");

					context.RegisterRefactoring(action);
				}
			}
		}

		private static async Task<Document> GenerateGuidAsync(Document document, LiteralExpressionSyntax literalExpression, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			// Generate a new GUID
			string newGuid = Guid.NewGuid().ToString();

			// Create a new string literal with the GUID
			var newLiteral = LiteralExpression(
				SyntaxKind.StringLiteralExpression,
				Literal(newGuid)
			).WithTriviaFrom(literalExpression);

			return document.WithSyntaxRoot(
				root: root.ReplaceNode(
					oldNode: literalExpression,
					newNode: newLiteral
				)
			);
		}
	}
}
