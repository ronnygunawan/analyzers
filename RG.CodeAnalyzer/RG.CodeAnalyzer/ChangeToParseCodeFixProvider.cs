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

	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ChangeToParseCodeFixProvider)), Shared]
	public class ChangeToParseCodeFixProvider : CodeFixProvider {
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RGDiagnosticAnalyzer.DO_NOT_PARSE_USING_CONVERT_ID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			if (context.Diagnostics.FirstOrDefault() is { Location: { SourceSpan: { Start: { } spanStart } } } diagnostic) {
				if (root.FindToken(spanStart).Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault() is
					{
						Expression: MemberAccessExpressionSyntax
						{
							Expression: IdentifierNameSyntax
							{
								Identifier:
								{
									ValueText: nameof(Convert)
								}
							},
							Name: IdentifierNameSyntax
							{
								Identifier:
								{
									ValueText: string methodName
								}
							}
						}
					} invocation) {
					(string? typeName, SyntaxKind? keyword) = methodName switch {
						nameof(Convert.ToBoolean) => ("bool", SyntaxKind.BoolKeyword),
						nameof(Convert.ToByte) => ("byte", SyntaxKind.ByteKeyword),
						nameof(Convert.ToChar) => ("char", SyntaxKind.CharKeyword),
						nameof(Convert.ToDateTime) => ("DateTime", null),
						nameof(Convert.ToDecimal) => ("decimal", SyntaxKind.DecimalKeyword),
						nameof(Convert.ToDouble) => ("double", SyntaxKind.DoubleKeyword),
						nameof(Convert.ToInt16) => ("short", SyntaxKind.ShortKeyword),
						nameof(Convert.ToInt32) => ("int", SyntaxKind.IntKeyword),
						nameof(Convert.ToInt64) => ("long", SyntaxKind.LongKeyword),
						nameof(Convert.ToSByte) => ("sbyte", SyntaxKind.SByteKeyword),
						nameof(Convert.ToSingle) => ("float", SyntaxKind.FloatKeyword),
						nameof(Convert.ToUInt16) => ("ushort", SyntaxKind.UShortKeyword),
						nameof(Convert.ToUInt32) => ("uint", SyntaxKind.UIntKeyword),
						nameof(Convert.ToUInt64) => ("ulong", SyntaxKind.ULongKeyword),
						_ => ((string?)null, (SyntaxKind?)null)
					};
					if (typeName is not null) {
						string title = $"Change to '{typeName}.Parse'";
						context.RegisterCodeFix(
							CodeAction.Create(
								title: title,
								createChangedDocument: c => ChangeToParseAsync(context.Document, invocation, typeName, keyword, c),
								equivalenceKey: title),
							diagnostic: diagnostic);
					}
				}
			}
		}

		private static async Task<Document> ChangeToParseAsync(Document document, InvocationExpressionSyntax invocation, string typeName, SyntaxKind? keyword, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			if (keyword.HasValue) {
				return document.WithSyntaxRoot(
					root: root.ReplaceNode(
						oldNode: invocation,
						newNode: invocation.WithExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, PredefinedType(Token(keyword.Value)), IdentifierName("Parse")))
					)
				);
			} else {
				return document.WithSyntaxRoot(
					root: root.ReplaceNode(
						oldNode: invocation,
						newNode: invocation.WithExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(typeName), IdentifierName("Parse")))
					)
				);
			}
		}
	}
}
