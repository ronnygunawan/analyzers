using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RG.CodeAnalyzer {
	using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddMissingPropertiesCodeFixProvider)), Shared]
	public class AddMissingPropertiesCodeFixProvider : CodeFixProvider {
		private const string ADD_MISSING_PROPERTIES_TITLE = "Add missing properties";
		private const string ADD_MISSING_REQUIRED_PROPERTIES_TITLE = "Add missing required properties";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RGDiagnosticAnalyzer.REQUIRED_RECORD_PROPERTY_SHOULD_BE_INITIALIZED_ID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			Diagnostic? diagnostic = context.Diagnostics.FirstOrDefault();

			if (diagnostic is null) return;

			Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

			SyntaxNode? nodeAtDiagnostic = root.FindToken(diagnosticSpan.Start).Parent;
			
			BaseObjectCreationExpressionSyntax? objectCreation = nodeAtDiagnostic?.AncestorsAndSelf().OfType<BaseObjectCreationExpressionSyntax>().FirstOrDefault();
			
			if (objectCreation is null) return;

			SemanticModel? semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			
			if (semanticModel is null) return;

			if (semanticModel.GetTypeInfo(objectCreation, context.CancellationToken).Type is not INamedTypeSymbol namedTypeSymbol) return;

			if (namedTypeSymbol.DeclaringSyntaxReferences.Length == 0) return;

			if (namedTypeSymbol.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken) is not RecordDeclarationSyntax recordDeclaration) return;

			context.RegisterCodeFix(
				CodeAction.Create(
					title: ADD_MISSING_PROPERTIES_TITLE,
					createChangedDocument: c => AddMissingPropertiesAsync(context.Document, objectCreation, recordDeclaration, semanticModel, allProperties: true, c),
					equivalenceKey: ADD_MISSING_PROPERTIES_TITLE),
				diagnostic: diagnostic);

			context.RegisterCodeFix(
				CodeAction.Create(
					title: ADD_MISSING_REQUIRED_PROPERTIES_TITLE,
					createChangedDocument: c => AddMissingPropertiesAsync(context.Document, objectCreation, recordDeclaration, semanticModel, allProperties: false, c),
					equivalenceKey: ADD_MISSING_REQUIRED_PROPERTIES_TITLE),
				diagnostic: diagnostic);
		}

		private static async Task<Document> AddMissingPropertiesAsync(
			Document document,
			BaseObjectCreationExpressionSyntax objectCreation,
			RecordDeclarationSyntax recordDeclaration,
			SemanticModel semanticModel,
			bool allProperties,
			CancellationToken cancellationToken) {
			
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			List<string> initializedProperties = new();
			if (objectCreation.Initializer?.Expressions is SeparatedSyntaxList<ExpressionSyntax> expressions) {
				foreach (ExpressionSyntax expr in expressions) {
					if (expr is AssignmentExpressionSyntax { Left: IdentifierNameSyntax identifierName }) {
						initializedProperties.Add(identifierName.Identifier.ValueText);
					}
				}
			}

			List<PropertyDeclarationSyntax> propertiesToAdd = new();
			foreach (MemberDeclarationSyntax member in recordDeclaration.Members) {
				if (member is PropertyDeclarationSyntax propertyDeclaration) {
					string propertyName = propertyDeclaration.Identifier.ValueText;
					
					if (initializedProperties.Contains(propertyName)) continue;

					bool isRequired = false;
					if (propertyDeclaration.Identifier.Text is { Length: > 0 } text && text[0] == '@') {
						isRequired = true;
					} else if (propertyDeclaration.AttributeLists
						.SelectMany(attributeList => attributeList.Attributes)
						.Any(attribute => {
							string attributeName = attribute.Name.ToString();
							if (attributeName is not ("Required" or "System.ComponentModel.DataAnnotations.Required")) {
								return false;
							}
							if (semanticModel.GetTypeInfo(attribute, cancellationToken).Type is INamedTypeSymbol attrSymbol) {
								return attrSymbol.ToString() == "System.ComponentModel.DataAnnotations.RequiredAttribute";
							}
							return false;
						})) {
						isRequired = true;
					}

					if (allProperties || isRequired) {
						propertiesToAdd.Add(propertyDeclaration);
					}
				}
			}

			if (propertiesToAdd.Count == 0) return document;

			SyntaxTriviaList itemLeadingTrivia;
			SyntaxTriviaList closeBraceLeadingTrivia;
			
			if (objectCreation.Initializer is { Expressions: { Count: > 0 } existingExpressions }) {
				itemLeadingTrivia = existingExpressions[0].GetLeadingTrivia();
				closeBraceLeadingTrivia = objectCreation.Initializer.CloseBraceToken.LeadingTrivia;
			} else if (objectCreation.Initializer is not null) {
				SyntaxToken statementLeadingToken = objectCreation.GetFirstToken();
				SyntaxTriviaList statementIndent = statementLeadingToken.LeadingTrivia;
				
				string indentString = string.Concat(statementIndent.Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia)).Select(t => t.ToString()));
				int tabCount = indentString.Count(c => c == '\t');
				int spaceCount = indentString.Count(c => c == ' ');
				int totalIndent = tabCount + (spaceCount / 4);
				
				string tabs = new string('\t', totalIndent + 1);
				itemLeadingTrivia = TriviaList(Whitespace(tabs));
				
				string closeTabs = new string('\t', totalIndent);
				closeBraceLeadingTrivia = TriviaList(Whitespace(closeTabs));
			} else {
				SyntaxToken statementLeadingToken = objectCreation.GetFirstToken();
				SyntaxTriviaList statementIndent = statementLeadingToken.LeadingTrivia;
				
				itemLeadingTrivia = TriviaList(LineFeed).AddRange(statementIndent).Add(Tab);
				closeBraceLeadingTrivia = TriviaList(LineFeed).AddRange(statementIndent);
			}

			List<SyntaxNodeOrToken> newAssignmentsWithSeparators = new();
			for (int i = 0; i < propertiesToAdd.Count; i++) {
				PropertyDeclarationSyntax property = propertiesToAdd[i];
				
				AssignmentExpressionSyntax assignment = AssignmentExpression(
					SyntaxKind.SimpleAssignmentExpression,
					IdentifierName(property.Identifier.ValueText),
					IdentifierName("_")
				).WithLeadingTrivia(itemLeadingTrivia);

				newAssignmentsWithSeparators.Add(assignment);
				
				if (i < propertiesToAdd.Count - 1) {
					newAssignmentsWithSeparators.Add(Token(SyntaxKind.CommaToken));
				}
			}

			InitializerExpressionSyntax newInitializer;
			if (objectCreation.Initializer is null) {
				newInitializer = InitializerExpression(
					SyntaxKind.ObjectInitializerExpression,
					Token(SyntaxKind.OpenBraceToken),
					SeparatedList<ExpressionSyntax>(newAssignmentsWithSeparators),
					Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(closeBraceLeadingTrivia)
				);
			} else {
				List<SyntaxNodeOrToken> combinedNodesAndTokens = new();
				
				SeparatedSyntaxList<ExpressionSyntax> currentExpressions = objectCreation.Initializer.Expressions;
				for (int i = 0; i < currentExpressions.Count; i++) {
					combinedNodesAndTokens.Add(currentExpressions[i]);
					if (i < currentExpressions.SeparatorCount) {
						combinedNodesAndTokens.Add(currentExpressions.GetSeparator(i));
					}
				}
				
				if (currentExpressions.Count > 0) {
					combinedNodesAndTokens.Add(Token(SyntaxKind.CommaToken));
				}
				
				combinedNodesAndTokens.AddRange(newAssignmentsWithSeparators);
				
				SeparatedSyntaxList<ExpressionSyntax> combinedExpressions = SeparatedList<ExpressionSyntax>(combinedNodesAndTokens);
				newInitializer = objectCreation.Initializer.WithExpressions(combinedExpressions);
			}

			BaseObjectCreationExpressionSyntax newObjectCreation = objectCreation.WithInitializer(newInitializer);

			SyntaxNode newRoot = root.ReplaceNode(objectCreation, newObjectCreation);
			return document.WithSyntaxRoot(newRoot);
		}
	}
}
