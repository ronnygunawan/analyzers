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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddMutableAttributeCodeFixProvider)), Shared]
	public class AddMutableAttributeCodeFixProvider : CodeFixProvider {
		private const string ADD_MUTABLE_ATTRIBUTE_TITLE = "Add [Mutable] attribute";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			RGDiagnosticAnalyzer.RECORDS_SHOULD_NOT_CONTAIN_SET_ACCESSOR_ID,
			RGDiagnosticAnalyzer.RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_FIELD_ID,
			RGDiagnosticAnalyzer.RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_COLLECTION_ID,
			RGDiagnosticAnalyzer.RECORDS_SHOULD_NOT_CONTAIN_REFERENCE_TO_CLASS_OR_STRUCT_TYPE_ID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null) return;

			Diagnostic? diagnostic = context.Diagnostics.FirstOrDefault();

			if (diagnostic is null) return;

			TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

			SyntaxNode? node = root.FindNode(diagnosticSpan);
			if (node is null) return;

			RecordDeclarationSyntax? recordDeclaration = node.AncestorsAndSelf().OfType<RecordDeclarationSyntax>().FirstOrDefault();
			if (recordDeclaration is null) return;

			bool alreadyHasMutableAttribute = recordDeclaration.AttributeLists
				.SelectMany(al => al.Attributes)
				.Any(attr => {
					string name = attr.Name.ToString();
					return name == "Mutable" || name == "MutableAttribute" || name == "RG.Annotations.Mutable" || name == "RG.Annotations.MutableAttribute";
				});

			if (alreadyHasMutableAttribute) return;

			context.RegisterCodeFix(
				CodeAction.Create(
					title: ADD_MUTABLE_ATTRIBUTE_TITLE,
					createChangedDocument: c => AddMutableAttributeAsync(context.Document, recordDeclaration, c),
					equivalenceKey: ADD_MUTABLE_ATTRIBUTE_TITLE),
				diagnostic: diagnostic);
		}

		private static async Task<Document> AddMutableAttributeAsync(Document document, RecordDeclarationSyntax recordDeclaration, CancellationToken cancellationToken) {
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			if (root is null) return document;

			SyntaxTriviaList leadingTrivia = recordDeclaration.GetLeadingTrivia();

			AttributeSyntax mutableAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Mutable"));
			AttributeListSyntax attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(mutableAttribute))
				.WithLeadingTrivia(leadingTrivia)
				.WithTrailingTrivia(SyntaxFactory.EndOfLine("\n"));

			RecordDeclarationSyntax newRecordDeclaration = recordDeclaration
				.WithAttributeLists(recordDeclaration.AttributeLists.Insert(0, attributeList))
				.WithLeadingTrivia(SyntaxFactory.TriviaList());

			SyntaxNode newRoot = root.ReplaceNode(recordDeclaration, newRecordDeclaration);

			CompilationUnitSyntax? compilationUnit = newRoot as CompilationUnitSyntax;
			if (compilationUnit is not null) {
				bool hasRgAnnotationsUsing = compilationUnit.Usings
					.Any(u => u.Name?.ToString() == "RG.Annotations");

				if (!hasRgAnnotationsUsing) {
					UsingDirectiveSyntax usingDirective = SyntaxFactory.UsingDirective(
						SyntaxFactory.QualifiedName(
							SyntaxFactory.IdentifierName("RG"),
							SyntaxFactory.IdentifierName("Annotations")))
						.WithTrailingTrivia(SyntaxFactory.EndOfLine("\n"));

					newRoot = compilationUnit.AddUsings(usingDirective);
				}
			}

			Document newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}
	}
}
