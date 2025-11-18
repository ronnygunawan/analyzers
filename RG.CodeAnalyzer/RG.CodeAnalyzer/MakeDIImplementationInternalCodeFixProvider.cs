using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RG.CodeAnalyzer {
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeDIImplementationInternalCodeFixProvider)), Shared]
	public class MakeDIImplementationInternalCodeFixProvider : CodeFixProvider {
		private const string MAKE_CLASS_INTERNAL_TITLE = "Make DI implementation internal";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RGDiagnosticAnalyzer.DI_IMPLEMENTATION_MUST_BE_INTERNAL_ID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			Diagnostic? diagnostic = context.Diagnostics.FirstOrDefault();

			if (diagnostic is null) return;

			TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

			SyntaxNode? node = root.FindNode(diagnosticSpan);
			if (node is null) return;

			InvocationExpressionSyntax? invocation = node.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
			if (invocation is null) return;

			SemanticModel? semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			if (semanticModel is null) return;

			IMethodSymbol? methodSymbol = semanticModel.GetSymbolInfo(invocation.Expression, context.CancellationToken).Symbol as IMethodSymbol;
			if (methodSymbol is null) return;

			ITypeSymbol? implementationType = null;
			if (methodSymbol.TypeArguments.Length > 0) {
				implementationType = methodSymbol.TypeArguments.Length > 1 
					? methodSymbol.TypeArguments[1] 
					: methodSymbol.TypeArguments[0];
			} else if (invocation.ArgumentList.Arguments.Count > 0
				&& invocation.ArgumentList.Arguments[0].Expression is TypeOfExpressionSyntax typeOfExpression
				&& semanticModel.GetTypeInfo(typeOfExpression.Type, context.CancellationToken).Type is ITypeSymbol typeArg) {
				implementationType = typeArg;
			}

			if (implementationType is not INamedTypeSymbol namedType) return;

			if (namedType.TypeKind != TypeKind.Class) return;

			SyntaxReference? syntaxRef = namedType.DeclaringSyntaxReferences.FirstOrDefault();
			if (syntaxRef is null) return;

			Document? typeDocument = context.Document.Project.GetDocument(syntaxRef.SyntaxTree);
			if (typeDocument is null) return;

			context.RegisterCodeFix(
				CodeAction.Create(
					title: MAKE_CLASS_INTERNAL_TITLE,
					createChangedDocument: c => MakeClassInternalAsync(typeDocument, namedType, c),
					equivalenceKey: MAKE_CLASS_INTERNAL_TITLE),
				diagnostic: diagnostic);
		}

		private static async Task<Document> MakeClassInternalAsync(Document document, INamedTypeSymbol classSymbol, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			if (root is null) return document;

			// Get the class declaration from the symbol
			SyntaxReference? syntaxRef = classSymbol.DeclaringSyntaxReferences.FirstOrDefault();
			if (syntaxRef is null) return document;

			// Find the node in the current document's tree by location
			SyntaxNode? classNode = root.FindNode(syntaxRef.Span);
			if (classNode is not ClassDeclarationSyntax actualClassDecl) return document;

			SyntaxTriviaList leadingTrivia = actualClassDecl.GetLeadingTrivia();

			SyntaxTokenList newModifiers = SyntaxFactory.TokenList(actualClassDecl.Modifiers.Where(mod => mod.Kind() switch {
				SyntaxKind.PublicKeyword => false,
				SyntaxKind.ProtectedKeyword => false,
				SyntaxKind.SealedKeyword => false,
				_ => true
			}));

			if (!newModifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword))) {
				// Get the trivia from the public keyword if it exists, otherwise use default
				SyntaxToken? publicToken = actualClassDecl.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.PublicKeyword));
				SyntaxToken internalToken = publicToken.HasValue 
					? SyntaxFactory.Token(publicToken.Value.LeadingTrivia, SyntaxKind.InternalKeyword, publicToken.Value.TrailingTrivia)
					: SyntaxFactory.Token(SyntaxKind.InternalKeyword);
				newModifiers = newModifiers.Insert(0, internalToken);
			}

			ClassDeclarationSyntax newClassDecl = actualClassDecl
				.WithModifiers(newModifiers)
				.WithLeadingTrivia(leadingTrivia);

			SyntaxNode newRoot = root.ReplaceNode(actualClassDecl, newClassDecl);
			Document newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}
	}
}
