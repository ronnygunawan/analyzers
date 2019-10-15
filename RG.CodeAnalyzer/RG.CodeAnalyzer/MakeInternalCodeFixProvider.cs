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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeInternalCodeFixProvider)), Shared]
	public class MakeInternalCodeFixProvider : CodeFixProvider {
		private const string MakeClassInternalTitle = "Make class internal";
		private const string MakeStructInternalTitle = "Make struct internal";
		private const string MakeInterfaceInternalTitle = "Make interface internal";
		private const string MakeEnumInternalTitle = "Make enum internal";
		private const string MakeDelegateInternalTitle = "Make delegate internal";

		public override ImmutableArray<string> FixableDiagnosticIds {
			get {
				return ImmutableArray.Create(RGDiagnosticAnalyzer.IdentifiersInInternalNamespaceMustBeInternalId);
			}
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			if (root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault() is ClassDeclarationSyntax classDeclarationSyntax) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: MakeClassInternalTitle,
						createChangedDocument: c => MakeClassInternalAsync(context.Document, classDeclarationSyntax, c),
						equivalenceKey: MakeClassInternalTitle),
					diagnostic: diagnostic);
			} else if (root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<StructDeclarationSyntax>().FirstOrDefault() is StructDeclarationSyntax structDeclarationSyntax) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: MakeStructInternalTitle,
						createChangedDocument: c => MakeStructInternalAsync(context.Document, structDeclarationSyntax, c),
						equivalenceKey: MakeStructInternalTitle),
					diagnostic: diagnostic);
			} else if (root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InterfaceDeclarationSyntax>().FirstOrDefault() is InterfaceDeclarationSyntax interfaceDeclarationSyntax) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: MakeInterfaceInternalTitle,
						createChangedDocument: c => MakeInterfaceInternalAsync(context.Document, interfaceDeclarationSyntax, c),
						equivalenceKey: MakeInterfaceInternalTitle),
					diagnostic: diagnostic);
			} else if (root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<EnumDeclarationSyntax>().FirstOrDefault() is EnumDeclarationSyntax enumDeclarationSyntax) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: MakeEnumInternalTitle,
						createChangedDocument: c => MakeEnumInternalAsync(context.Document, enumDeclarationSyntax, c),
						equivalenceKey: MakeEnumInternalTitle),
					diagnostic: diagnostic);
			} else if (root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<DelegateDeclarationSyntax>().FirstOrDefault() is DelegateDeclarationSyntax delegateDeclarationSyntax) {
				context.RegisterCodeFix(
					CodeAction.Create(
						title: MakeDelegateInternalTitle,
						createChangedDocument: c => MakeDelegateInternalAsync(context.Document, delegateDeclarationSyntax, c),
						equivalenceKey: MakeDelegateInternalTitle),
					diagnostic: diagnostic);
			}
		}

		private async Task<Document> MakeClassInternalAsync(Document document, ClassDeclarationSyntax classDecl, CancellationToken cancellationToken) {
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var leadingTrivia = classDecl.GetLeadingTrivia();
			var newModifiers = SyntaxFactory.TokenList(classDecl.Modifiers.Where(mod => mod.Kind() switch {
				SyntaxKind.PublicKeyword => false,
				SyntaxKind.ProtectedKeyword => false,
				SyntaxKind.SealedKeyword => false,
				_ => true
			})).Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
			var newClassDecl = classDecl
				.WithModifiers(newModifiers)
				.WithLeadingTrivia(leadingTrivia);
			var newRoot = root.ReplaceNode(classDecl, newClassDecl);
			var newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}

		private async Task<Document> MakeStructInternalAsync(Document document, StructDeclarationSyntax structDecl, CancellationToken cancellationToken) {
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var leadingTrivia = structDecl.GetLeadingTrivia();
			var newModifiers = SyntaxFactory.TokenList(structDecl.Modifiers.Where(mod => mod.Kind() switch {
				SyntaxKind.PublicKeyword => false,
				SyntaxKind.ProtectedKeyword => false,
				SyntaxKind.SealedKeyword => false,
				_ => true
			})).Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
			var newStructDecl = structDecl
				.WithModifiers(newModifiers)
				.WithLeadingTrivia(leadingTrivia);
			var newRoot = root.ReplaceNode(structDecl, newStructDecl);
			var newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}

		private async Task<Document> MakeInterfaceInternalAsync(Document document, InterfaceDeclarationSyntax interfaceDecl, CancellationToken cancellationToken) {
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var leadingTrivia = interfaceDecl.GetLeadingTrivia();
			var newModifiers = SyntaxFactory.TokenList(interfaceDecl.Modifiers.Where(mod => mod.Kind() switch {
				SyntaxKind.PublicKeyword => false,
				SyntaxKind.ProtectedKeyword => false,
				SyntaxKind.SealedKeyword => false,
				_ => true
			})).Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
			var newInterfaceDecl = interfaceDecl
				.WithModifiers(newModifiers)
				.WithLeadingTrivia(leadingTrivia);
			var newRoot = root.ReplaceNode(interfaceDecl, newInterfaceDecl);
			var newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}

		private async Task<Document> MakeEnumInternalAsync(Document document, EnumDeclarationSyntax enumDecl, CancellationToken cancellationToken) {
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var leadingTrivia = enumDecl.GetLeadingTrivia();
			var newModifiers = SyntaxFactory.TokenList(enumDecl.Modifiers.Where(mod => mod.Kind() switch {
				SyntaxKind.PublicKeyword => false,
				SyntaxKind.ProtectedKeyword => false,
				SyntaxKind.SealedKeyword => false,
				_ => true
			})).Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
			var newEnumDecl = enumDecl
				.WithModifiers(newModifiers)
				.WithLeadingTrivia(leadingTrivia);
			var newRoot = root.ReplaceNode(enumDecl, newEnumDecl);
			var newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}

		private async Task<Document> MakeDelegateInternalAsync(Document document, DelegateDeclarationSyntax delegateDecl, CancellationToken cancellationToken) {
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var leadingTrivia = delegateDecl.GetLeadingTrivia();
			var newModifiers = SyntaxFactory.TokenList(delegateDecl.Modifiers.Where(mod => mod.Kind() switch {
				SyntaxKind.PublicKeyword => false,
				SyntaxKind.ProtectedKeyword => false,
				SyntaxKind.SealedKeyword => false,
				_ => true
			})).Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
			var newDelegateDecl = delegateDecl
				.WithModifiers(newModifiers)
				.WithLeadingTrivia(leadingTrivia);
			var newRoot = root.ReplaceNode(delegateDecl, newDelegateDecl);
			var newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}
	}
}
