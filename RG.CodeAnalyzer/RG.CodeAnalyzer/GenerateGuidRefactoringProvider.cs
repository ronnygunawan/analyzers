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
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(GenerateGuidRefactoringProvider)), Shared]
	public class GenerateGuidRefactoringProvider : CodeRefactoringProvider {
		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			SyntaxNode? node = root.FindNode(context.Span);

			if (node is LiteralExpressionSyntax literalExpression &&
				literalExpression.IsKind(SyntaxKind.StringLiteralExpression) &&
				literalExpression.Token.ValueText == string.Empty) {
				
				CodeAction action = CodeAction.Create(
					title: "Generate GUID",
					createChangedDocument: c => GenerateGuidAsync(context.Document, literalExpression, c),
					equivalenceKey: nameof(GenerateGuidRefactoringProvider));

				context.RegisterRefactoring(action);
			}
		}

		private static async Task<Document> GenerateGuidAsync(Document document, LiteralExpressionSyntax literalExpression, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			string newGuid = Guid.NewGuid().ToString();
			LiteralExpressionSyntax newLiteral = SyntaxFactory.LiteralExpression(
				SyntaxKind.StringLiteralExpression,
				SyntaxFactory.Literal(newGuid));

			SyntaxNode newRoot = root.ReplaceNode(literalExpression, newLiteral);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
