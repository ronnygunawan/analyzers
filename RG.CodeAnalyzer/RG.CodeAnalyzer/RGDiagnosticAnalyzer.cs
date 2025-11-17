using System;
using System.Collections.Generic;
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
		public const string RECORDS_SHOULD_NOT_CONTAIN_SET_ACCESSOR_ID = "RG0015";
		public const string RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_FIELD_ID = "RG0016";
		public const string RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_COLLECTION_ID = "RG0017";
		public const string RECORDS_SHOULD_NOT_CONTAIN_REFERENCE_TO_CLASS_OR_STRUCT_TYPE_ID = "RG0018";
		public const string REQUIRED_RECORD_PROPERTY_SHOULD_BE_INITIALIZED_ID = "RG0019";
		public const string VALUE_TYPE_RECORD_PROPERTY_SHOULD_BE_INITIALIZED_ID = "RG0020";
		public const string LOCAL_IS_READONLY_ID = "RG0021";
		public const string PARAMETER_IS_READONLY_ID = "RG0022";
		public const string REF_OR_OUT_PARAMETER_CANNOT_BE_READONLY_ID = "RG0023";
		public const string IN_ARGUMENT_SHOULD_BE_READONLY_ID = "RG0024";
		public const string CASTING_TO_AN_INCOMPATIBLE_ENUM_ID = "RG0025";
		public const string POSSIBLY_CASTING_TO_AN_INCOMPATIBLE_ENUM_MEMBER_SWAPPED_ID = "RG0026";
		public const string POSSIBLY_CASTING_TO_AN_INCOMPATIBLE_ENUM_DIFFERENT_MEMBER_NAME_ID = "RG0027";
		public const string PROTOBUF_MESSAGE_PROPERTIES_ARE_REQUIRED_ID = "RG0028";
		public const string PROTOBUF_MESSAGE_ONEOF_PROPERTY_ALREADY_INITIALIZED_ID = "RG0029";
		public const string ARGUMENT_MUST_BE_LOCKED_ID = "RG0030";
		public const string DO_NOT_USE_DYNAMIC_TYPE_ID = "RG0031";
		public const string STATIC_CLASS_WITH_EXTENSION_METHODS_SHOULD_HAVE_EXTENSIONS_SUFFIX_ID = "RG0032";
		public const string USE_OVERLOAD_WITHOUT_CANCELLATION_TOKEN_IF_ARGUMENT_IS_DEFAULT_ID = "RG0033";

		private static readonly DiagnosticDescriptor NO_AWAIT_INSIDE_LOOP = new(
			id: NO_AWAIT_INSIDE_LOOP_ID,
			title: "Do not await inside a loop",
			messageFormat: "Asynchronous operation awaited inside {0}",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Info,
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

		private static readonly DiagnosticDescriptor RECORDS_SHOULD_NOT_CONTAIN_SET_ACCESSOR = new(
			id: RECORDS_SHOULD_NOT_CONTAIN_SET_ACCESSOR_ID,
			title: "Records should not contain set accessor",
			messageFormat: "'{0}' should not have set accessor because it's declared in a record",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Records should not contain set accessor.");

		private static readonly DiagnosticDescriptor RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_FIELD = new(
			id: RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_FIELD_ID,
			title: "Records should not contain mutable field",
			messageFormat: "'{0}' should not be mutable because it's declared in a record",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Records should not contain mutable field.");

		private static readonly DiagnosticDescriptor RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_COLLECTION = new(
			id: RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_COLLECTION_ID,
			title: "Records should not contain mutable collection",
			messageFormat: "'{0}' is a mutable collection and should not be used in a record",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Records should not contain mutable collection.");

		private static readonly DiagnosticDescriptor RECORDS_SHOULD_NOT_CONTAIN_REFERENCE_TO_CLASS_OR_STRUCT_TYPE = new(
			id: RECORDS_SHOULD_NOT_CONTAIN_REFERENCE_TO_CLASS_OR_STRUCT_TYPE_ID,
			title: "Records should not contain reference to class or struct type",
			messageFormat: "'{0}' is {1} type and should not be used in a record",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Records should not contain reference to class or struct type.");

		private static readonly DiagnosticDescriptor REQUIRED_RECORD_PROPERTY_SHOULD_BE_INITIALIZED = new(
			id: REQUIRED_RECORD_PROPERTY_SHOULD_BE_INITIALIZED_ID,
			title: "Required record property should be initialized",
			messageFormat: "'{0}' is a required property and should be initialized",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Required record property should be initialized.");

		private static readonly DiagnosticDescriptor LOCAL_IS_READONLY = new(
			id: LOCAL_IS_READONLY_ID,
			title: "Local variable is readonly",
			messageFormat: "'{0}' is a readonly local variable",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Local variables prefixed with an '@' are readonly.");

		private static readonly DiagnosticDescriptor PARAMETER_IS_READONLY = new(
			id: PARAMETER_IS_READONLY_ID,
			title: "Parameter is readonly",
			messageFormat: "'{0}' is a readonly parameter",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Parameters prefixed with an '@' are readonly.");

		private static readonly DiagnosticDescriptor REF_OR_OUT_PARAMETER_CANNOT_BE_READONLY = new(
			id: REF_OR_OUT_PARAMETER_CANNOT_BE_READONLY_ID,
			title: "Ref or out parameter cannot be readonly",
			messageFormat: "'{0}' parameter '{1}' cannot be readonly",
			category: "Usage",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Ref or out parameters cannot be readonly.");

		private static readonly DiagnosticDescriptor IN_ARGUMENT_SHOULD_BE_READONLY = new(
			id: IN_ARGUMENT_SHOULD_BE_READONLY_ID,
			title: "In argument should be readonly",
			messageFormat: "'in' argument '{0}' should be readonly",
			category: "Reliability",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "In argument should be readonly.");

		private static readonly DiagnosticDescriptor CASTING_TO_AN_INCOMPATIBLE_ENUM = new(
			id: CASTING_TO_AN_INCOMPATIBLE_ENUM_ID,
			title: "Casting to an incompatible enum",
			messageFormat: "Casting to an incompatible enum; Value {0} is missing from '{1}'",
			category: "Reliability",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Avoid casting to an incompatible enum.");

		private static readonly DiagnosticDescriptor POSSIBLY_CASTING_TO_AN_INCOMPATIBLE_ENUM_MEMBER_SWAPPED = new(
			id: POSSIBLY_CASTING_TO_AN_INCOMPATIBLE_ENUM_MEMBER_SWAPPED_ID,
			title: "Possibly casting to an incompatible enum, some names have different value",
			messageFormat: "Possibly casting to an incompatible enum; '{0}' doesn't have the same value in '{1}' and in '{2}'",
			category: "Reliability",
			defaultSeverity: DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Avoid casting to an enum which has different enum values for the same names.");

		private static readonly DiagnosticDescriptor POSSIBLY_CASTING_TO_AN_INCOMPATIBLE_ENUM_DIFFERENT_MEMBER_NAME = new(
			id: POSSIBLY_CASTING_TO_AN_INCOMPATIBLE_ENUM_DIFFERENT_MEMBER_NAME_ID,
			title: "Possibly casting to an incompatible enum, some values have different name",
			messageFormat: "Possibly casting to an incompatible enum; Value {0} doesn't have a same name in '{1}' and in '{2}'",
			category: "Reliability",
			defaultSeverity: DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Avoid casting to an enum which has different enum names for the same value.");

		private static readonly DiagnosticDescriptor PROTOBUF_MESSAGE_PROPERTIES_ARE_REQUIRED = new(
			id: PROTOBUF_MESSAGE_PROPERTIES_ARE_REQUIRED_ID,
			title: "Required protobuf property should be initialized",
			messageFormat: "'{0}' is a required protobuf property and should be initialized",
			category: "Usage",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "All properties in protobuf message need to be initialized.");

		private static readonly DiagnosticDescriptor PROTOBUF_MESSAGE_ONEOF_PROPERTY_ALREADY_INITIALIZED = new(
			id: PROTOBUF_MESSAGE_ONEOF_PROPERTY_ALREADY_INITIALIZED_ID,
			title: "Can only initialize one of properties in a OneOf case",
			messageFormat: "'{0}' cannot be initialized because '{1}' has been initialized",
			category: "Usage",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Only one of properties in a OneOf case can be initialized.");

		private static readonly DiagnosticDescriptor ARGUMENT_MUST_BE_LOCKED = new(
			id: ARGUMENT_MUST_BE_LOCKED_ID,
			title: "Argument must be locked",
			messageFormat: "Argument must be locked before calling this method",
			category: "Convention",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Argument must be locked before calling method annotated with [MustBeLocked] attribute.");

		private static readonly DiagnosticDescriptor DO_NOT_USE_DYNAMIC_TYPE = new(
			id: DO_NOT_USE_DYNAMIC_TYPE_ID,
			title: "Do not use dynamic type",
			messageFormat: "Do not use dynamic type",
			category: "Code Quality",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Do not use dynamic type.");

		private static readonly DiagnosticDescriptor STATIC_CLASS_WITH_EXTENSION_METHODS_SHOULD_HAVE_EXTENSIONS_SUFFIX = new(
			id: STATIC_CLASS_WITH_EXTENSION_METHODS_SHOULD_HAVE_EXTENSIONS_SUFFIX_ID,
			title: "Static classes containing extension methods should have an 'Extensions' suffix",
			messageFormat: "'{0}' contains extension methods and should have an 'Extensions' suffix",
			category: "Naming",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Static classes containing extension methods should have an 'Extensions' suffix.");

		private static readonly DiagnosticDescriptor USE_OVERLOAD_WITHOUT_CANCELLATION_TOKEN_IF_ARGUMENT_IS_DEFAULT = new(
			id: USE_OVERLOAD_WITHOUT_CANCELLATION_TOKEN_IF_ARGUMENT_IS_DEFAULT_ID,
			title: "Use overload without CancellationToken if default or CancellationToken.None was supplied",
			messageFormat: "Use overload without CancellationToken instead",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Use overload without CancellationToken if default or CancellationToken.None was supplied.");

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
			DO_NOT_PARSE_USING_CONVERT,
			RECORDS_SHOULD_NOT_CONTAIN_SET_ACCESSOR,
			RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_FIELD,
			RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_COLLECTION,
			RECORDS_SHOULD_NOT_CONTAIN_REFERENCE_TO_CLASS_OR_STRUCT_TYPE,
			REQUIRED_RECORD_PROPERTY_SHOULD_BE_INITIALIZED,
			LOCAL_IS_READONLY,
			PARAMETER_IS_READONLY,
			REF_OR_OUT_PARAMETER_CANNOT_BE_READONLY,
			IN_ARGUMENT_SHOULD_BE_READONLY,
			CASTING_TO_AN_INCOMPATIBLE_ENUM,
			POSSIBLY_CASTING_TO_AN_INCOMPATIBLE_ENUM_MEMBER_SWAPPED,
			POSSIBLY_CASTING_TO_AN_INCOMPATIBLE_ENUM_DIFFERENT_MEMBER_NAME,
			PROTOBUF_MESSAGE_PROPERTIES_ARE_REQUIRED,
			PROTOBUF_MESSAGE_ONEOF_PROPERTY_ALREADY_INITIALIZED,
			ARGUMENT_MUST_BE_LOCKED,
			DO_NOT_USE_DYNAMIC_TYPE,
			STATIC_CLASS_WITH_EXTENSION_METHODS_SHOULD_HAVE_EXTENSIONS_SUFFIX,
			USE_OVERLOAD_WITHOUT_CANCELLATION_TOKEN_IF_ARGUMENT_IS_DEFAULT
		);

		public override void Initialize(AnalysisContext context) {
			if (context is null) return;

			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
			context.EnableConcurrentExecution();

			// NO_AWAIT_INSIDE_LOOP
			context.RegisterSyntaxNodeAction(AnalyzeAwaitExpression, SyntaxKind.AwaitExpression);

			// DONT_RETURN_TASK_IF_METHOD_DISPOSES_OBJECT
			context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);

			// DONT_RETURN_TASK_IF_METHOD_DISPOSES_OBJECT
			context.RegisterSyntaxNodeAction(AnalyzeUsingDeclarationStatement, SyntaxKind.LocalDeclarationStatement);

			// LOCAL_IS_READONLY
			context.RegisterSyntaxNodeAction(AnalyzeReadonlyLocals, SyntaxKind.LocalDeclarationStatement);

			// LOCAL_IS_READONLY
			context.RegisterSyntaxNodeAction(AnalyzeReadonlyDeclarationExpressions, SyntaxKind.DeclarationExpression);

			// LOCAL_IS_READONLY
			context.RegisterSyntaxNodeAction(AnalyzeForStatements, SyntaxKind.ForStatement);

			// PARAMETER_IS_READONLY
			// REF_OR_OUT_PARAMETER_CANNOT_BE_READONLY
			context.RegisterSyntaxNodeAction(AnalyzeParameters, SyntaxKind.Parameter);

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
			// ARGUMENT_MUST_BE_LOCKED
			// USE_OVERLOAD_WITHOUT_CANCELLATION_TOKEN_IF_ARGUMENT_IS_DEFAULT
			context.RegisterSyntaxNodeAction(AnalyzeInvocations, SyntaxKind.InvocationExpression);

			// VAR_INFERRED_TYPE_IS_OBSOLETE
			context.RegisterSyntaxNodeAction(AnalyzeVariableDeclarations, SyntaxKind.VariableDeclaration);

			// INTERFACES_SHOULDNT_DERIVE_FROM_IDISPOSABLE
			context.RegisterSyntaxNodeAction(AnalyzeInterfaceDeclarations, SyntaxKind.InterfaceDeclaration);

			// UNRESOLVED_TASK
			context.RegisterSyntaxTreeAction(AnalyzeSingleLineComments);

			// WITH_SHOULDNT_BE_USED_OUTSIDE_ITS_RECORD_DECLARATION
			context.RegisterSyntaxNodeAction(AnalyzeWithExpressions, SyntaxKind.WithExpression);

			// RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_PROPERTY
			// RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_FIELD
			// RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_COLLECTION
			// RECORDS_SHOULD_NOT_CONTAIN_REFERENCE_TO_CLASS_OR_STRUCT_TYPE
			context.RegisterSyntaxNodeAction(AnalyzeRecordDeclarations, SyntaxKind.RecordDeclaration);

			// REQUIRED_RECORD_PROPERTY_SHOULD_BE_INITIALIZED
			context.RegisterSyntaxNodeAction(AnalyzeObjectInitializers, SyntaxKind.ObjectInitializerExpression);

			// REQUIRED_RECORD_PROPERTY_SHOULD_BE_INITIALIZED
			// PROTOBUF_MESSAGE_PROPERTIES_ARE_REQUIRED
			// PROTOBUF_MESSAGE_ONEOF_PROPERTY_ALREADY_INITIALIZED
			context.RegisterSyntaxNodeAction(AnalyzeObjectCreationExpressions, SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression);

			// CASTING_TO_AN_INCOMPATIBLE_ENUM
			// POSSIBLY_CASTING_TO_AN_INCOMPATIBLE_ENUM_MEMBER_SWAPPED
			// POSSIBLY_CASTING_TO_AN_INCOMPATIBLE_ENUM_DIFFERENT_MEMBER_NAME
			context.RegisterSyntaxNodeAction(AnalyzeCastExpressions, SyntaxKind.CastExpression);

			// DO_NOT_USE_DYNAMIC_TYPE
			context.RegisterSyntaxNodeAction(AnalyzeIdentifierNames, SyntaxKind.IdentifierName);

			// STATIC_CLASS_WITH_EXTENSION_METHODS_SHOULD_HAVE_EXTENSIONS_SUFFIX
			context.RegisterSyntaxNodeAction(AnalyzeClassDeclarations, SyntaxKind.ClassDeclaration);
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
					&& usingKeyword.IsKind(SyntaxKind.UsingKeyword)) {
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

		private static void AnalyzeReadonlyLocals(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is LocalDeclarationStatementSyntax { Declaration: var declaration } localDeclarationStatementSyntax) {
					RefTypeSyntax? refTypeSyntax = declaration.Type as RefTypeSyntax;
					bool isRefLocal = refTypeSyntax is not null;
					bool isRefReadonlyLocal = isRefLocal && refTypeSyntax.ReadOnlyKeyword.IsKind(SyntaxKind.ReadOnlyKeyword);
					
					foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in declaration.Variables) {
						if (variableDeclaratorSyntax is { Identifier: var declaredIdentifier }
							&& declaredIdentifier.Text.StartsWith("@", StringComparison.Ordinal)) {
							
							// For ref locals with @, check the initializer
							// - ref int @x can only reference readonly sources (@ vars/params, in params)
							// - ref readonly int @x can reference readonly sources AND regular vars/params, but NOT ref/out parameters
							if (isRefLocal && variableDeclaratorSyntax.Initializer?.Value is RefExpressionSyntax refExpression) {
								// Check if the ref target is a readonly or mutable variable
								bool refTargetIsReadonly = false;
								if (refExpression.Expression is IdentifierNameSyntax refTargetIdentifier) {
									// Check if the symbol was declared with @ prefix
									if (context.SemanticModel.GetSymbolInfo(refTargetIdentifier, context.CancellationToken).Symbol is ISymbol symbol) {
										if (symbol is ILocalSymbol localSymbol) {
											if (localSymbol.DeclaringSyntaxReferences.Length > 0) {
												SyntaxNode declarationNode = localSymbol.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken);
												if (declarationNode is VariableDeclaratorSyntax varDeclarator) {
													refTargetIsReadonly = varDeclarator.Identifier.Text.StartsWith("@", StringComparison.Ordinal);
												} else if (declarationNode is SingleVariableDesignationSyntax singleVar) {
													refTargetIsReadonly = singleVar.Identifier.Text.StartsWith("@", StringComparison.Ordinal);
												}
											}
											// For ref readonly locals, also allow regular (non-ref) locals
											if (isRefReadonlyLocal && !refTargetIsReadonly) {
												refTargetIsReadonly = true; // Regular local is OK for ref readonly
											}
										} else if (symbol is IParameterSymbol paramSymbol) {
											// Check if it's an 'in' parameter or a parameter declared with @
											if (paramSymbol.RefKind == RefKind.In) {
												refTargetIsReadonly = true;
											} else if (paramSymbol.RefKind is RefKind.Ref or RefKind.Out) {
												// ref/out parameters are never readonly
												refTargetIsReadonly = false;
											} else if (paramSymbol.DeclaringSyntaxReferences.Length > 0) {
												SyntaxNode paramDeclarationNode = paramSymbol.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken);
												if (paramDeclarationNode is ParameterSyntax paramSyntax) {
													refTargetIsReadonly = paramSyntax.Identifier.Text.StartsWith("@", StringComparison.Ordinal);
												}
											}
											// For ref readonly locals, also allow regular (non-ref) parameters
											if (isRefReadonlyLocal && !refTargetIsReadonly && paramSymbol.RefKind == RefKind.None) {
												refTargetIsReadonly = true; // Regular parameter is OK for ref readonly
											}
										}
									}
									
									if (!refTargetIsReadonly) {
										// Assigning a mutable variable to a readonly ref local
										Diagnostic diagnostic = Diagnostic.Create(LOCAL_IS_READONLY, variableDeclaratorSyntax.GetLocation(), declaredIdentifier.ValueText);
										context.ReportDiagnostic(diagnostic);
									}
								} else if (refExpression.Expression is MemberAccessExpressionSyntax memberAccess) {
									// Accessing a field/property - check if it's readonly
									if (context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken).Symbol is ISymbol symbol) {
										if (!IsSymbolReadOnly(symbol)) {
											Diagnostic diagnostic = Diagnostic.Create(LOCAL_IS_READONLY, variableDeclaratorSyntax.GetLocation(), declaredIdentifier.ValueText);
											context.ReportDiagnostic(diagnostic);
										}
									}
								} else if (refExpression.Expression is InvocationExpressionSyntax) {
									// Method invocation - for now we assume ref readonly returns are okay
									// This would need more sophisticated analysis to be fully correct
								}
							}
							
							if (localDeclarationStatementSyntax.Ancestors().FirstOrDefault(ancestor => ancestor.Kind() is SyntaxKind.Block) is SyntaxNode scopeNode) {
								AnalyzeReadonlyLocalUsages(context, declaredIdentifier, scopeNode, LOCAL_IS_READONLY);
							}
						}
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeReadonlyDeclarationExpressions(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is DeclarationExpressionSyntax { Designation: SingleVariableDesignationSyntax { Identifier: var declaredIdentifier } } declarationExpressionSyntax
					&& declaredIdentifier.Text.StartsWith("@", StringComparison.Ordinal)) {
					if (declarationExpressionSyntax.Ancestors().FirstOrDefault(ancestor => ancestor.Kind() is SyntaxKind.Block) is SyntaxNode scopeNode) {
						AnalyzeReadonlyLocalUsages(context, declaredIdentifier, scopeNode, LOCAL_IS_READONLY);
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeForStatements(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is ForStatementSyntax { Declaration: { Variables: var variables } } forStatementDeclarationSyntax) {
					foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in variables) {
						if (variableDeclaratorSyntax is { Identifier: var declaredIdentifier }
							&& declaredIdentifier.Text.StartsWith("@", StringComparison.Ordinal)) {
							if (forStatementDeclarationSyntax.Ancestors().FirstOrDefault(ancestor => ancestor.Kind() is SyntaxKind.Block) is SyntaxNode scopeNode) {
								AnalyzeReadonlyLocalUsages(context, declaredIdentifier, scopeNode, LOCAL_IS_READONLY);
							}
						}
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeParameters(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is ParameterSyntax { Identifier: var declaredIdentifier } parameterSyntax
					&& declaredIdentifier.Text.StartsWith("@", StringComparison.Ordinal)) {
					switch (parameterSyntax.Modifiers.Select(modifier => modifier.Kind()).FirstOrDefault(kind => kind is SyntaxKind.RefKeyword or SyntaxKind.OutKeyword)) {
						case SyntaxKind.RefKeyword: {
								Diagnostic diagnostic = Diagnostic.Create(REF_OR_OUT_PARAMETER_CANNOT_BE_READONLY, parameterSyntax.GetLocation(), "ref", declaredIdentifier.ValueText);
								context.ReportDiagnostic(diagnostic);
								break;
							}
						case SyntaxKind.OutKeyword: {
								Diagnostic diagnostic = Diagnostic.Create(REF_OR_OUT_PARAMETER_CANNOT_BE_READONLY, parameterSyntax.GetLocation(), "out", declaredIdentifier.ValueText);
								context.ReportDiagnostic(diagnostic);
								break;
							}
						default: {
								if (parameterSyntax.Ancestors().FirstOrDefault(ancestor => ancestor.Kind()
									is SyntaxKind.MethodDeclaration
									or SyntaxKind.SimpleLambdaExpression
									or SyntaxKind.ParenthesizedLambdaExpression
									or SyntaxKind.IndexerDeclaration) is SyntaxNode scopeNode) {
									AnalyzeReadonlyLocalUsages(context, declaredIdentifier, scopeNode, PARAMETER_IS_READONLY);
								}
								break;
							}
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeReadonlyLocalUsages(SyntaxNodeAnalysisContext context, SyntaxToken declaredIdentifier, SyntaxNode scopeNode, DiagnosticDescriptor diagnosticDescriptor) {
			foreach (SyntaxNode node in scopeNode.DescendantNodes()) {
				switch (node) {
					case AssignmentExpressionSyntax assignmentExpressionSyntax
					when assignmentExpressionSyntax.Left is IdentifierNameSyntax { Identifier: var identifier }
						&& declaredIdentifier.ValueText == identifier.ValueText: {
							Diagnostic diagnostic = Diagnostic.Create(diagnosticDescriptor, assignmentExpressionSyntax.GetLocation(), identifier.ValueText);
							context.ReportDiagnostic(diagnostic);
							break;
						}
					case AssignmentExpressionSyntax assignmentExpressionSyntax
					when assignmentExpressionSyntax.Left is TupleExpressionSyntax { Arguments: var tupleArguments }: {
							foreach (ArgumentSyntax tupleArgument in tupleArguments) {
								switch (tupleArgument.Expression) {
									case IdentifierNameSyntax { Identifier: var identifier }
									when declaredIdentifier.ValueText == identifier.ValueText: {
											Diagnostic diagnostic = Diagnostic.Create(diagnosticDescriptor, tupleArgument.GetLocation(), identifier.ValueText);
											context.ReportDiagnostic(diagnostic);
											break;
										}
								}
							}
							break;
						}
					case PrefixUnaryExpressionSyntax { Operand: IdentifierNameSyntax { Identifier: var identifier } } prefixUnaryExpressionSyntax
					when prefixUnaryExpressionSyntax.OperatorToken.Kind() is SyntaxKind.PlusPlusToken or SyntaxKind.MinusMinusToken
						&& declaredIdentifier.ValueText == identifier.ValueText: {
							Diagnostic diagnostic = Diagnostic.Create(diagnosticDescriptor, prefixUnaryExpressionSyntax.GetLocation(), identifier.ValueText);
							context.ReportDiagnostic(diagnostic);
							break;
						}
					case PostfixUnaryExpressionSyntax { Operand: IdentifierNameSyntax { Identifier: var identifier } } postfixUnaryExpressionSyntax
					when postfixUnaryExpressionSyntax.OperatorToken.Kind() is SyntaxKind.PlusPlusToken or SyntaxKind.MinusMinusToken
						&& declaredIdentifier.ValueText == identifier.ValueText: {
							Diagnostic diagnostic = Diagnostic.Create(diagnosticDescriptor, postfixUnaryExpressionSyntax.GetLocation(), identifier.ValueText);
							context.ReportDiagnostic(diagnostic);
							break;
						}
					case ArgumentSyntax { Expression: IdentifierNameSyntax { Identifier: var identifier } } refOrOutArgumentSyntax
					when refOrOutArgumentSyntax.RefKindKeyword.Kind() is SyntaxKind.RefKeyword or SyntaxKind.OutKeyword
						&& declaredIdentifier.ValueText == identifier.ValueText: {
							Diagnostic diagnostic = Diagnostic.Create(diagnosticDescriptor, refOrOutArgumentSyntax.GetLocation(), identifier.ValueText);
							context.ReportDiagnostic(diagnostic);
							break;
						}
					case RefExpressionSyntax { Expression: IdentifierNameSyntax { Identifier: var identifier } } refExpressionSyntax
					when declaredIdentifier.ValueText == identifier.ValueText: {
							// Check if this ref expression is used in a ref local declaration
							if (refExpressionSyntax.Parent is EqualsValueClauseSyntax equalsValueClause
								&& equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator
								&& variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration
								&& variableDeclaration.Type is RefTypeSyntax refType
								&& !refType.ReadOnlyKeyword.IsKind(SyntaxKind.ReadOnlyKeyword)) {
								// Verify this is actually a local declaration, not a ref reassignment
								// Ref reassignments don't have LocalDeclarationStatementSyntax as an ancestor of the variable declaration
								if (variableDeclaration.Parent is LocalDeclarationStatementSyntax) {
									// This is a ref (not ref readonly) local declaration
									if (variableDeclarator.Identifier.Text.StartsWith("@", StringComparison.Ordinal)) {
										// The target is readonly, so it's okay
									} else {
										// The target is mutable, warn about using readonly source
										Diagnostic diagnostic = Diagnostic.Create(diagnosticDescriptor, refExpressionSyntax.Expression.GetLocation(), identifier.ValueText);
										context.ReportDiagnostic(diagnostic);
									}
								}
							}
							// Note: Ref reassignments are not analyzed yet
							break;
						}
				}
			}
		}

		private static void AnalyzeNamedTypeDeclaration(SymbolAnalysisContext context) {
			try {
				if (context.Symbol is INamedTypeSymbol { ContainingNamespace: { } containingNamespace } namedTypeSymbol) {
					if (IsInternalNamespace(containingNamespace, out string fullNamespace)) {
						switch (context.Symbol.DeclaredAccessibility) {
							case Accessibility.Internal:
							case Accessibility.Private:
							case Accessibility.Protected:
							case Accessibility.ProtectedAndInternal:
								return;
							default:
								Diagnostic diagnostic = Diagnostic.Create(IDENTIFIERS_IN_INTERNAL_NAMESPACE_MUST_BE_INTERNAL, context.Symbol.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken).GetLocation(), context.Symbol.Name, fullNamespace);
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
					if (context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax, context.CancellationToken) is SymbolInfo memberSymbolInfo
						&& memberSymbolInfo.Symbol is ISymbol member
						&& member.Kind == SymbolKind.Field
						&& !member.IsStatic
						&& member.DeclaredAccessibility == Accessibility.Private
						&& memberAccessExpressionSyntax.Expression.ToString() != "this") {
						Diagnostic diagnostic = Diagnostic.Create(DO_NOT_ACCESS_PRIVATE_FIELDS_OF_ANOTHER_OBJECT_DIRECTLY, memberAccessExpressionSyntax.Name.GetLocation(), memberAccessExpressionSyntax.Name.Identifier.ValueText);
						context.ReportDiagnostic(diagnostic);
					} else if (context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Expression, context.CancellationToken) is SymbolInfo objSymbolInfo
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
								if (context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Name, context.CancellationToken) is SymbolInfo waitSymbolInfo
									&& waitSymbolInfo.Symbol is { } waitSymbol
									&& waitSymbol.ContainingType.ToString().StartsWith("System.Threading.Tasks.Task", StringComparison.Ordinal)
									&& context.Node.Parent is InvocationExpressionSyntax waitInvocationSyntax) {
									Diagnostic diagnostic = Diagnostic.Create(DO_NOT_CALL_TASK_WAIT_TO_INVOKE_TASK, waitInvocationSyntax.GetLocation());
									context.ReportDiagnostic(diagnostic);
								}
								break;
							case "Result":
								if (context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Name, context.CancellationToken) is SymbolInfo resultSymbolInfo
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
						if (context.SemanticModel.GetTypeInfo(invocationArguments[0].Expression, context.CancellationToken) is { Type: INamedTypeSymbol argumentTypeSymbol }
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
					} else if (context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken) is { Symbol: IMethodSymbol { Parameters: { } methodParameters, ReturnType: { } methodReturnType } }) {
						MethodDeclarationSyntax? methodDeclaration = invocationExpressionSyntax.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
						if (methodDeclaration is { ParameterList: { Parameters: { } declaredParameters } }) {
							if (declaredParameters.Any(callerParameter => callerParameter.Type?.ToString() is "CancellationToken" or "System.Threading.CancellationToken")) {
								if (methodParameters.Length == invocationArguments.Count) {
									foreach (IMethodSymbol overloadSymbol in context.SemanticModel.GetMemberGroup(invocationExpressionSyntax.Expression, context.CancellationToken).OfType<IMethodSymbol>()) {
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
							// RG0033: Check if CancellationToken.None or default was passed and there's an overload without it
							if (methodParameters.Length > 0
								&& methodParameters.Last().Type.ToString() is "CancellationToken" or "System.Threading.CancellationToken"
								&& invocationArguments.Count > 0) {
								ArgumentSyntax lastArgument = invocationArguments.Last();
								bool isDefaultOrNone = false;
								
								if (lastArgument.Expression is MemberAccessExpressionSyntax { Name: IdentifierNameSyntax { Identifier: { ValueText: "None" } } } memberAccess) {
									if (context.SemanticModel.GetSymbolInfo(memberAccess.Expression, context.CancellationToken) is { Symbol: INamedTypeSymbol typeSymbol }
										&& typeSymbol.ToString() is "System.Threading.CancellationToken" or "CancellationToken") {
										isDefaultOrNone = true;
									}
								} else if (lastArgument.Expression is LiteralExpressionSyntax { Token: { ValueText: "default" } }) {
									isDefaultOrNone = true;
								} else if (lastArgument.Expression is DefaultExpressionSyntax) {
									isDefaultOrNone = true;
								}
								
								if (isDefaultOrNone) {
									foreach (IMethodSymbol overloadSymbol in context.SemanticModel.GetMemberGroup(invocationExpressionSyntax.Expression, context.CancellationToken).OfType<IMethodSymbol>()) {
										if (overloadSymbol.Parameters.Length == methodParameters.Length - 1
											&& SymbolEqualityComparer.Default.Equals(overloadSymbol.ReturnType, methodReturnType)) {
											bool signatureMatches = true;
											for (int i = 0; i < overloadSymbol.Parameters.Length; i++) {
												if (!SymbolEqualityComparer.Default.Equals(overloadSymbol.Parameters[i].Type, methodParameters[i].Type)) {
													signatureMatches = false;
													break;
												}
											}
											if (signatureMatches) {
												Diagnostic diagnostic = Diagnostic.Create(USE_OVERLOAD_WITHOUT_CANCELLATION_TOKEN_IF_ARGUMENT_IS_DEFAULT, invocationExpressionSyntax.GetLocation());
												context.ReportDiagnostic(diagnostic);
												break;
											}
										}
									}
								}
							}
							ImmutableList<ParameterSyntax> mustBeLockedParameters = declaredParameters.Where(declaredParameter => declaredParameter.AttributeLists.Any(attributeList => attributeList.Attributes.Any(attribute => attribute.Name.ToString() is "MustBeLocked" or "RG.Annotations.MustBeLocked"))).ToImmutableList();
							if (mustBeLockedParameters.Count > 0) {
								// TODO: Implement RG0030
							}
						}
						
						// RG0024: Check if 'in' parameters receive readonly arguments
						for (int i = 0; i < Math.Min(methodParameters.Length, invocationArguments.Count); i++) {
							if (methodParameters[i].RefKind == RefKind.In) {
								ArgumentSyntax argument = invocationArguments[i];
								if (argument.Expression is IdentifierNameSyntax argumentIdentifier) {
									// Check if the argument is a readonly variable or an 'in' parameter
									bool isReadonly = false;
									
									// Check if it's a parameter with 'in' modifier
									if (context.SemanticModel.GetSymbolInfo(argumentIdentifier, context.CancellationToken).Symbol is IParameterSymbol paramSymbol) {
										isReadonly = paramSymbol.RefKind == RefKind.In;
									} else if (context.SemanticModel.GetSymbolInfo(argumentIdentifier, context.CancellationToken).Symbol is ILocalSymbol localSymbol) {
										// Check if the local was declared with @ prefix
										if (localSymbol.DeclaringSyntaxReferences.Length > 0) {
											SyntaxNode declarationNode = localSymbol.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken);
											if (declarationNode is VariableDeclaratorSyntax variableDeclarator) {
												isReadonly = variableDeclarator.Identifier.Text.StartsWith("@", StringComparison.Ordinal);
											} else if (declarationNode is SingleVariableDesignationSyntax singleVariableDesignation) {
												isReadonly = singleVariableDesignation.Identifier.Text.StartsWith("@", StringComparison.Ordinal);
											} else if (declarationNode is ParameterSyntax parameterSyntax) {
												isReadonly = parameterSyntax.Identifier.Text.StartsWith("@", StringComparison.Ordinal);
											}
										}
									}
									
									if (!isReadonly) {
										Diagnostic diagnostic = Diagnostic.Create(IN_ARGUMENT_SHOULD_BE_READONLY, argument.GetLocation(), argumentIdentifier.Identifier.ValueText);
										context.ReportDiagnostic(diagnostic);
									}
								}
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
				SyntaxNode root = context.Tree.GetCompilationUnitRoot();
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
					} else if (context.SemanticModel.GetDeclaredSymbol(recordDeclarationSyntax, context.CancellationToken)?.ToString() != namedTypeSymbol.ToString()) {
						Diagnostic diagnostic = Diagnostic.Create(WITH_SHOULDNT_BE_USED_OUTSIDE_ITS_RECORD_DECLARATION, withKeyword.GetLocation(), namedTypeSymbol.ToString());
						context.ReportDiagnostic(diagnostic);
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeRecordDeclarations(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is RecordDeclarationSyntax
					{
						Members: { } members,
						AttributeLists: var attributeLists
					} recordDeclaration
					&& context.SemanticModel.GetDeclaredSymbol(recordDeclaration, context.CancellationToken) is INamedTypeSymbol recordSymbol
					&& !attributeLists.Any(attributeList => attributeList.Attributes.Any(attribute => attribute.Name.ToString() is "Mutable" or "RG.Annotations.Mutable"))) {
					if (recordDeclaration.ParameterList is { } parameterList) {
						foreach (ParameterSyntax parameter in parameterList.Parameters) {
							if (parameter.Type is not null
								&& context.SemanticModel.GetTypeInfo(parameter.Type, context.CancellationToken).Type is ITypeSymbol parameterTypeSymbol) {
								AnalyzeRecordMemberType(context, parameter.Type, parameterTypeSymbol);
							}
						}
					}
					foreach (MemberDeclarationSyntax member in members) {
						switch (member) {
							case PropertyDeclarationSyntax property: {
									if (property.AccessorList is { Accessors: { } accessors }) {
										if (accessors.FirstOrDefault(accessor => accessor.Kind() == SyntaxKind.SetAccessorDeclaration) is { } setAccessor) {
											Diagnostic diagnostic = Diagnostic.Create(RECORDS_SHOULD_NOT_CONTAIN_SET_ACCESSOR, setAccessor.GetLocation(), property.Identifier.ValueText);
											context.ReportDiagnostic(diagnostic);
										}
									}
									if (recordSymbol.GetMembers(property.Identifier.ValueText) is { Length: > 0 } propertySymbols
										&& propertySymbols[0] is IPropertySymbol { Type: ITypeSymbol propertyTypeSymbol }) {
										AnalyzeRecordMemberType(context, property.Type, propertyTypeSymbol);
									}
									break;
								}
							case FieldDeclarationSyntax field: {
									if (!field.Modifiers.Any(modifier => modifier.Kind() is SyntaxKind.ReadOnlyKeyword or SyntaxKind.ConstKeyword)) {
										foreach (VariableDeclaratorSyntax variableDeclarator in field.Declaration.Variables) {
											Diagnostic diagnostic = Diagnostic.Create(RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_FIELD, variableDeclarator.GetLocation(), variableDeclarator.Identifier.ValueText);
											context.ReportDiagnostic(diagnostic);
										}
									}
									foreach (VariableDeclaratorSyntax variableDeclarator in field.Declaration.Variables) {
										if (recordSymbol.GetMembers(variableDeclarator.Identifier.ValueText) is { } fieldSymbols
											&& fieldSymbols[0] is IFieldSymbol { Type: ITypeSymbol fieldTypeSymbol }) {
											AnalyzeRecordMemberType(context, field.Declaration.Type, fieldTypeSymbol);
										}
									}
									break;
								}
						}
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeObjectInitializers(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is InitializerExpressionSyntax { Parent: var parent, Expressions: var initializerExpressions } initializer) {
					if (parent is BaseObjectCreationExpressionSyntax objectCreation
						&& context.SemanticModel.GetTypeInfo(objectCreation, context.CancellationToken).Type is ITypeSymbol type
						&& type.DeclaringSyntaxReferences.Length > 0
						&& type.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken) is RecordDeclarationSyntax { Members: var recordMembers } recordDeclaration) {
						foreach (MemberDeclarationSyntax memberDeclaration in recordMembers) {
							switch (memberDeclaration) {
								case PropertyDeclarationSyntax propertyDeclaration: {
										if (propertyDeclaration.Identifier.Text.Length > 0
											&& propertyDeclaration.Identifier.Text[0] == '@') {
											if (!initializerExpressions.Any(initializerExpression => {
												return initializerExpression is AssignmentExpressionSyntax { Left: IdentifierNameSyntax { Identifier: { ValueText: var initializedMemberName } } }
													&& initializedMemberName == propertyDeclaration.Identifier.ValueText;
											})) {
												Diagnostic diagnostic = Diagnostic.Create(REQUIRED_RECORD_PROPERTY_SHOULD_BE_INITIALIZED, initializer.GetLocation(), propertyDeclaration.Identifier.ValueText);
												context.ReportDiagnostic(diagnostic);
											}
										} else if (propertyDeclaration.AttributeLists
											.SelectMany(attributeList => attributeList.Attributes)
											.FirstOrDefault(attribute => attribute.Name.ToString() is "Required" or "System.ComponentModel.DataAnnotations.Required")
											is AttributeSyntax requiredAttribute) {
											if (context.SemanticModel.GetTypeInfo(requiredAttribute, context.CancellationToken).Type is INamedTypeSymbol requiredAttributeSymbol
												&& requiredAttributeSymbol.ToString() == "System.ComponentModel.DataAnnotations.RequiredAttribute"
												&& !initializerExpressions.Any(initializerExpression => {
													return initializerExpression is AssignmentExpressionSyntax { Left: IdentifierNameSyntax { Identifier: { ValueText: var initializedMemberName } } }
														&& initializedMemberName == propertyDeclaration.Identifier.ValueText;
												})) {
												Diagnostic diagnostic = Diagnostic.Create(REQUIRED_RECORD_PROPERTY_SHOULD_BE_INITIALIZED, initializer.GetLocation(), propertyDeclaration.Identifier.ValueText);
												context.ReportDiagnostic(diagnostic);
											}
										}
										break;
									}
							}
						}
					}
				}
			} catch (Exception exc) {
				throw new Exception($"'{exc.GetType()}' was thrown from {exc.StackTrace}", exc);
			}
		}

		private static void AnalyzeObjectCreationExpressions(SyntaxNodeAnalysisContext context) {
			if (context.Node is BaseObjectCreationExpressionSyntax objectCreationExpressionSyntax
				&& context.SemanticModel.GetTypeInfo(objectCreationExpressionSyntax, context.CancellationToken).Type is INamedTypeSymbol namedTypeSymbol) {
				if (namedTypeSymbol is { DeclaringSyntaxReferences: { Length: > 0 } declaringSyntaxReferences }
					&& declaringSyntaxReferences[0].GetSyntax(context.CancellationToken) is RecordDeclarationSyntax { Members: var recordMembers } recordDeclarationSyntax
					&& objectCreationExpressionSyntax.Initializer is null) {
					foreach (MemberDeclarationSyntax memberDeclaration in recordMembers) {
						switch (memberDeclaration) {
							case PropertyDeclarationSyntax propertyDeclarationSyntax: {
									if (propertyDeclarationSyntax.Identifier.Text is { Length: > 0 } text
										&& text[0] == '@') {
										Diagnostic diagnostic = Diagnostic.Create(REQUIRED_RECORD_PROPERTY_SHOULD_BE_INITIALIZED, objectCreationExpressionSyntax.GetLocation(), propertyDeclarationSyntax.Identifier.ValueText);
										context.ReportDiagnostic(diagnostic);
									} else if (propertyDeclarationSyntax.AttributeLists
										.SelectMany(attributeList => attributeList.Attributes)
										.FirstOrDefault(attribute => attribute.Name.ToString() is "Required" or "System.ComponentModel.DataAnnotations.Required")
										is AttributeSyntax requiredAttribute) {
										if (context.SemanticModel.GetTypeInfo(requiredAttribute, context.CancellationToken).Type is INamedTypeSymbol requiredAttributeSymbol
											&& requiredAttributeSymbol.ToString() == "System.ComponentModel.DataAnnotations.RequiredAttribute") {
											Diagnostic diagnostic = Diagnostic.Create(REQUIRED_RECORD_PROPERTY_SHOULD_BE_INITIALIZED, objectCreationExpressionSyntax.GetLocation(), propertyDeclarationSyntax.Identifier.ValueText);
											context.ReportDiagnostic(diagnostic);
										}
									}
									break;
								}
						}
					}
				} else if (namedTypeSymbol is { TypeKind: TypeKind.Class }
					&& namedTypeSymbol.Interfaces.Any(i => i.ToString() == "Google.Protobuf.IMessage")) {
					if (objectCreationExpressionSyntax.ArgumentList is { Arguments: { Count: 1 } }) return;

					ImmutableArray<IPropertySymbol> propertySymbols = namedTypeSymbol.GetMembers()
						.OfType<IPropertySymbol>()
						.Where(member => member.Type.OriginalDefinition.ToString() is not "Google.Protobuf.Reflection.MessageDescriptor" and not "Google.Protobuf.MessageParser<>")
						.ToImmutableArray();
					ImmutableArray<INamedTypeSymbol> messageTypeMembers = namedTypeSymbol.GetTypeMembers()
						.Where(typeMember => typeMember.DeclaredAccessibility == Accessibility.Public && typeMember.Name.EndsWith("OneofCase"))
						.ToImmutableArray();
					Dictionary<string, string> oneofGroupNameByPropertyName = new();
					foreach (INamedTypeSymbol enumTypeSymbol in messageTypeMembers) {
						foreach (IFieldSymbol fieldSymbol in enumTypeSymbol.GetMembers().OfType<IFieldSymbol>().Where(field => field is { IsConst: true, Name: not "None" })) {
							oneofGroupNameByPropertyName.Add(fieldSymbol.Name, enumTypeSymbol.Name.Substring(0, enumTypeSymbol.Name.Length - "OneofCase".Length) + "Case");
						}
					}

					if (objectCreationExpressionSyntax.Initializer is null) {
						foreach (IPropertySymbol property in from property in propertySymbols
															 where property.SetMethod is not null || property.Type.OriginalDefinition.ToString() == "Google.Protobuf.Collections.RepeatedField<>"
															 select property) {
							Diagnostic diagnostic = Diagnostic.Create(PROTOBUF_MESSAGE_PROPERTIES_ARE_REQUIRED, objectCreationExpressionSyntax.GetLocation(), property.Name);
							context.ReportDiagnostic(diagnostic);
						}
					} else {
						Dictionary<string, string> initializedOneofGroupNameByPropertyName = new();
						foreach (IPropertySymbol property in propertySymbols) {
							if (property.SetMethod is not null
								|| property.Type.OriginalDefinition.ToString() == "Google.Protobuf.Collections.RepeatedField<>") {
								string? oneofGroupName = oneofGroupNameByPropertyName.TryGetValue(property.Name, out string? g) ? g : null;
								if (objectCreationExpressionSyntax.Initializer.Expressions.FirstOrDefault(expr =>
									expr is AssignmentExpressionSyntax { Left: IdentifierNameSyntax { Identifier: { ValueText: var assignmentTargetName } } }
									&& assignmentTargetName == property.Name) is not AssignmentExpressionSyntax assignmentExpressionSyntax) {
									if (oneofGroupName is null || !initializedOneofGroupNameByPropertyName.ContainsKey(oneofGroupName)) {
										Diagnostic diagnostic = Diagnostic.Create(PROTOBUF_MESSAGE_PROPERTIES_ARE_REQUIRED, objectCreationExpressionSyntax.Initializer.GetLocation(), property.Name);
										context.ReportDiagnostic(diagnostic);
									}
								} else {
									if (oneofGroupName is not null) {
										if (initializedOneofGroupNameByPropertyName.TryGetValue(oneofGroupName, out string? initializedPropertyName)) {
											Diagnostic diagnostic = Diagnostic.Create(PROTOBUF_MESSAGE_ONEOF_PROPERTY_ALREADY_INITIALIZED, assignmentExpressionSyntax.GetLocation(), property.Name, initializedPropertyName);
											context.ReportDiagnostic(diagnostic);
										} else {
											initializedOneofGroupNameByPropertyName.Add(oneofGroupName, property.Name);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		private static void AnalyzeCastExpressions(SyntaxNodeAnalysisContext context) {
			if (context.Node is CastExpressionSyntax { Type: IdentifierNameSyntax { Identifier: { Text: { } typeIdentifier } } typeSyntax, Expression: { } expression } castExpressionSyntax
				&& context.SemanticModel.GetTypeInfo(typeSyntax, context.CancellationToken) is { Type: INamedTypeSymbol { EnumUnderlyingType: { } enumUnderlyingType, DeclaringSyntaxReferences: { Length: 1 } typeDeclaringSyntaxes } } typeInfo
				&& context.SemanticModel.GetTypeInfo(expression, context.CancellationToken) is { Type: INamedTypeSymbol { EnumUnderlyingType: { } exprEnumUnderlyingType, DeclaringSyntaxReferences: { Length: 1 } exprTypeDeclaringSyntaxes } exprTypeSymbol } exprTypeInfo
				&& typeDeclaringSyntaxes[0].GetSyntax(context.CancellationToken) is EnumDeclarationSyntax { Members: var typeMembers }
				&& exprTypeDeclaringSyntaxes[0].GetSyntax(context.CancellationToken) is EnumDeclarationSyntax { Members: var exprTypeMembers }) {
				ImmutableList<KeyValuePair<string, int>> enumValues = GetEnumValues(typeMembers).ToImmutableList();
				ImmutableList<KeyValuePair<string, int>> exprTypeEnumValues = GetEnumValues(exprTypeMembers).ToImmutableList();
				foreach (KeyValuePair<string, int> exprTypeEnumValue in exprTypeEnumValues) {
					if (!enumValues.Any(ev => ev.Value == exprTypeEnumValue.Value)) {
						Diagnostic diagnostic = Diagnostic.Create(CASTING_TO_AN_INCOMPATIBLE_ENUM, castExpressionSyntax.GetLocation(), exprTypeEnumValue.Value, typeIdentifier);
						context.ReportDiagnostic(diagnostic);
						return;
					}
				}
				foreach (KeyValuePair<string, int> exprTypeEnumValue in exprTypeEnumValues) {
					foreach (KeyValuePair<string, int> enumValue in enumValues) {
						if (enumValue.Key == exprTypeEnumValue.Key
							&& enumValue.Value != exprTypeEnumValue.Value) {
							Diagnostic diagnostic = Diagnostic.Create(POSSIBLY_CASTING_TO_AN_INCOMPATIBLE_ENUM_MEMBER_SWAPPED, castExpressionSyntax.GetLocation(), enumValue.Key, typeIdentifier, exprTypeSymbol.Name);
							context.ReportDiagnostic(diagnostic);
							return;
						}
					}
				}
				foreach (KeyValuePair<string, int> exprTypeEnumValue in exprTypeEnumValues) {
					foreach (KeyValuePair<string, int> enumValue in enumValues) {
						if (enumValue.Value == exprTypeEnumValue.Value
							&& enumValue.Key != exprTypeEnumValue.Key) {
							Diagnostic diagnostic = Diagnostic.Create(POSSIBLY_CASTING_TO_AN_INCOMPATIBLE_ENUM_DIFFERENT_MEMBER_NAME, castExpressionSyntax.GetLocation(), enumValue.Value, typeIdentifier, exprTypeSymbol.Name);
							context.ReportDiagnostic(diagnostic);
							return;
						}
					}
				}
			}
		}

		private static void AnalyzeIdentifierNames(SyntaxNodeAnalysisContext context) {
			if (context.Node is IdentifierNameSyntax {
				Identifier: { Text: "dynamic" } identifier
			}) {
				Diagnostic diagnostic = Diagnostic.Create(DO_NOT_USE_DYNAMIC_TYPE, identifier.GetLocation());
				context.ReportDiagnostic(diagnostic);
			}
		}

		private static void AnalyzeClassDeclarations(SyntaxNodeAnalysisContext context) {
			try {
				if (context.Node is ClassDeclarationSyntax classDeclarationSyntax
					&& classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword)) {
					
					// Check if the class contains extension methods
					bool hasExtensionMethods = classDeclarationSyntax.Members
						.OfType<MethodDeclarationSyntax>()
						.Any(method => method.ParameterList.Parameters.Count > 0 
							&& method.ParameterList.Parameters[0].Modifiers.Any(SyntaxKind.ThisKeyword));

					if (hasExtensionMethods) {
						string className = classDeclarationSyntax.Identifier.ValueText;
						
						// Check if the class name already ends with "Extensions"
						if (!className.EndsWith("Extensions", StringComparison.Ordinal)) {
							Diagnostic diagnostic = Diagnostic.Create(
								STATIC_CLASS_WITH_EXTENSION_METHODS_SHOULD_HAVE_EXTENSIONS_SUFFIX,
								classDeclarationSyntax.Identifier.GetLocation(),
								className);
							context.ReportDiagnostic(diagnostic);
						}
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

		private static void AnalyzeRecordMemberType(SyntaxNodeAnalysisContext context, TypeSyntax typeSyntax, ITypeSymbol typeSymbol) {
			if (typeSymbol.GetAttributes().Any(attributeData => attributeData.AttributeClass is { Name: "MutableAttribute" })) {
				Diagnostic diagnostic = Diagnostic.Create(RECORDS_SHOULD_NOT_CONTAIN_REFERENCE_TO_CLASS_OR_STRUCT_TYPE, typeSyntax.GetLocation(), typeSyntax.ToString(), "mutable");
				context.ReportDiagnostic(diagnostic);
				return;
			}

			switch (typeSymbol.SpecialType) {
				case SpecialType.System_Boolean:
				case SpecialType.System_Byte:
				case SpecialType.System_Char:
				case SpecialType.System_DateTime:
				case SpecialType.System_Decimal:
				case SpecialType.System_Delegate:
				case SpecialType.System_Double:
				case SpecialType.System_Int16:
				case SpecialType.System_Int32:
				case SpecialType.System_Int64:
				case SpecialType.System_IntPtr:
				case SpecialType.System_SByte:
				case SpecialType.System_Single:
				case SpecialType.System_String:
				case SpecialType.System_UInt16:
				case SpecialType.System_UInt32:
				case SpecialType.System_UInt64:
				case SpecialType.System_UIntPtr:
					return;
			}

			switch (typeSymbol.ToString()) {
				case "System.Uri":
				case "System.Type":
				case "System.Reflection.Module":
				case "System.Reflection.Assembly":
				case "System.Reflection.TypeInfo":
				case "System.Reflection.MethodInfo":
				case "System.Reflection.PropertyInfo":
				case "System.Reflection.FieldInfo":
				case "System.Reflection.ConstructorInfo":
				case "System.Reflection.ParameterInfo":
				case "System.Reflection.EventInfo":
				case "System.Reflection.LocalVariableInfo":
				case "System.Reflection.MemberInfo":
				case "System.Reflection.ManifestResourceInfo":
				case "System.Reflection.MethodBase":
				case "System.Reflection.MethodBody":
				case "System.Net.IPAddress":
					return;
				case string typeSymbolName when typeSymbolName.Split('<')[0]
					is "System.Memory"
					or "System.Span": {
						Diagnostic diagnostic = Diagnostic.Create(RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_COLLECTION, typeSyntax.GetLocation(), typeSyntax.ToString());
						context.ReportDiagnostic(diagnostic);
						return;
					}
			}

			switch (typeSyntax) {
				case NullableTypeSyntax { ElementType: var elementType }: {
						if (context.SemanticModel.GetSymbolInfo(elementType, context.CancellationToken).Symbol is ITypeSymbol elementTypeSymbol) {
							AnalyzeRecordMemberType(context, elementType, elementTypeSymbol);
						}
						return;
					}
				case ArrayTypeSyntax: {
						Diagnostic diagnostic = Diagnostic.Create(RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_COLLECTION, typeSyntax.GetLocation(), typeSyntax.ToString());
						context.ReportDiagnostic(diagnostic);
						return;
					}
				case TupleTypeSyntax: {
						Diagnostic diagnostic = Diagnostic.Create(RECORDS_SHOULD_NOT_CONTAIN_REFERENCE_TO_CLASS_OR_STRUCT_TYPE, typeSyntax.GetLocation(), typeSyntax.ToString(), "tuple");
						context.ReportDiagnostic(diagnostic);
						return;
					}
				case GenericNameSyntax { TypeArgumentList: { Arguments: var genericTypeArguments } } genericNameSyntax: {
						if (typeSymbol.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.ToString() == "System.Collections.IEnumerable")) {
							if (typeSymbol.ContainingNamespace.ToString() == "System.Collections.Immutable") {
								if (typeSymbol is INamedTypeSymbol namedTypeSymbol) {
									foreach ((ITypeSymbol genericTypeArgument, TypeSyntax genericTypeSyntax) in namedTypeSymbol.TypeArguments.Zip(genericTypeArguments, (symbol, syntax) => (symbol, syntax))) {
										AnalyzeRecordMemberType(context, genericTypeSyntax, genericTypeArgument);
									}
								}
							} else {
								Diagnostic diagnostic = Diagnostic.Create(RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_COLLECTION, typeSyntax.GetLocation(), typeSyntax.ToString());
								context.ReportDiagnostic(diagnostic);
							}
						}
						return;
					}
			}

			if (typeSymbol.ToString() == "System.Collections.IEnumerable"
				|| typeSymbol.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.ToString() == "System.Collections.IEnumerable")) {
				Diagnostic diagnostic = Diagnostic.Create(RECORDS_SHOULD_NOT_CONTAIN_MUTABLE_COLLECTION, typeSyntax.GetLocation(), typeSyntax.ToString());
				context.ReportDiagnostic(diagnostic);
				return;
			}

			if (typeSymbol.TypeKind == TypeKind.Class
				&& typeSymbol.DeclaringSyntaxReferences.Length > 0
				&& typeSymbol.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken) is RecordDeclarationSyntax) {
				return;
			}

			string? illegalTypeKind = typeSymbol.SpecialType switch {
				SpecialType.System_Object => "object",
				_ => typeSymbol.TypeKind switch {
					TypeKind.Array => "array",
					TypeKind.Class => "class",
					TypeKind.Dynamic => "dynamic",
					TypeKind.Interface => "interface",
					TypeKind.Pointer => "pointer",
					TypeKind.Struct => typeSymbol.ContainingNamespace.ToString() switch {
						"System"
						or "UnitsNet" => null,
						_ => "struct"
					},
					_ => null
				}
			};
			if (illegalTypeKind is not null) {
				Diagnostic diagnostic = Diagnostic.Create(RECORDS_SHOULD_NOT_CONTAIN_REFERENCE_TO_CLASS_OR_STRUCT_TYPE, typeSyntax.GetLocation(), typeSyntax.ToString(), illegalTypeKind);
				context.ReportDiagnostic(diagnostic);
			}
		}

		private static IEnumerable<KeyValuePair<string, int>> GetEnumValues(IEnumerable<EnumMemberDeclarationSyntax> memberDeclarations) {
			int currentValue = 0;
			foreach (EnumMemberDeclarationSyntax memberDeclaration in memberDeclarations) {
				switch (memberDeclaration.EqualsValue?.Value) {
					case PrefixUnaryExpressionSyntax { OperatorToken: { } operatorToken, Operand: LiteralExpressionSyntax { Token: { } operandToken } }
					when operatorToken.IsKind(SyntaxKind.MinusToken) && operandToken.IsKind(SyntaxKind.NumericLiteralToken) && operandToken.Value is int value:
						currentValue = -value;
						yield return new KeyValuePair<string, int>(memberDeclaration.Identifier.Text, currentValue++);
						break;
					case LiteralExpressionSyntax { Token: { } literalToken }
					when literalToken.IsKind(SyntaxKind.NumericLiteralToken) && literalToken.Value is int value:
						currentValue = value;
						yield return new KeyValuePair<string, int>(memberDeclaration.Identifier.Text, currentValue++);
						break;
					default:
						yield return new KeyValuePair<string, int>(memberDeclaration.Identifier.Text, currentValue++);
						break;
				}
			}
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
			return $"{char.ToUpper(camelCaseIdentifierName[0], CultureInfo.InvariantCulture)}{camelCaseIdentifierName.Substring(1)}";
		}
		#endregion
	}
}
