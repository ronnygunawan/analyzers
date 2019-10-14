using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RG.CodeAnalyzer {
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class RGDiagnosticAnalyzer : DiagnosticAnalyzer {
		public const string NoAwaitInsideLoopId = "RG0001";

		private static DiagnosticDescriptor NoAwaitInsideLoop = new DiagnosticDescriptor(
			id: NoAwaitInsideLoopId,
			title: "Do not await inside a loop.",
			messageFormat: "Asynchronous operation awaited inside {0}.",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not await inside a loop. Perform asynchronous operations in a batch instead.");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
			get {
				return ImmutableArray.Create(
					NoAwaitInsideLoop
				);
			}
		}

		public override void Initialize(AnalysisContext context) {
			context.RegisterSyntaxNodeAction(AnalyzeAwaitExpression, SyntaxKind.AwaitExpression);
		}

		private static void AnalyzeAwaitExpression(SyntaxNodeAnalysisContext context) {
			SyntaxNode loopNode = context.Node.Ancestors().FirstOrDefault(ancestor => {
				SyntaxKind kind = ancestor.Kind();
				return kind == SyntaxKind.ForStatement
					|| kind == SyntaxKind.ForEachStatement
					|| kind == SyntaxKind.WhileStatement
					|| kind == SyntaxKind.DoStatement
					|| kind == SyntaxKind.SelectClause;
			});
			if (loopNode != null) {
				var diagnostic = Diagnostic.Create(NoAwaitInsideLoop, context.Node.GetLocation(), loopNode.Kind() switch
				{
					SyntaxKind.ForStatement => "for loop",
					SyntaxKind.ForEachStatement => "foreach loop",
					SyntaxKind.WhileStatement => "while loop",
					SyntaxKind.DoStatement => "do..while loop",
					SyntaxKind.SelectClause => "select clause",
					_ => "loop"
				});
				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
