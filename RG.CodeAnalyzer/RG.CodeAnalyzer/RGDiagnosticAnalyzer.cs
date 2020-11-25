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
		public const string NO_AWAIT_INSIDE_LOOP_ID = "RG0001";
		public const string DONT_RETURN_TASK_IF_METHOD_DISPOSES_OBJECT_ID = "RG0002";
		public const string IDENTIFIERS_IN_INTERNAL_NAMESPACE_MUST_BE_INTERNAL_ID = "RG0003";
		public const string DO_NOT_ACCESS_PRIVATE_FIELDS_OF_ANOTHER_OBJECT_DIRECTLY_ID = "RG0004";
		public const string DO_NOT_CALL_DISPOSE_ON_STATIC_READONLY_FIELDS_ID = "RG0005";
		public const string DO_NOT_CALL_TASK_WAIT_TO_INVOKE_TASK_ID = "RG0006";
		public const string DO_NOT_ACCESS_TASK_RESULT_TO_INVOKE_TASK_ID = "RG0007";
		public const string TUPLE_ELEMENT_NAMES_MUST_BE_IN_PASCAL_CASE_ID = "RG0008";
		public const string NOT_USING_OVERLOAD_WITH_CANCELLATION_TOKEN_ID = "RG0009";
		public const string VAR_INFERRED_TYPE_IS_OBSOLETE_ID = "RG0010";
		public const string INTERFACES_SHOULDNT_DERIVE_FROM_IDISPOSABLE_ID = "RG0011";
		public const string UNRESOLVED_TASK_ID = "RG0012";
		public const string WITH_SHOULDNT_BE_USED_OUTSIDE_ITS_RECORD_DECLARATION_ID = "RG0013";
		public const string DO_NOT_PARSE_USING_CONVERT_ID = "RG0014";

		private static readonly DiagnosticDescriptor NO_AWAIT_INSIDE_LOOP = new(
			id: NO_AWAIT_INSIDE_LOOP_ID,
			title: "Do not await inside a loop",
			messageFormat: "Asynchronous operation awaited inside {0}",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not await inside a loop. Perform asynchronous operations in a batch instead.");

		private static readonly DiagnosticDescriptor DONT_RETURN_TASK_IF_METHOD_DISPOSES_OBJECT = new(
			id: DONT_RETURN_TASK_IF_METHOD_DISPOSES_OBJECT_ID,
			title: "Do not return Task from a method that disposes object",
			messageFormat: "Method '{0}' disposes an object and shouldn't return Task",
			category: "Reliability",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not return Task from a method that disposes an object. Mark method as async instead.");

		private static readonly DiagnosticDescriptor IDENTIFIERS_IN_INTERNAL_NAMESPACE_MUST_BE_INTERNAL = new(
			id: IDENTIFIERS_IN_INTERNAL_NAMESPACE_MUST_BE_INTERNAL_ID,
			title: "Identifiers declared in Internal namespace must be internal",
			messageFormat: "Identifier '{0}' is declared in '{1}' namespace, and thus must be declared internal",
			category: "Security",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Identifiers declared in Internal namespace must be internal.");

		private static readonly DiagnosticDescriptor DO_NOT_ACCESS_PRIVATE_FIELDS_OF_ANOTHER_OBJECT_DIRECTLY = new(
			id: DO_NOT_ACCESS_PRIVATE_FIELDS_OF_ANOTHER_OBJECT_DIRECTLY_ID,
			title: "Do not access private fields of another object directly",
			messageFormat: "Private field '{0}' should not be accessed directly",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not access private fields of another object directly.");

		private static readonly DiagnosticDescriptor DO_NOT_CALL_DISPOSE_ON_STATIC_READONLY_FIELDS = new(
			id: DO_NOT_CALL_DISPOSE_ON_STATIC_READONLY_FIELDS_ID,
			title: "Do not call Dispose() on static readonly fields",
			messageFormat: "Field '{0}' is marked 'static readonly' and should not be disposed",
			category: "Reliability",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not call Dispose() on static readonly fields.");

		private static readonly DiagnosticDescriptor DO_NOT_CALL_TASK_WAIT_TO_INVOKE_TASK = new(
			id: DO_NOT_CALL_TASK_WAIT_TO_INVOKE_TASK_ID,
			title: "Do not call Task.Wait() to invoke a Task",
			messageFormat: "Calling Task.Wait() blocks current thread and is not recommended; Use await instead",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not call Task.Wait() to invoke a Task. Use await instead.");

		private static readonly DiagnosticDescriptor DO_NOT_ACCESS_TASK_RESULT_TO_INVOKE_TASK = new(
			id: DO_NOT_ACCESS_TASK_RESULT_TO_INVOKE_TASK_ID,
			title: "Do not access Task<>.Result to invoke a Task",
			messageFormat: "Accessing Task<>.Result blocks current thread and is not recommended; Use await instead",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not access Task<>.Result to invoke a Task. Use await instead.");

		private static readonly DiagnosticDescriptor TUPLE_ELEMENT_NAMES_MUST_BE_IN_PASCAL_CASE = new(
			id: TUPLE_ELEMENT_NAMES_MUST_BE_IN_PASCAL_CASE_ID,
			title: "Tuple element names must be in Pascal case",
			messageFormat: "'{0}' is not a proper name of a tuple element; Change it to PascalCase",
			category: "Code Style",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Tuple element names must be in Pascal case.");

		private static readonly DiagnosticDescriptor NOT_USING_OVERLOAD_WITH_CANCELLATION_TOKEN = new(
			id: NOT_USING_OVERLOAD_WITH_CANCELLATION_TOKEN_ID,
			title: "Not using overload with CancellationToken",
			messageFormat: "This method has an overload that accepts CancellationToken",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Not using overload with CancellationToken.");

		private static readonly DiagnosticDescriptor VAR_INFERRED_TYPE_IS_OBSOLETE = new(
			id: VAR_INFERRED_TYPE_IS_OBSOLETE_ID,
			title: "Inferred type is obsolete",
			messageFormat: "'{0}' is obsolete{1}",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Inferred type is obsolete.");

		private static readonly DiagnosticDescriptor INTERFACES_SHOULDNT_DERIVE_FROM_IDISPOSABLE = new(
			id: INTERFACES_SHOULDNT_DERIVE_FROM_IDISPOSABLE_ID,
			title: "Interfaces shouldn't derive from IDisposable",
			messageFormat: "'{0}' derives from IDisposable",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Interfaces shouldn't derive from IDisposable.");

		private static readonly DiagnosticDescriptor UNRESOLVED_TASK = new(
			id: UNRESOLVED_TASK_ID,
			title: "Task is unresolved",
			messageFormat: "Unresolved {0}",
			category: "Maintainability",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Task is unresolved.");

		private static readonly DiagnosticDescriptor WITH_SHOULDNT_BE_USED_OUTSIDE_ITS_RECORD_DECLARATION = new(
			id: WITH_SHOULDNT_BE_USED_OUTSIDE_ITS_RECORD_DECLARATION_ID,
			title: "'with' shouldn't be used outside its record declaration",
			messageFormat: "'with' used outside '{0}'",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "'with' shouldn't be used outside its record declaration.");

		private static readonly DiagnosticDescriptor DO_NOT_PARSE_USING_CONVERT = new(
			id: DO_NOT_PARSE_USING_CONVERT_ID,
			title: "Do not parse using Convert",
			messageFormat: "Parsing '{0}' using 'Convert.{1}'",
			category: "Reliability",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Do not parse using Convert.");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			NO_AWAIT_INSIDE_LOOP,
			DONT_RETURN_TASK_IF_METHOD_DISPOSES_OBJECT,
			IDENTIFIERS_IN_INTERNAL_NAMESPACE_MUST_BE_INTERNAL,
			DO_NOT_ACCESS_PRIVATE_FIELDS_OF_ANOTHER_OBJECT_DIRECTLY,
			DO_NOT_CALL_DISPOSE_ON_STATIC_READONLY_FIELDS,
			DO_NOT_CALL_TASK_WAIT_TO_INVOKE_TASK,
			DO_NOT_ACCESS_TASK_RESULT_TO_INVOKE_TASK,
			TUPLE_ELEMENT_NAMES_MUST_BE_IN_PASCAL_CASE,
			NOT_USING_OVERLOAD_WITH_CANCELLATION_TOKEN,
			VAR_INFERRED_TYPE_IS_OBSOLETE,
			INTERFACES_SHOULDNT_DERIVE_FROM_IDISPOSABLE,
			UNRESOLVED_TASK,
			WITH_SHOULDNT_BE_USED_OUTSIDE_ITS_RECORD_DECLARATION,
			DO_NOT_PARSE_USING_CONVERT
		);

		public override void Initialize(AnalysisContext context) {
			if (context is null) return;

			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();

			// NO_AWAIT_INSIDE_LOOP
			context.RegisterSyntaxNodeAction(AnalyzeAwaitExpression, SyntaxKind.AwaitExpression);

			// DONT_RETURN_TASK_IF_METHOD_DISPOSES_OBJECT
			context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);

			// DONT_RETURN_TASK_IF_METHOD_DISPOSES_OBJECT
			context.RegisterSyntaxNodeAction(AnalyzeUsingDeclarationStatement, SyntaxKind.LocalDeclarationStatement);

			// IDENTIFIERS_IN_INTERNAL_NAMESPACE_MUST_BE_INTERNAL
			context.RegisterSymbolAction(AnalyzeNamedTypeDeclaration, SymbolKind.NamedType);

			// DO_NOT_ACCESS_PRIVATE_FIELDS_OF_ANOTHER_OBJECT_DIRECTLY
			// DO_NOT_CALL_DISPOSE_ON_STATIC_READONLY_FIELDS
			// DO_NOT_CALL_TASK_WAIT_TO_INVOKE_TASK
			// DO_NOT_ACCESS_TASK_RESULT_TO_INVOKE_TASK
			context.RegisterSyntaxNodeAction(AnalyzeMemberAccessExpression, SyntaxKind.SimpleMemberAccessExpression);

			// TUPLE_ELEMENT_NAMES_MUST_BE_IN_PASCAL_CASE
			context.RegisterSyntaxNodeAction(AnalyzeTupleTypes, SyntaxKind.TupleType);

			// NOT_USING_OVERLOAD_WITH_CANCELLATION_TOKEN
			// DO_NOT_PARSE_USING_CONVERT
			context.RegisterSyntaxNodeAction(AnalyzeInvocations, SyntaxKind.InvocationExpression);

			// VAR_INFERRED_TYPE_IS_OBSOLETE
			context.RegisterSyntaxNodeAction(AnalyzeVariableDeclarations, SyntaxKind.VariableDeclaration);

			// INTERFACES_SHOULDNT_DERIVE_FROM_IDISPOSABLE
			context.RegisterSyntaxNodeAction(AnalyzeInterfaceDeclarations, SyntaxKind.InterfaceDeclaration);

			// UNRESOLVED_TASK
			context.RegisterSyntaxTreeAction(AnalyzeSingleLineComments);

			// WITH_SHOULDNT_BE_USED_OUTSIDE_ITS_RECORD_DECLARATION
			context.RegisterSyntaxNodeAction(AnalyzeWithExpressions, SyntaxKind.WithExpression);
		}

		private static void AnalyzeAwaitExpression(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is AwaitExpressionSyntax awaitExpressionSyntax) {
					SyntaxNode? loopNode = awaitExpressionSyntax.Ancestors().FirstOrDefault(ancestor => {
						return ancestor.Kind()
							is SyntaxKind.ForStatement
							or SyntaxKind.ForEachStatement
							or SyntaxKind.WhileStatement
							or SyntaxKind.DoStatement
							or SyntaxKind.MethodDeclaration
							or SyntaxKind.ParenthesizedLambdaExpression
							or SyntaxKind.SimpleLambdaExpression
							or SyntaxKind.AnonymousMethodExpression;
					});
					if (loopNode is ForEachStatementSyntax foreachStatement) {
						if (foreachStatement.Expression is not AwaitExpressionSyntax foreachAwaitExpression
							|| awaitExpressionSyntax != foreachAwaitExpression) {
							Diagnostic diagnostic = Diagnostic.Create(NO_AWAIT_INSIDE_LOOP, awaitExpressionSyntax.GetLocation(), "foreach loop");
							context.ReportDiagnostic(diagnostic);
						}
					} else if (loopNode is { }
						&& loopNode.Kind()
							is SyntaxKind.ForStatement
							or SyntaxKind.ForEachStatement
							or SyntaxKind.WhileStatement
							or SyntaxKind.DoStatement) {
						Diagnostic diagnostic = Diagnostic.Create(NO_AWAIT_INSIDE_LOOP, awaitExpressionSyntax.GetLocation(), loopNode.Kind() switch {
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
						return ancestor.Kind()
							is SyntaxKind.MethodDeclaration
							or SyntaxKind.ParenthesizedLambdaExpression
							or SyntaxKind.SimpleLambdaExpression
							or SyntaxKind.AnonymousMethodExpression;
					});
					switch (methodNode) {
						case MethodDeclarationSyntax { ReturnType: { } returnType } methodDeclarationSyntax:
							if (context.SemanticModel.GetSymbolInfo(returnType, context.CancellationToken).Symbol is INamedTypeSymbol namedTypeSymbol
								&& namedTypeSymbol.ToString() is string fullName
								&& fullName.StartsWith("System.Threading.Tasks.Task", StringComparison.Ordinal)
								&& !methodDeclarationSyntax.Modifiers.Any(SyntaxKind.AsyncKeyword)) {
								Diagnostic diagnostic = Diagnostic.Create(DONT_RETURN_TASK_IF_METHOD_DISPOSES_OBJECT, methodDeclarationSyntax.GetLocation(), methodDeclarationSyntax.Identifier.ValueText);
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

		private static void AnalyzeUsingDeclarationStatement(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is LocalDeclarationStatementSyntax { UsingKeyword: { } usingKeyword } localDeclarationStatementSyntax
					&& usingKeyword.Kind() == SyntaxKind.UsingKeyword){
					SyntaxNode methodNode = localDeclarationStatementSyntax.Ancestors().FirstOrDefault(ancestor => {
						return ancestor.Kind()
							is SyntaxKind.MethodDeclaration
							or SyntaxKind.ParenthesizedLambdaExpression
							or SyntaxKind.SimpleLambdaExpression
							or SyntaxKind.AnonymousMethodExpression;
					});
					switch (methodNode) {
						case MethodDeclarationSyntax { ReturnType: { } returnType } methodDeclarationSyntax:
							if (context.SemanticModel.GetSymbolInfo(returnType, context.CancellationToken).Symbol is INamedTypeSymbol namedTypeSymbol
								&& namedTypeSymbol.ToString() is string fullName
								&& fullName.StartsWith("System.Threading.Tasks.Task", StringComparison.Ordinal)
								&& !methodDeclarationSyntax.Modifiers.Any(SyntaxKind.AsyncKeyword)) {
								Diagnostic diagnostic = Diagnostic.Create(DONT_RETURN_TASK_IF_METHOD_DISPOSES_OBJECT, methodDeclarationSyntax.GetLocation(), methodDeclarationSyntax.Identifier.ValueText);
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
								Diagnostic diagnostic = Diagnostic.Create(IDENTIFIERS_IN_INTERNAL_NAMESPACE_MUST_BE_INTERNAL, context.Symbol.DeclaringSyntaxReferences[0].GetSyntax().GetLocation(), context.Symbol.Name, fullNamespace);
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
						Diagnostic diagnostic = Diagnostic.Create(DO_NOT_ACCESS_PRIVATE_FIELDS_OF_ANOTHER_OBJECT_DIRECTLY, memberAccessExpressionSyntax.Name.GetLocation(), memberAccessExpressionSyntax.Name.Identifier.ValueText);
						context.ReportDiagnostic(diagnostic);
					} else if (context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Expression) is SymbolInfo objSymbolInfo
						&& objSymbolInfo.Symbol is ISymbol obj) {
						switch (memberAccessExpressionSyntax.Name.Identifier.ValueText) {
							case "Dispose":
								if (context.Node.Parent is InvocationExpressionSyntax disposeInvocationSyntax
									&& obj.Kind == SymbolKind.Field
									&& obj.IsStatic
									&& IsSymbolReadOnly(obj)) {
									Diagnostic diagnostic = Diagnostic.Create(DO_NOT_CALL_DISPOSE_ON_STATIC_READONLY_FIELDS, disposeInvocationSyntax.GetLocation(), obj.Name);
									context.ReportDiagnostic(diagnostic);
								}
								break;
							case "Wait":
								if (context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Name) is SymbolInfo waitSymbolInfo
									&& waitSymbolInfo.Symbol is { } waitSymbol
									&& waitSymbol.ContainingType.ToString().StartsWith("System.Threading.Tasks.Task", StringComparison.Ordinal)
									&& context.Node.Parent is InvocationExpressionSyntax waitInvocationSyntax) {
									Diagnostic diagnostic = Diagnostic.Create(DO_NOT_CALL_TASK_WAIT_TO_INVOKE_TASK, waitInvocationSyntax.GetLocation());
									context.ReportDiagnostic(diagnostic);
								}
								break;
							case "Result":
								if (context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Name) is SymbolInfo resultSymbolInfo
									&& resultSymbolInfo.Symbol is { } resultSymbol
									&& resultSymbol.ContainingType.ToString().StartsWith("System.Threading.Tasks.Task", StringComparison.Ordinal)
									&& !resultSymbol.IsStatic) {
									Diagnostic diagnostic = Diagnostic.Create(DO_NOT_ACCESS_TASK_RESULT_TO_INVOKE_TASK, memberAccessExpressionSyntax.GetLocation());
									context.ReportDiagnostic(diagnostic);
								}
								break;
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
					foreach (TupleElementSyntax tupleElementSyntax in tupleElements.ToImmutableArray()) {
						if (tupleElementSyntax is { Identifier: { ValueText: string elementName } identifier }
							&& !IsInPascalCase(elementName)) {
							Diagnostic diagnostic = Diagnostic.Create(TUPLE_ELEMENT_NAMES_MUST_BE_IN_PASCAL_CASE, tupleElementSyntax.GetLocation(), elementName);
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
					if (expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier: { ValueText: nameof(Convert) } }, Name: IdentifierNameSyntax methodName }) {
						if (context.SemanticModel.GetTypeInfo(invocationArguments[0].Expression) is { Type: INamedTypeSymbol argumentTypeSymbol }
							&& argumentTypeSymbol.ToString() == "string") {
							string? typeName = methodName.Identifier.ValueText switch {
								nameof(Convert.ToBoolean) => "bool",
								nameof(Convert.ToByte) => "byte",
								nameof(Convert.ToChar) => "char",
								nameof(Convert.ToDateTime) => "DateTime",
								nameof(Convert.ToDecimal) => "decimal",
								nameof(Convert.ToDouble) => "double",
								nameof(Convert.ToInt16) => "short",
								nameof(Convert.ToInt32) => "int",
								nameof(Convert.ToInt64) => "long",
								nameof(Convert.ToSByte) => "sbyte",
								nameof(Convert.ToSingle) => "float",
								nameof(Convert.ToUInt16) => "ushort",
								nameof(Convert.ToUInt32) => "uint",
								nameof(Convert.ToUInt64) => "ulong",
								_ => null
							};
							if (typeName is not null) {
								Diagnostic diagnostic = Diagnostic.Create(DO_NOT_PARSE_USING_CONVERT, invocationExpressionSyntax.GetLocation(), typeName, methodName.Identifier.ValueText);
								context.ReportDiagnostic(diagnostic);
							}
						}
					} else if (context.SemanticModel.GetSymbolInfo(expression) is { Symbol: IMethodSymbol { Parameters: { } methodParameters, ReturnType: { } methodReturnType } }) {
						MethodDeclarationSyntax? methodDeclaration = invocationExpressionSyntax.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
						if (methodDeclaration is { ParameterList: { Parameters: { } callerParameters } }
							&& callerParameters.Any(callerParameter => callerParameter.Type?.ToString() is "CancellationToken" or "System.Threading.CancellationToken")) {
							if (methodParameters.Length == invocationArguments.Count) {
								foreach (IMethodSymbol overloadSymbol in context.SemanticModel.GetMemberGroup(invocationExpressionSyntax.Expression).OfType<IMethodSymbol>()) {
									if (overloadSymbol.Parameters.Length == methodParameters.Length + 1
										&& SymbolEqualityComparer.Default.Equals(overloadSymbol.ReturnType, methodReturnType)
										&& overloadSymbol.Parameters.LastOrDefault()?.Type.ToString() is string type
										&& (type is "CancellationToken" or "System.Threading.CancellationToken")) {
										bool signatureMatches = true;
										for (int i = 0; i < methodParameters.Length; i++) {
											if (!SymbolEqualityComparer.Default.Equals(overloadSymbol.Parameters[i].Type, methodParameters[i].Type)) {
												signatureMatches = false;
											}
										}
										if (signatureMatches) {
											Diagnostic diagnostic = Diagnostic.Create(NOT_USING_OVERLOAD_WITH_CANCELLATION_TOKEN, invocationExpressionSyntax.GetLocation());
											context.ReportDiagnostic(diagnostic);
										}
									}
								}
							} else if (methodParameters.Length == invocationExpressionSyntax.ArgumentList.Arguments.Count + 1
								&& methodParameters.Last().Type.ToString() is "CancellationToken" or "System.Threading.CancellationToken") {
								Diagnostic diagnostic = Diagnostic.Create(NOT_USING_OVERLOAD_WITH_CANCELLATION_TOKEN, invocationExpressionSyntax.GetLocation());
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
					&& attributes.FirstOrDefault(attribute => attribute.AttributeClass?.ToString() == "System.ObsoleteAttribute") is { ConstructorArguments: { } attributeArguments }) {
					if (attributeArguments.Length > 0
						&& attributeArguments[0].Value is { } messageArg
						&& messageArg.ToString() is string message) {
						if (attributeArguments.Length > 1
							&& attributeArguments[1].Value is { } errorArg
							&& errorArg is true) {
							Diagnostic diagnostic = Diagnostic.Create(
								id: VAR_INFERRED_TYPE_IS_OBSOLETE_ID,
								category: VAR_INFERRED_TYPE_IS_OBSOLETE.Category,
								message: string.Format(CultureInfo.InvariantCulture, VAR_INFERRED_TYPE_IS_OBSOLETE.MessageFormat.ToString(CultureInfo.InvariantCulture), typeSymbol.Name, $": '{message}'"),
								severity: DiagnosticSeverity.Error,
								defaultSeverity: VAR_INFERRED_TYPE_IS_OBSOLETE.DefaultSeverity,
								isEnabledByDefault: VAR_INFERRED_TYPE_IS_OBSOLETE.IsEnabledByDefault,
								warningLevel: 0,
								location: varNode.GetLocation());
							context.ReportDiagnostic(diagnostic);
						} else {
							Diagnostic diagnostic = Diagnostic.Create(VAR_INFERRED_TYPE_IS_OBSOLETE, varNode.GetLocation(), typeSymbol.Name, $": '{message}'");
							context.ReportDiagnostic(diagnostic);
						}
					} else {
						Diagnostic diagnostic = Diagnostic.Create(VAR_INFERRED_TYPE_IS_OBSOLETE, varNode.GetLocation(), typeSymbol.Name, ".");
						context.ReportDiagnostic(diagnostic);
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeInterfaceDeclarations(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is InterfaceDeclarationSyntax { BaseList: { Types: var baseTypes } } declaration) {
					if (baseTypes.Any(baseType => baseType.Type.ToString() == "IDisposable")) {
						Diagnostic diagnostic = Diagnostic.Create(INTERFACES_SHOULDNT_DERIVE_FROM_IDISPOSABLE, declaration.GetLocation(), declaration.Identifier.ValueText);
						context.ReportDiagnostic(diagnostic);
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeSingleLineComments(SyntaxTreeAnalysisContext context) {
			try {
				SyntaxNode root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);
				foreach (SyntaxTrivia singleLineCommentTrivia in from trivia in root.DescendantTrivia()
																 where trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
																 select trivia) {
					if (singleLineCommentTrivia.ToString() is string commentText) {
						if (commentText.StartsWith("// TODO", StringComparison.CurrentCulture)
							|| commentText.StartsWith("//TODO", StringComparison.CurrentCulture)
							|| commentText.StartsWith("// HACK", StringComparison.CurrentCulture)
							|| commentText.StartsWith("//HACK", StringComparison.CurrentCulture)
							|| commentText.StartsWith("// FIXME", StringComparison.CurrentCulture)
							|| commentText.StartsWith("//FIXME", StringComparison.CurrentCulture)
							|| commentText.StartsWith("// UNDONE", StringComparison.CurrentCulture)
							|| commentText.StartsWith("//UNDONE", StringComparison.CurrentCulture)) {
							Diagnostic diagnostic = Diagnostic.Create(UNRESOLVED_TASK, singleLineCommentTrivia.GetLocation(), commentText.Substring(2).TrimStart());
							context.ReportDiagnostic(diagnostic);
						}
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeWithExpressions(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is WithExpressionSyntax { Expression: { } expression, WithKeyword: var withKeyword } withExpression
					&& context.SemanticModel.GetTypeInfo(expression, context.CancellationToken) is { Type: INamedTypeSymbol { } namedTypeSymbol }) {
					if (withExpression.FirstAncestorOrSelf<RecordDeclarationSyntax>() is not { } recordDeclarationSyntax) {
						Diagnostic diagnostic = Diagnostic.Create(WITH_SHOULDNT_BE_USED_OUTSIDE_ITS_RECORD_DECLARATION, withKeyword.GetLocation(), namedTypeSymbol.ToString());
						context.ReportDiagnostic(diagnostic);
					} else if (context.SemanticModel.GetDeclaredSymbol(recordDeclarationSyntax)?.ToString() != namedTypeSymbol.ToString()) {
						Diagnostic diagnostic = Diagnostic.Create(WITH_SHOULDNT_BE_USED_OUTSIDE_ITS_RECORD_DECLARATION, withKeyword.GetLocation(), namedTypeSymbol.ToString());
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
			Type symbolType = symbol.GetType();
			PropertyInfo? prop = symbolType.GetRuntimeProperties().FirstOrDefault(rp => rp.Name.EndsWith("IsReadOnly", StringComparison.Ordinal));
			return prop?.GetValue(symbol) is true;
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
			return $"{char.ToUpper(camelCaseIdentifierName[0], CultureInfo.CurrentCulture)}{camelCaseIdentifierName.Substring(1)}";
		}
		#endregion
	}
}
