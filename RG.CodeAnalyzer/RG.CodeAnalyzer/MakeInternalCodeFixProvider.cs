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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeInternalCodeFixProvider)), Shared]
	public class MakeInternalCodeFixProvider : CodeFixProvider {
		private const string MAKE_CLASS_INTERNAL_TITLE = "Make class internal";
		private const string MAKE_STRUCT_INTERNAL_TITLE = "Make struct internal";
		private const string MAKE_INTERFACE_INTERNAL_TITLE = "Make interface internal";
		private const string MAKE_ENUM_INTERNAL_TITLE = "Make enum internal";
		private const string MAKE_DELEGATE_INTERNAL_TITLE = "Make delegate internal";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RGDiagnosticAnalyzer.IDENTIFIERS_IN_INTERNAL_NAMESPACE_MUST_BE_INTERNAL_ID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			Diagnostic? diagnostic = context.Diagnostics.FirstOrDefault();

			if (diagnostic is null) return;

			TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

			if (root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault() is ClassDeclarationSyntax classDeclarationSyntax) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: MAKE_CLASS_INTERNAL_TITLE,
						createChangedDocument: c => MakeClassInternalAsync(context.Document, classDeclarationSyntax, c),
						equivalenceKey: MAKE_CLASS_INTERNAL_TITLE),
					diagnostic: diagnostic);
			} else if (root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<StructDeclarationSyntax>().FirstOrDefault() is StructDeclarationSyntax structDeclarationSyntax) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: MAKE_STRUCT_INTERNAL_TITLE,
						createChangedDocument: c => MakeStructInternalAsync(context.Document, structDeclarationSyntax, c),
						equivalenceKey: MAKE_STRUCT_INTERNAL_TITLE),
					diagnostic: diagnostic);
			} else if (root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<InterfaceDeclarationSyntax>().FirstOrDefault() is InterfaceDeclarationSyntax interfaceDeclarationSyntax) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: MAKE_INTERFACE_INTERNAL_TITLE,
						createChangedDocument: c => MakeInterfaceInternalAsync(context.Document, interfaceDeclarationSyntax, c),
						equivalenceKey: MAKE_INTERFACE_INTERNAL_TITLE),
					diagnostic: diagnostic);
			} else if (root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<EnumDeclarationSyntax>().FirstOrDefault() is EnumDeclarationSyntax enumDeclarationSyntax) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: MAKE_ENUM_INTERNAL_TITLE,
						createChangedDocument: c => MakeEnumInternalAsync(context.Document, enumDeclarationSyntax, c),
						equivalenceKey: MAKE_ENUM_INTERNAL_TITLE),
					diagnostic: diagnostic);
			} else if (root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<DelegateDeclarationSyntax>().FirstOrDefault() is DelegateDeclarationSyntax delegateDeclarationSyntax) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: MAKE_DELEGATE_INTERNAL_TITLE,
						createChangedDocument: c => MakeDelegateInternalAsync(context.Document, delegateDeclarationSyntax, c),
						equivalenceKey: MAKE_DELEGATE_INTERNAL_TITLE),
					diagnostic: diagnostic);
			}
		}

		private static async Task<Document> MakeClassInternalAsync(Document document, ClassDeclarationSyntax classDecl, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			SyntaxTriviaList leadingTrivia = classDecl.GetLeadingTrivia();

			SyntaxTokenList newModifiers = SyntaxFactory.TokenList(classDecl.Modifiers.Where(mod => mod.Kind() switch {
				SyntaxKind.PublicKeyword => false,
				SyntaxKind.ProtectedKeyword => false,
				SyntaxKind.SealedKeyword => false,
				_ => true
			})).Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));

			ClassDeclarationSyntax newClassDecl = classDecl
				.WithModifiers(newModifiers)
				.WithLeadingTrivia(leadingTrivia);

			SyntaxNode newRoot = root.ReplaceNode(classDecl, newClassDecl);
			Document newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}

		private static async Task<Document> MakeStructInternalAsync(Document document, StructDeclarationSyntax structDecl, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			SyntaxTriviaList leadingTrivia = structDecl.GetLeadingTrivia();

			SyntaxTokenList newModifiers = SyntaxFactory.TokenList(structDecl.Modifiers.Where(mod => mod.Kind() switch {
				SyntaxKind.PublicKeyword => false,
				SyntaxKind.ProtectedKeyword => false,
				SyntaxKind.SealedKeyword => false,
				_ => true
			})).Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));

			StructDeclarationSyntax newStructDecl = structDecl
				.WithModifiers(newModifiers)
				.WithLeadingTrivia(leadingTrivia);

			SyntaxNode newRoot = root.ReplaceNode(structDecl, newStructDecl);
			Document newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}

		private static async Task<Document> MakeInterfaceInternalAsync(Document document, InterfaceDeclarationSyntax interfaceDecl, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			SyntaxTriviaList leadingTrivia = interfaceDecl.GetLeadingTrivia();

			SyntaxTokenList newModifiers = SyntaxFactory.TokenList(interfaceDecl.Modifiers.Where(mod => mod.Kind() switch {
				SyntaxKind.PublicKeyword => false,
				SyntaxKind.ProtectedKeyword => false,
				SyntaxKind.SealedKeyword => false,
				_ => true
			})).Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));

			InterfaceDeclarationSyntax newInterfaceDecl = interfaceDecl
				.WithModifiers(newModifiers)
				.WithLeadingTrivia(leadingTrivia);

			SyntaxNode newRoot = root.ReplaceNode(interfaceDecl, newInterfaceDecl);
			Document newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}

		private static async Task<Document> MakeEnumInternalAsync(Document document, EnumDeclarationSyntax enumDecl, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			SyntaxTriviaList leadingTrivia = enumDecl.GetLeadingTrivia();

			SyntaxTokenList newModifiers = SyntaxFactory.TokenList(enumDecl.Modifiers.Where(mod => mod.Kind() switch {
				SyntaxKind.PublicKeyword => false,
				SyntaxKind.ProtectedKeyword => false,
				SyntaxKind.SealedKeyword => false,
				_ => true
			})).Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));

			EnumDeclarationSyntax newEnumDecl = enumDecl
				.WithModifiers(newModifiers)
				.WithLeadingTrivia(leadingTrivia);

			SyntaxNode newRoot = root.ReplaceNode(enumDecl, newEnumDecl);
			Document newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}

		private static async Task<Document> MakeDelegateInternalAsync(Document document, DelegateDeclarationSyntax delegateDecl, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root is null) return document;

			SyntaxTriviaList leadingTrivia = delegateDecl.GetLeadingTrivia();

			SyntaxTokenList newModifiers = SyntaxFactory.TokenList(delegateDecl.Modifiers.Where(mod => mod.Kind() switch {
				SyntaxKind.PublicKeyword => false,
				SyntaxKind.ProtectedKeyword => false,
				SyntaxKind.SealedKeyword => false,
				_ => true
			})).Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));

			DelegateDeclarationSyntax newDelegateDecl = delegateDecl
				.WithModifiers(newModifiers)
				.WithLeadingTrivia(leadingTrivia);

			SyntaxNode newRoot = root.ReplaceNode(delegateDecl, newDelegateDecl);
			Document newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}
	}
}
