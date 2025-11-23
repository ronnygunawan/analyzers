using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RG.CodeAnalyzer {
	using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AddMissingNamedArgumentsRefactoringProvider)), Shared]
	public class AddMissingNamedArgumentsRefactoringProvider : CodeRefactoringProvider {
		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			SyntaxNode? node = root.FindNode(context.Span);

			// Handle object creation (new Person())
			if (node is BaseObjectCreationExpressionSyntax objectCreation) {
				await RegisterObjectCreationRefactoringsAsync(context, objectCreation);
				return;
			}

			// Handle method invocation (Foo())
			if (node is InvocationExpressionSyntax invocation) {
				await RegisterInvocationRefactoringsAsync(context, invocation);
				return;
			}

			// Also check ancestors for object creation or invocation
			BaseObjectCreationExpressionSyntax? ancestorObjectCreation = node?.AncestorsAndSelf().OfType<BaseObjectCreationExpressionSyntax>().FirstOrDefault();
			if (ancestorObjectCreation is not null) {
				await RegisterObjectCreationRefactoringsAsync(context, ancestorObjectCreation);
				return;
			}

			InvocationExpressionSyntax? ancestorInvocation = node?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
			if (ancestorInvocation is not null) {
				await RegisterInvocationRefactoringsAsync(context, ancestorInvocation);
			}
		}

		private async Task RegisterObjectCreationRefactoringsAsync(
			CodeRefactoringContext context,
			BaseObjectCreationExpressionSyntax objectCreation) {

			SemanticModel? semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			if (semanticModel is null) return;

			ITypeSymbol? typeSymbol = semanticModel.GetTypeInfo(objectCreation, context.CancellationToken).Type;
			if (typeSymbol is null) return;

			// Get all constructors
			IEnumerable<IMethodSymbol> constructors = typeSymbol.GetMembers()
				.OfType<IMethodSymbol>()
				.Where(m => m.MethodKind == MethodKind.Constructor && !m.IsStatic);

			foreach (IMethodSymbol constructor in constructors) {
				if (constructor.Parameters.Length == 0) continue;

				string title = constructor.Parameters.Length == 1
					? "Add missing named argument"
					: "Add missing named arguments";

				if (constructors.Count() > 1) {
					// Show parameter types in title when multiple overloads exist
					string paramTypes = string.Join(", ", constructor.Parameters.Select(p => p.Type.Name));
					title += $" ({paramTypes})";
				}

				CodeAction action = CodeAction.Create(
					title: title,
					createChangedDocument: c => AddNamedArgumentsToObjectCreationAsync(
						context.Document, objectCreation, constructor, c),
					equivalenceKey: title);

				context.RegisterRefactoring(action);
			}
		}

		private async Task RegisterInvocationRefactoringsAsync(
			CodeRefactoringContext context,
			InvocationExpressionSyntax invocation) {

			SemanticModel? semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			if (semanticModel is null) return;

			SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(invocation, context.CancellationToken);
			
			List<IMethodSymbol> methods = new();
			if (symbolInfo.Symbol is IMethodSymbol method) {
				methods.Add(method);
			}
			
			// Also check candidate symbols for overloads
			if (symbolInfo.CandidateSymbols.Length > 0) {
				methods.AddRange(symbolInfo.CandidateSymbols.OfType<IMethodSymbol>());
			}

			if (methods.Count == 0) return;

			foreach (IMethodSymbol methodSymbol in methods) {
				if (methodSymbol.Parameters.Length == 0) continue;

				string title = methodSymbol.Parameters.Length == 1
					? "Add missing named argument"
					: "Add missing named arguments";

				if (methods.Count > 1) {
					// Show parameter types in title when multiple overloads exist
					string paramTypes = string.Join(", ", methodSymbol.Parameters.Select(p => p.Type.Name));
					title += $" ({paramTypes})";
				}

				CodeAction action = CodeAction.Create(
					title: title,
					createChangedDocument: c => AddNamedArgumentsToInvocationAsync(
						context.Document, invocation, methodSymbol, c),
					equivalenceKey: title);

				context.RegisterRefactoring(action);
			}
		}

		private async Task<Document> AddNamedArgumentsToObjectCreationAsync(
			Document document,
			BaseObjectCreationExpressionSyntax objectCreation,
			IMethodSymbol constructor,
			CancellationToken cancellationToken) {

			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			if (root is null) return document;

			ArgumentListSyntax? argumentList = objectCreation.ArgumentList;
			if (argumentList is null) {
				// No argument list, create one
				argumentList = ArgumentList();
			}

			List<ArgumentSyntax> newArguments = new();
			foreach (IParameterSymbol parameter in constructor.Parameters) {
				ArgumentSyntax argument = Argument(
					NameColon(IdentifierName(parameter.Name)),
					Token(SyntaxKind.None),
					IdentifierName("_")
				);
				newArguments.Add(argument);
			}

			// Format with proper indentation
			SyntaxToken statementLeadingToken = objectCreation.GetFirstToken();
			SyntaxTriviaList statementIndent = statementLeadingToken.LeadingTrivia;
			
			string indentString = string.Concat(statementIndent.Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia)).Select(t => t.ToString()));
			int tabCount = indentString.Count(c => c == '\t');
			int spaceCount = indentString.Count(c => c == ' ');
			int totalIndent = tabCount + (spaceCount / 4);
			
			string argumentIndent = new string('\t', totalIndent + 1);
			SyntaxTriviaList argumentLeadingTrivia = TriviaList(LineFeed, Whitespace(argumentIndent));
			
			string closingIndent = new string('\t', totalIndent);
			SyntaxTriviaList closingTrivia = TriviaList(LineFeed, Whitespace(closingIndent));

			SeparatedSyntaxList<ArgumentSyntax> formattedArguments = SeparatedList(
				newArguments.Select((arg, index) => {
					if (index == 0) {
						return arg.WithLeadingTrivia(argumentLeadingTrivia);
					}
					return arg.WithLeadingTrivia(argumentLeadingTrivia);
				}),
				Enumerable.Repeat(Token(SyntaxKind.CommaToken), newArguments.Count - 1)
			);

			ArgumentListSyntax newArgumentList = ArgumentList(formattedArguments)
				.WithCloseParenToken(Token(SyntaxKind.CloseParenToken).WithLeadingTrivia(closingTrivia));

			BaseObjectCreationExpressionSyntax newObjectCreation = objectCreation.WithArgumentList(newArgumentList);

			SyntaxNode newRoot = root.ReplaceNode(objectCreation, newObjectCreation);
			return document.WithSyntaxRoot(newRoot);
		}

		private async Task<Document> AddNamedArgumentsToInvocationAsync(
			Document document,
			InvocationExpressionSyntax invocation,
			IMethodSymbol method,
			CancellationToken cancellationToken) {

			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			if (root is null) return document;

			ArgumentListSyntax argumentList = invocation.ArgumentList;

			List<ArgumentSyntax> newArguments = new();
			foreach (IParameterSymbol parameter in method.Parameters) {
				ArgumentSyntax argument = Argument(
					NameColon(IdentifierName(parameter.Name)),
					Token(SyntaxKind.None),
					IdentifierName("_")
				);
				newArguments.Add(argument);
			}

			// Format with proper indentation
			SyntaxToken statementLeadingToken = invocation.GetFirstToken();
			SyntaxTriviaList statementIndent = statementLeadingToken.LeadingTrivia;
			
			string indentString = string.Concat(statementIndent.Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia)).Select(t => t.ToString()));
			int tabCount = indentString.Count(c => c == '\t');
			int spaceCount = indentString.Count(c => c == ' ');
			int totalIndent = tabCount + (spaceCount / 4);
			
			string argumentIndent = new string('\t', totalIndent + 1);
			SyntaxTriviaList argumentLeadingTrivia = TriviaList(LineFeed, Whitespace(argumentIndent));
			
			string closingIndent = new string('\t', totalIndent);
			SyntaxTriviaList closingTrivia = TriviaList(LineFeed, Whitespace(closingIndent));

			SeparatedSyntaxList<ArgumentSyntax> formattedArguments = SeparatedList(
				newArguments.Select((arg, index) => {
					if (index == 0) {
						return arg.WithLeadingTrivia(argumentLeadingTrivia);
					}
					return arg.WithLeadingTrivia(argumentLeadingTrivia);
				}),
				Enumerable.Repeat(Token(SyntaxKind.CommaToken), newArguments.Count - 1)
			);

			ArgumentListSyntax newArgumentList = ArgumentList(formattedArguments)
				.WithCloseParenToken(Token(SyntaxKind.CloseParenToken).WithLeadingTrivia(closingTrivia));

			InvocationExpressionSyntax newInvocation = invocation.WithArgumentList(newArgumentList);

			SyntaxNode newRoot = root.ReplaceNode(invocation, newInvocation);
			return document.WithSyntaxRoot(newRoot);
		}
	}
}
