using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RG.CodeAnalyzer {
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class CA1812Suppressor : DiagnosticSuppressor {
		private const string CA1812_ID = "CA1812";

		private static readonly SuppressionDescriptor CA1812_SUPPRESSION = new(
			id: "RGS001",
			suppressedDiagnosticId: CA1812_ID,
			justification: "Internal class is used as a type argument, which may be instantiated by reflection or external code."
		);

		public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(CA1812_SUPPRESSION);

		public override void ReportSuppressions(SuppressionAnalysisContext context) {
			foreach (Diagnostic diagnostic in context.ReportedDiagnostics) {
				if (diagnostic.Id != CA1812_ID) continue;

				SyntaxTree? tree = diagnostic.Location.SourceTree;
				if (tree is null) continue;

				SyntaxNode root = tree.GetRoot(context.CancellationToken);
				SyntaxNode? node = root.FindNode(diagnostic.Location.SourceSpan);
				if (node is null) continue;

				TypeDeclarationSyntax? typeDeclaration = node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
				if (typeDeclaration is null) continue;

				SemanticModel semanticModel = context.GetSemanticModel(tree);
				INamedTypeSymbol? typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration, context.CancellationToken) as INamedTypeSymbol;
				if (typeSymbol is null) continue;

				if (IsUsedAsTypeArgument(typeSymbol, context.Compilation, context.CancellationToken)) {
					context.ReportSuppression(Suppression.Create(CA1812_SUPPRESSION, diagnostic));
				}
			}
		}

		private static bool IsUsedAsTypeArgument(
			INamedTypeSymbol typeSymbol,
			Compilation compilation,
			System.Threading.CancellationToken cancellationToken) {
			foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees) {
				SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
				SyntaxNode root = syntaxTree.GetRoot(cancellationToken);

				foreach (SyntaxNode node in root.DescendantNodes()) {
					switch (node) {
						case GenericNameSyntax genericName:
							foreach (TypeSyntax typeArg in genericName.TypeArgumentList.Arguments) {
								TypeInfo typeInfo = semanticModel.GetTypeInfo(typeArg, cancellationToken);
								if (SymbolEqualityComparer.Default.Equals(typeInfo.Type, typeSymbol)) {
									return true;
								}
							}
							break;
						case TypeOfExpressionSyntax typeOfExpression:
							TypeInfo typeOfTypeInfo = semanticModel.GetTypeInfo(typeOfExpression.Type, cancellationToken);
							if (SymbolEqualityComparer.Default.Equals(typeOfTypeInfo.Type, typeSymbol)) {
								return true;
							}
							if (typeOfExpression.Type is GenericNameSyntax genericTypeOf) {
								foreach (TypeSyntax typeArg in genericTypeOf.TypeArgumentList.Arguments) {
									TypeInfo typeInfo = semanticModel.GetTypeInfo(typeArg, cancellationToken);
									if (SymbolEqualityComparer.Default.Equals(typeInfo.Type, typeSymbol)) {
										return true;
									}
								}
							}
							break;
					}
				}
			}

			return false;
		}
	}
}
