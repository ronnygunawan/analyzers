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

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
			get {
				return ImmutableArray.Create(
					NoAwaitInsideLoop,
					DontReturnTaskIfMethodDisposesObject
				);
			}
		}

		public override void Initialize(AnalysisContext context) {
			context.RegisterSyntaxNodeAction(AnalyzeAwaitExpression, SyntaxKind.AwaitExpression);
			context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
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
					if (context.SemanticModel.GetSymbolInfo(methodDeclarationSyntax.ReturnType).Symbol is INamedTypeSymbol namedTypeSymbol
						&& namedTypeSymbol.ToString() is string fullName
						&& fullName.StartsWith("System.Threading.Tasks.Task")
						&& !methodDeclarationSyntax.Modifiers.Any(SyntaxKind.AsyncKeyword)) {
						var diagnostic = Diagnostic.Create(DontReturnTaskIfMethodDisposesObject, context.Node.GetLocation(), methodDeclarationSyntax.Identifier.ValueText);
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
	}
}
