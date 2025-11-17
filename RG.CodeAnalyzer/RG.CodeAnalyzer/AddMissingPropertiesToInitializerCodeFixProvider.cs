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
	using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddMissingPropertiesToInitializerCodeFixProvider)), Shared]
	public class AddMissingPropertiesToInitializerCodeFixProvider : CodeFixProvider {
		private const string ADD_MISSING_PROPERTIES_TITLE = "Add missing properties";
		private const string ADD_MISSING_REQUIRED_PROPERTIES_TITLE = "Add missing required properties";

		// This code fix provider doesn't fix any specific diagnostic - it provides refactoring-style code fixes
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray<string>.Empty;

		public override FixAllProvider GetFixAllProvider() => null;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			// Find the object initializer at the diagnostic location
			SyntaxToken token = root.FindToken(context.Span.Start);
			InitializerExpressionSyntax? initializer = token.Parent?.AncestorsAndSelf().OfType<InitializerExpressionSyntax>().FirstOrDefault();

			if (initializer is null || initializer.Kind() != SyntaxKind.ObjectInitializerExpression) return;

			if (initializer.Parent is not BaseObjectCreationExpressionSyntax objectCreation) return;

			SemanticModel? semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			if (semanticModel is null) return;

			ITypeSymbol? typeSymbol = semanticModel.GetTypeInfo(objectCreation, context.CancellationToken).Type;
			if (typeSymbol is null) return;

			// Get all settable properties
			var allSettableProperties = GetSettableProperties(typeSymbol);
			var alreadyInitializedProperties = GetInitializedProperties(initializer);
			var missingProperties = allSettableProperties.Where(p => !alreadyInitializedProperties.Contains(p.Name)).ToList();

			if (missingProperties.Count == 0) return;

			// Register "Add missing properties" code fix
			context.RegisterCodeFix(
				CodeAction.Create(
					title: ADD_MISSING_PROPERTIES_TITLE,
					createChangedDocument: c => AddPropertiesAsync(context.Document, objectCreation, initializer, missingProperties, semanticModel, c),
					equivalenceKey: ADD_MISSING_PROPERTIES_TITLE),
				context.Diagnostics);

			// Get required properties
			var requiredProperties = GetRequiredProperties(typeSymbol, semanticModel, context.CancellationToken);
			var missingRequiredProperties = requiredProperties.Where(p => !alreadyInitializedProperties.Contains(p.Name)).ToList();

			if (missingRequiredProperties.Count > 0) {
				// Register "Add missing required properties" code fix
				context.RegisterCodeFix(
					CodeAction.Create(
						title: ADD_MISSING_REQUIRED_PROPERTIES_TITLE,
						createChangedDocument: c => AddPropertiesAsync(context.Document, objectCreation, initializer, missingRequiredProperties, semanticModel, c),
						equivalenceKey: ADD_MISSING_REQUIRED_PROPERTIES_TITLE),
					context.Diagnostics);
			}
		}

		private static ImmutableArray<IPropertySymbol> GetSettableProperties(ITypeSymbol typeSymbol) {
			return typeSymbol.GetMembers()
				.OfType<IPropertySymbol>()
				.Where(p => p.DeclaredAccessibility == Accessibility.Public &&
							!p.IsStatic &&
							(p.SetMethod != null || p.IsInitOnly))
				.ToImmutableArray();
		}

		private static ImmutableHashSet<string> GetInitializedProperties(InitializerExpressionSyntax initializer) {
			return initializer.Expressions
				.OfType<AssignmentExpressionSyntax>()
				.Select(a => a.Left)
				.OfType<IdentifierNameSyntax>()
				.Select(i => i.Identifier.ValueText)
				.ToImmutableHashSet();
		}

		private static ImmutableArray<IPropertySymbol> GetRequiredProperties(ITypeSymbol typeSymbol, SemanticModel semanticModel, CancellationToken cancellationToken) {
			var requiredProperties = ImmutableArray.CreateBuilder<IPropertySymbol>();

			foreach (var property in typeSymbol.GetMembers().OfType<IPropertySymbol>()) {
				if (property.DeclaredAccessibility != Accessibility.Public || property.IsStatic) continue;
				if (property.SetMethod == null && !property.IsInitOnly) continue;

				// Check for [Required] attribute
				var hasRequiredAttribute = property.GetAttributes().Any(attr =>
					attr.AttributeClass?.Name == "RequiredAttribute" ||
					attr.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.RequiredAttribute");

				if (hasRequiredAttribute) {
					requiredProperties.Add(property);
					continue;
				}

				// Check for @ prefix (for records)
				if (property.DeclaringSyntaxReferences.Length > 0) {
					var propertySyntax = property.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as PropertyDeclarationSyntax;
					if (propertySyntax?.Identifier.Text is string text && text.Length > 0 && text[0] == '@') {
						requiredProperties.Add(property);
					}
				}
			}

			return requiredProperties.ToImmutable();
		}

		private static async Task<Document> AddPropertiesAsync(
			Document document,
			BaseObjectCreationExpressionSyntax objectCreation,
			InitializerExpressionSyntax initializer,
			ImmutableArray<IPropertySymbol> propertiesToAdd,
			SemanticModel semanticModel,
			CancellationToken cancellationToken) {

			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			if (root is null) return document;

			// Create new assignments for each property
			var newAssignments = propertiesToAdd.Select(property => {
				var defaultValue = GetDefaultValueExpression(property.Type);
				return AssignmentExpression(
					SyntaxKind.SimpleAssignmentExpression,
					IdentifierName(property.Name),
					defaultValue);
			}).ToArray();

			// Combine with existing expressions
			var allExpressions = initializer.Expressions.AddRange(newAssignments);

			// Create new initializer with all expressions
			var newInitializer = initializer.WithExpressions(
				SeparatedList<ExpressionSyntax>(allExpressions));

			// Replace the old initializer with the new one
			var newRoot = root.ReplaceNode(initializer, newInitializer);

			return document.WithSyntaxRoot(newRoot);
		}

		private static ExpressionSyntax GetDefaultValueExpression(ITypeSymbol typeSymbol) {
			// For value types, use default literal
			if (typeSymbol.IsValueType) {
				if (typeSymbol.SpecialType == SpecialType.System_Boolean) {
					return LiteralExpression(SyntaxKind.FalseLiteralExpression);
				}
				if (typeSymbol.SpecialType == SpecialType.System_Int32 ||
					typeSymbol.SpecialType == SpecialType.System_Int64 ||
					typeSymbol.SpecialType == SpecialType.System_Int16 ||
					typeSymbol.SpecialType == SpecialType.System_Byte ||
					typeSymbol.SpecialType == SpecialType.System_SByte ||
					typeSymbol.SpecialType == SpecialType.System_UInt16 ||
					typeSymbol.SpecialType == SpecialType.System_UInt32 ||
					typeSymbol.SpecialType == SpecialType.System_UInt64) {
					return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0));
				}
				if (typeSymbol.SpecialType == SpecialType.System_Double) {
					return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0.0));
				}
				if (typeSymbol.SpecialType == SpecialType.System_Single) {
					return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0.0f));
				}
				if (typeSymbol.SpecialType == SpecialType.System_Decimal) {
					return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0m));
				}
				if (typeSymbol.SpecialType == SpecialType.System_Char) {
					return LiteralExpression(SyntaxKind.CharacterLiteralExpression, Literal('\0'));
				}
			}

			// For strings, use empty string
			if (typeSymbol.SpecialType == SpecialType.System_String) {
				return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(""));
			}

			// For everything else, use default literal
			return LiteralExpression(SyntaxKind.DefaultLiteralExpression, Token(SyntaxKind.DefaultKeyword));
		}
	}
}
