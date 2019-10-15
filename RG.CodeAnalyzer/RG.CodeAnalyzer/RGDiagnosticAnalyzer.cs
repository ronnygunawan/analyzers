using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RG.CodeAnalyzer {
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class RGDiagnosticAnalyzer : DiagnosticAnalyzer {
		public const string NoAwaitInsideLoopId = "RG0001";
		public const string DontReturnTaskIfMethodDisposesObjectId = "RG0002";
		public const string IdentifiersInInternalNamespaceMustBeInternalId = "RG0003";

		private static DiagnosticDescriptor NoAwaitInsideLoop = new DiagnosticDescriptor(
			id: NoAwaitInsideLoopId,
			title: "Do not await inside a loop.",
			messageFormat: "Asynchronous operation awaited inside {0}.",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not await inside a loop. Perform asynchronous operations in a batch instead.");

		private static DiagnosticDescriptor DontReturnTaskIfMethodDisposesObject = new DiagnosticDescriptor(
			id: DontReturnTaskIfMethodDisposesObjectId,
			title: "Do not return Task from a method that disposes object.",
			messageFormat: "Method '{0}' disposes an object and shouldn't return Task.",
			category: "Reliability",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not return Task from a method that disposes an object. Mark method as async instead.");

		private static DiagnosticDescriptor IdentifiersInInternalNamespaceMustBeInternal = new DiagnosticDescriptor(
			id: IdentifiersInInternalNamespaceMustBeInternalId,
			title: "Identifiers declared in Internal namespace must be internal.",
			messageFormat: "Identifier '{0}' is declared in '{1}' namespace, and thus must be declared internal.",
			category: "Security",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Identifiers declared in Internal namespace must be internal.");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
			get {
				return ImmutableArray.Create(
					NoAwaitInsideLoop,
					DontReturnTaskIfMethodDisposesObject,
					IdentifiersInInternalNamespaceMustBeInternal
				);
			}
		}

		public override void Initialize(AnalysisContext context) {
			context.RegisterSyntaxNodeAction(AnalyzeAwaitExpression, SyntaxKind.AwaitExpression);
			context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
			context.RegisterSymbolAction(AnalyzeNamedTypeDeclaration, SymbolKind.NamedType);
		}

		private static void AnalyzeAwaitExpression(SyntaxNodeAnalysisContext context) {
			SyntaxNode loopNode = context.Node.Ancestors().FirstOrDefault(ancestor => {
				SyntaxKind kind = ancestor.Kind();
				return kind == SyntaxKind.ForStatement
					|| kind == SyntaxKind.ForEachStatement
					|| kind == SyntaxKind.WhileStatement
					|| kind == SyntaxKind.DoStatement;
			});
			if (loopNode != null) {
				var diagnostic = Diagnostic.Create(NoAwaitInsideLoop, context.Node.GetLocation(), loopNode.Kind() switch
				{
					SyntaxKind.ForStatement => "for loop",
					SyntaxKind.ForEachStatement => "foreach loop",
					SyntaxKind.WhileStatement => "while loop",
					SyntaxKind.DoStatement => "do..while loop",
					_ => "loop"
				});
				context.ReportDiagnostic(diagnostic);
			}
		}

		private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context) {
			SyntaxNode methodNode = context.Node.Ancestors().FirstOrDefault(ancestor => {
				SyntaxKind kind = ancestor.Kind();
				return kind == SyntaxKind.MethodDeclaration
					|| kind == SyntaxKind.ParenthesizedLambdaExpression
					|| kind == SyntaxKind.SimpleLambdaExpression;
			});
			switch (methodNode) {
				case MethodDeclarationSyntax methodDeclarationSyntax:
					if (context.SemanticModel.GetSymbolInfo(methodDeclarationSyntax.ReturnType, context.CancellationToken).Symbol is INamedTypeSymbol namedTypeSymbol
						&& namedTypeSymbol.ToString() is string fullName
						&& fullName.StartsWith("System.Threading.Tasks.Task")
						&& !methodDeclarationSyntax.Modifiers.Any(SyntaxKind.AsyncKeyword)) {
						var diagnostic = Diagnostic.Create(DontReturnTaskIfMethodDisposesObject, methodDeclarationSyntax.GetLocation(), methodDeclarationSyntax.Identifier.ValueText);
						context.ReportDiagnostic(diagnostic);
					}
					break;
				case ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax:
					// TODO: handle parenthesized lambda expression
					break;
				case SimpleLambdaExpressionSyntax simpleLambdaExpressionSyntax:
					// TODO: handle simple lambda expression
					break;
			}
		}

		private static void AnalyzeNamedTypeDeclaration(SymbolAnalysisContext context) {
			if (IsInternalNamespace(context.Symbol.ContainingNamespace, out string fullNamespace)) {
				switch (context.Symbol.DeclaredAccessibility) {
					case Accessibility.Internal:
					case Accessibility.Private:
					case Accessibility.Protected:
					case Accessibility.ProtectedAndInternal:
						return;
					default:
						var diagnostic = Diagnostic.Create(IdentifiersInInternalNamespaceMustBeInternal, context.Symbol.DeclaringSyntaxReferences[0].GetSyntax().GetLocation(), context.Symbol.Name, fullNamespace);
						context.ReportDiagnostic(diagnostic);
						return;
				}
			}
		}

		private static bool IsInternalNamespace(INamespaceSymbol @namespace, out string fullNamespace) {
			fullNamespace = "";
			bool isInternal = false;
			while (@namespace is { }) {
				if (@namespace.Name == "Internal"
					|| @namespace.Name == "Internals") {
					isInternal = true;
				}
				if (!string.IsNullOrEmpty(@namespace.Name)) {
					if (fullNamespace.Length > 0) {
						fullNamespace = $"{@namespace.Name}.{fullNamespace}";
					} else {
						fullNamespace = @namespace.Name;
					}
				}
				@namespace = @namespace.ContainingNamespace;
			}
			return isInternal;
		}
	}
}
