using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class RefactorExpressionBodiedPropertyTests : CodeFixVerifier {

		[TestMethod]
		public void TestEmptyCode() {
			string test = @"";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestPropertyReturningConst() {
			string test = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		private const int SixSeven = 67;
		public int MaxSize => SixSeven;
	}
}
";
			DiagnosticResult expected = new() {
				Id = "RG0042",
				Message = "Property 'MaxSize' can be refactored to auto-property with initializer",
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 7, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		private const int SixSeven = 67;
		public int MaxSize { get; } = SixSeven;
    }
}
";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void TestPropertyReturningStaticReadonly() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	public class TypeName
	{
		private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);
		public TimeSpan TTL => OneHour;
	}
}
";
			DiagnosticResult expected = new() {
				Id = "RG0042",
				Message = "Property 'TTL' can be refactored to auto-property with initializer",
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 9, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
using System;

namespace ConsoleApplication1
{
	public class TypeName
	{
		private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);
		public TimeSpan TTL { get; } = OneHour;
    }
}
";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void TestMultiplePropertiesReturningConstValues() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	public class TypeName
	{
		private const int SixSeven = 67;
		private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);
		
		public int MaxSize => SixSeven;
		public TimeSpan TTL => OneHour;
	}
}
";
			DiagnosticResult expected1 = new() {
				Id = "RG0042",
				Message = "Property 'MaxSize' can be refactored to auto-property with initializer",
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 11, 3)
					}
			};

			DiagnosticResult expected2 = new() {
				Id = "RG0042",
				Message = "Property 'TTL' can be refactored to auto-property with initializer",
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 12, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2);
		}

		[TestMethod]
		public void TestPropertyReturningNonStaticField_NoDiagnostic() {
			string test = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		private readonly int _value = 42;
		public int Value => _value;
	}
}
";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestPropertyReturningStaticNonReadonlyField_NoDiagnostic() {
			string test = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		private static int _value = 42;
		public int Value => _value;
	}
}
";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestPropertyReturningMethodCall_NoDiagnostic() {
			string test = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		public int Value => GetValue();
		
		private int GetValue() => 42;
	}
}
";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestPropertyReturningLiteral_NoDiagnostic() {
			string test = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		public int Value => 42;
	}
}
";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestPropertyWithAutoPropertySyntax_NoDiagnostic() {
			string test = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		private const int SixSeven = 67;
		public int MaxSize { get; } = SixSeven;
	}
}
";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestPropertyWithGetterBody_NoDiagnostic() {
			string test = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		private const int SixSeven = 67;
		public int MaxSize {
			get {
				return SixSeven;
			}
		}
	}
}
";
			VerifyCSharpDiagnostic(test);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider() {
			return new RefactorExpressionBodiedPropertyCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
