using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class SuppressMessageJustificationTests : CodeFixVerifier {

		[TestMethod]
		public void TestEmptyJustification() {
			string test = @"
using System.Diagnostics.CodeAnalysis;

namespace ConsoleApplication1
{
	public class TypeName
	{
		[SuppressMessage(""Category"", ""RG0001"", Justification = """")]
		public void MethodName() {
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0038",
				Message = "Justification is required for suppressing message 'RG0001'",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestWhitespaceOnlyJustification() {
			string test = @"
using System.Diagnostics.CodeAnalysis;

namespace ConsoleApplication1
{
	public class TypeName
	{
		[SuppressMessage(""Category"", ""RG0002"", Justification = ""   "")]
		public void MethodName() {
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0038",
				Message = "Justification is required for suppressing message 'RG0002'",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestPendingJustification() {
			string test = @"
using System.Diagnostics.CodeAnalysis;

namespace ConsoleApplication1
{
	public class TypeName
	{
		[SuppressMessage(""Category"", ""RG0003"", Justification = ""<Pending>"")]
		public void MethodName() {
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0038",
				Message = "Justification is required for suppressing message 'RG0003'",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestValidJustification() {
			string test = @"
using System.Diagnostics.CodeAnalysis;

namespace ConsoleApplication1
{
	public class TypeName
	{
		[SuppressMessage(""Category"", ""RG0001"", Justification = ""This is a valid reason"")]
		public void MethodName() {
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestNoJustificationParameter() {
			string test = @"
using System.Diagnostics.CodeAnalysis;

namespace ConsoleApplication1
{
	public class TypeName
	{
		[SuppressMessage(""Category"", ""RG0001"")]
		public void MethodName() {
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0038",
				Message = "Justification is required for suppressing message 'RG0001'",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMultipleAttributes() {
			string test = @"
using System.Diagnostics.CodeAnalysis;

namespace ConsoleApplication1
{
	public class TypeName
	{
		[SuppressMessage(""Category"", ""RG0001"", Justification = """")]
		[SuppressMessage(""Category"", ""RG0002"", Justification = ""Valid reason"")]
		public void MethodName() {
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0038",
				Message = "Justification is required for suppressing message 'RG0001'",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestFullyQualifiedAttributeName() {
			string test = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage(""Category"", ""RG0001"", Justification = """")]
		public void MethodName() {
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0038",
				Message = "Justification is required for suppressing message 'RG0001'",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 6, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
