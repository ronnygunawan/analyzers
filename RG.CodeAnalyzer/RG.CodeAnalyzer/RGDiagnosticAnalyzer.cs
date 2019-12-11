using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
		public const string DoNotAccessPrivateFieldsOfAnotherObjectDirectlyId = "RG0004";
		public const string DoNotCallDisposeOnStaticReadonlyFieldsId = "RG0005";
		public const string DoNotCallTaskWaitToInvokeTaskId = "RG0006";
		public const string DoNotAccessTaskResultToInvokeTaskId = "RG0007";
		public const string TupleElementNamesMustBeInPascalCaseId = "RG0008";
		public const string NotUsingOverloadWithCancellationTokenId = "RG0009";
		public const string VarInferredTypeIsObsoleteId = "RG0010";

		private static readonly DiagnosticDescriptor NoAwaitInsideLoop = new DiagnosticDescriptor(
			id: NoAwaitInsideLoopId,
			title: "Do not await inside a loop.",
			messageFormat: "Asynchronous operation awaited inside {0}.",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not await inside a loop. Perform asynchronous operations in a batch instead.");

		private static readonly DiagnosticDescriptor DontReturnTaskIfMethodDisposesObject = new DiagnosticDescriptor(
			id: DontReturnTaskIfMethodDisposesObjectId,
			title: "Do not return Task from a method that disposes object.",
			messageFormat: "Method '{0}' disposes an object and shouldn't return Task.",
			category: "Reliability",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not return Task from a method that disposes an object. Mark method as async instead.");

		private static readonly DiagnosticDescriptor IdentifiersInInternalNamespaceMustBeInternal = new DiagnosticDescriptor(
			id: IdentifiersInInternalNamespaceMustBeInternalId,
			title: "Identifiers declared in Internal namespace must be internal.",
			messageFormat: "Identifier '{0}' is declared in '{1}' namespace, and thus must be declared internal.",
			category: "Security",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Identifiers declared in Internal namespace must be internal.");

		private static readonly DiagnosticDescriptor DoNotAccessPrivateFieldsOfAnotherObjectDirectly = new DiagnosticDescriptor(
			id: DoNotAccessPrivateFieldsOfAnotherObjectDirectlyId,
			title: "Do not access private fields of another object directly.",
			messageFormat: "Private field '{0}' should not be accessed directly.",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not access private fields of another object directly.");

		private static readonly DiagnosticDescriptor DoNotCallDisposeOnStaticReadonlyFields = new DiagnosticDescriptor(
			id: DoNotCallDisposeOnStaticReadonlyFieldsId,
			title: "Do not call Dispose() on static readonly fields.",
			messageFormat: "Field '{0}' is marked 'static readonly' and should not be disposed.",
			category: "Reliability",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not call Dispose() on static readonly fields.");

		private static readonly DiagnosticDescriptor DoNotCallTaskWaitToInvokeTask = new DiagnosticDescriptor(
			id: DoNotCallTaskWaitToInvokeTaskId,
			title: "Do not call Task.Wait() to invoke a Task.",
			messageFormat: "Calling Task.Wait() blocks current thread and is not recommended. Use await instead.",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not call Task.Wait() to invoke a Task. Use await instead.");

		private static readonly DiagnosticDescriptor DoNotAccessTaskResultToInvokeTask = new DiagnosticDescriptor(
			id: DoNotAccessTaskResultToInvokeTaskId,
			title: "Do not access Task<>.Result to invoke a Task.",
			messageFormat: "Accessing Task<>.Result blocks current thread and is not recommended. Use await instead.",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not access Task<>.Result to invoke a Task. Use await instead.");

		private static readonly DiagnosticDescriptor TupleElementNamesMustBeInPascalCase = new DiagnosticDescriptor(
			id: TupleElementNamesMustBeInPascalCaseId,
			title: "Tuple element names must be in Pascal case.",
			messageFormat: "'{0}' is not a proper name of a tuple element. Change it to PascalCase.",
			category: "Code Style",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Tuple element names must be in Pascal case.");

		private static readonly DiagnosticDescriptor NotUsingOverloadWithCancellationToken = new DiagnosticDescriptor(
			id: NotUsingOverloadWithCancellationTokenId,
			title: "Not using overload with CancellationToken.",
			messageFormat: "This method has an overload that accepts CancellationToken.",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Not using overload with CancellationToken.");

		private static readonly DiagnosticDescriptor VarInferredTypeIsObsolete = new DiagnosticDescriptor(
			id: VarInferredTypeIsObsoleteId,
			title: "Inferred type is obsolete.",
			messageFormat: "'{0}' is obsolete{1}",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Inferred type is obsolete.");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
			get {
				return ImmutableArray.Create(
					NoAwaitInsideLoop,
					DontReturnTaskIfMethodDisposesObject,
					IdentifiersInInternalNamespaceMustBeInternal,
					DoNotAccessPrivateFieldsOfAnotherObjectDirectly,
					DoNotCallDisposeOnStaticReadonlyFields,
					DoNotCallTaskWaitToInvokeTask,
					DoNotAccessTaskResultToInvokeTask,
					TupleElementNamesMustBeInPascalCase,
					NotUsingOverloadWithCancellationToken,
					VarInferredTypeIsObsolete
				);
			}
		}

		public override void Initialize(AnalysisContext context) {
			if (context is null) {
				throw new ArgumentNullException(nameof(context));
			}

			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeAwaitExpression, SyntaxKind.AwaitExpression);
			context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
			context.RegisterSymbolAction(AnalyzeNamedTypeDeclaration, SymbolKind.NamedType);
			context.RegisterSyntaxNodeAction(AnalyzeMemberAccessExpression, SyntaxKind.SimpleMemberAccessExpression);
			context.RegisterSyntaxNodeAction(AnalyzeTupleTypes, SyntaxKind.TupleType);
			context.RegisterSyntaxNodeAction(AnalyzeInvocations, SyntaxKind.InvocationExpression);
			context.RegisterSyntaxNodeAction(AnalyzeVariableDeclarations, SyntaxKind.VariableDeclaration);
		}

		private static void AnalyzeAwaitExpression(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is AwaitExpressionSyntax awaitExpressionSyntax) {
					SyntaxNode loopNode = awaitExpressionSyntax.Ancestors().FirstOrDefault(ancestor => {
						SyntaxKind kind = ancestor.Kind();
						return kind == SyntaxKind.ForStatement
							|| kind == SyntaxKind.ForEachStatement
							|| kind == SyntaxKind.WhileStatement
							|| kind == SyntaxKind.DoStatement
							|| kind == SyntaxKind.MethodDeclaration
							|| kind == SyntaxKind.ParenthesizedLambdaExpression
							|| kind == SyntaxKind.SimpleLambdaExpression
							|| kind == SyntaxKind.AnonymousMethodExpression;
					});
					if (loopNode is { }
						&& loopNode.Kind() is SyntaxKind kind
						&& (kind == SyntaxKind.ForStatement
							|| kind == SyntaxKind.ForEachStatement
							|| kind == SyntaxKind.WhileStatement
							|| kind == SyntaxKind.DoStatement)) {
						var diagnostic = Diagnostic.Create(NoAwaitInsideLoop, awaitExpressionSyntax.GetLocation(), loopNode.Kind() switch
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
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is UsingStatementSyntax usingStatementSyntax) {
					SyntaxNode methodNode = usingStatementSyntax.Ancestors().FirstOrDefault(ancestor => {
						SyntaxKind kind = ancestor.Kind();
						return kind == SyntaxKind.MethodDeclaration
							|| kind == SyntaxKind.ParenthesizedLambdaExpression
							|| kind == SyntaxKind.SimpleLambdaExpression
							|| kind == SyntaxKind.AnonymousMethodExpression;
					});
					switch (methodNode) {
						case MethodDeclarationSyntax { ReturnType: { } returnType } methodDeclarationSyntax:
							if (context.SemanticModel.GetSymbolInfo(returnType, context.CancellationToken).Symbol is INamedTypeSymbol namedTypeSymbol
								&& namedTypeSymbol.ToString() is string fullName
								&& fullName.StartsWith("System.Threading.Tasks.Task", StringComparison.Ordinal)
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
						case AnonymousMethodExpressionSyntax anonymousMethodExpressionSyntax:
							// TODO: handle anonymous method expression
							break;
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeNamedTypeDeclaration(SymbolAnalysisContext context) {
			try {
				if (context.Symbol is INamedTypeSymbol { ContainingNamespace: { } containingNamespace }) {
					if (IsInternalNamespace(containingNamespace, out string fullNamespace)) {
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
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeMemberAccessExpression(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is MemberAccessExpressionSyntax memberAccessExpressionSyntax) {
					if (context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax) is SymbolInfo memberSymbolInfo
						&& memberSymbolInfo.Symbol is ISymbol member
						&& member.Kind == SymbolKind.Field
						&& !member.IsStatic
						&& member.DeclaredAccessibility == Accessibility.Private
						&& memberAccessExpressionSyntax.Expression.ToString() != "this") {
						var diagnostic = Diagnostic.Create(DoNotAccessPrivateFieldsOfAnotherObjectDirectly, memberAccessExpressionSyntax.Name.GetLocation(), memberAccessExpressionSyntax.Name.Identifier.ValueText);
						context.ReportDiagnostic(diagnostic);
					} else if (context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Expression) is SymbolInfo objSymbolInfo
						&& objSymbolInfo.Symbol is ISymbol obj) {
						if (memberAccessExpressionSyntax.Name.Identifier.ValueText == "Dispose"
							&& context.Node.Parent is InvocationExpressionSyntax disposeInvocationSyntax
							&& obj.Kind == SymbolKind.Field
							&& obj.IsStatic
							&& IsSymbolReadOnly(obj)) {
							var diagnostic = Diagnostic.Create(DoNotCallDisposeOnStaticReadonlyFields, disposeInvocationSyntax.GetLocation(), obj.Name);
							context.ReportDiagnostic(diagnostic);
						} else if (memberAccessExpressionSyntax.Name.Identifier.ValueText == "Wait"
							&& context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Name) is SymbolInfo waitSymbolInfo
							&& waitSymbolInfo.Symbol.ContainingType.ToString().StartsWith("System.Threading.Tasks.Task", StringComparison.Ordinal)
							&& context.Node.Parent is InvocationExpressionSyntax waitInvocationSyntax) {
							var diagnostic = Diagnostic.Create(DoNotCallTaskWaitToInvokeTask, waitInvocationSyntax.GetLocation());
							context.ReportDiagnostic(diagnostic);
						} else if (memberAccessExpressionSyntax.Name.Identifier.ValueText == "Result"
							&& context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Name) is SymbolInfo resultSymbolInfo
							&& resultSymbolInfo.Symbol.ContainingType.ToString().StartsWith("System.Threading.Tasks.Task", StringComparison.Ordinal)
							&& !resultSymbolInfo.Symbol.IsStatic) {
							var diagnostic = Diagnostic.Create(DoNotAccessTaskResultToInvokeTask, memberAccessExpressionSyntax.GetLocation());
							context.ReportDiagnostic(diagnostic);
						}
					} 
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeTupleTypes(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is TupleTypeSyntax { Elements: { } tupleElements } tupleTypeSyntax) {
					foreach (TupleElementSyntax tupleElementSyntax in tupleElements) {
						if (tupleElementSyntax is { Identifier: { ValueText: string elementName } identifier }
							&& !IsInPascalCase(elementName)) {
							var diagnostic = Diagnostic.Create(TupleElementNamesMustBeInPascalCase, tupleElementSyntax.GetLocation(), elementName);
							context.ReportDiagnostic(diagnostic);
						}
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeInvocations(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is InvocationExpressionSyntax { Expression: { } expression, ArgumentList: { Arguments: { } invocationArguments } } invocationExpressionSyntax) {
					if (context.SemanticModel.GetSymbolInfo(expression) is { Symbol: IMethodSymbol { Parameters: { } methodParameters, ReturnType: { } methodReturnType } }) {
						var methodDeclaration = invocationExpressionSyntax.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
						if (methodDeclaration is { ParameterList: { Parameters: { } callerParameters } }
							&& callerParameters.Any(callerParameter => callerParameter?.Type?.ToString() is string type && (type == "CancellationToken" || type == "System.Threading.CancellationToken"))) {
							if (methodParameters.Length == invocationArguments.Count) {
								foreach (IMethodSymbol overloadSymbol in context.SemanticModel.GetMemberGroup(invocationExpressionSyntax.Expression).OfType<IMethodSymbol>()) {
									if (overloadSymbol.Parameters.Length == methodParameters.Length + 1
										&& overloadSymbol.ReturnType.Equals(methodReturnType)
										&& overloadSymbol.Parameters.Last()?.Type?.ToString() is string type
										&& (type == "CancellationToken" || type == "System.Threading.CancellationToken")) {
										bool signatureMatches = true;
										for (int i = 0; i < methodParameters.Length; i++) {
											if (!overloadSymbol.Parameters[i].Type.Equals(methodParameters[i].Type)) {
												signatureMatches = false;
											}
										}
										if (signatureMatches) {
											var diagnostic = Diagnostic.Create(NotUsingOverloadWithCancellationToken, invocationExpressionSyntax.GetLocation());
											context.ReportDiagnostic(diagnostic);
										}
									}
								}
							} else if (methodParameters.Length == invocationExpressionSyntax.ArgumentList.Arguments.Count + 1
								&& methodParameters.Last()?.Type?.ToString() is string type
								&& (type == "CancellationToken" || type == "System.Threading.CancellationToken")) {
								var diagnostic = Diagnostic.Create(NotUsingOverloadWithCancellationToken, invocationExpressionSyntax.GetLocation());
								context.ReportDiagnostic(diagnostic);
							}
						}
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeVariableDeclarations(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is VariableDeclarationSyntax { Type: { IsVar: true } varNode } varDeclarationSyntax
					&& context.SemanticModel.GetTypeInfo(varNode, context.CancellationToken) is { Type: INamedTypeSymbol typeSymbol }
					&& typeSymbol.GetAttributes() is { } attributes
					&& attributes.FirstOrDefault(attribute => attribute.AttributeClass.ToString() == "System.ObsoleteAttribute") is { ConstructorArguments: { } attributeArguments }) {
					if (attributeArguments.Length > 0
						&& attributeArguments[0].Value is { } messageArg
						&& messageArg.ToString() is string message) {
						if (attributeArguments.Length > 1
							&& attributeArguments[1].Value is { } errorArg
							&& errorArg is true) {
							var diagnostic = Diagnostic.Create(
								id: VarInferredTypeIsObsoleteId,
								category: VarInferredTypeIsObsolete.Category,
								message: string.Format(CultureInfo.InvariantCulture, VarInferredTypeIsObsolete.MessageFormat.ToString(CultureInfo.InvariantCulture), typeSymbol.Name, $": '{message}'"),
								severity: DiagnosticSeverity.Error,
								defaultSeverity: VarInferredTypeIsObsolete.DefaultSeverity,
								isEnabledByDefault: VarInferredTypeIsObsolete.IsEnabledByDefault,
								warningLevel: 0,
								location: varNode.GetLocation());
							context.ReportDiagnostic(diagnostic);
						} else {
							var diagnostic = Diagnostic.Create(VarInferredTypeIsObsolete, varNode.GetLocation(), typeSymbol.Name, $": '{message}'");
							context.ReportDiagnostic(diagnostic);
						}
					} else {
						var diagnostic = Diagnostic.Create(VarInferredTypeIsObsolete, varNode.GetLocation(), typeSymbol.Name, ".");
						context.ReportDiagnostic(diagnostic);
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		#region Helpers
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

		private static bool IsSymbolReadOnly(ISymbol symbol) {
			PropertyInfo prop = symbol.GetType().GetRuntimeProperty("IsReadOnly");
			return prop.GetValue(symbol) is true;
		}

		internal static bool IsInPascalCase(string identifierName) {
			return identifierName.Length > 0
				&& char.IsUpper(identifierName[0])
				&& identifierName.Skip(1).All(c => char.IsLetterOrDigit(c));
		}

		internal static bool IsInCamelCase(string identifierName) {
			return identifierName.Length > 0
				&& char.IsLower(identifierName[0])
				&& identifierName.Skip(1).All(c => char.IsLetterOrDigit(c));
		}

		internal static string ToPascalCase(string camelCaseIdentifierName) {
			if (!IsInCamelCase(camelCaseIdentifierName)) throw new ArgumentException("Identifier name is not in camel case.", nameof(camelCaseIdentifierName));
			return $"{char.ToUpper(camelCaseIdentifierName[0])}{camelCaseIdentifierName.Substring(1)}";
		}
		#endregion
	}
}
