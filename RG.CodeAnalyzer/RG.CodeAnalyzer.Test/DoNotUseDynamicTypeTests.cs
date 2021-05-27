using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class DoNotUseDynamicTypeTests : CodeFixVerifier {

		[TestMethod]
		public void TestMethod1() {
			string test = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		public dynamic MethodName(dynamic i) {
			dynamic x = i * i;
			x += i;
			return x;
		}
	}
}
";

			DiagnosticResult expected1 = new() {
				Id = "RG0031",
				Message = "Do not use dynamic type",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 6, 10)
					}
			};

			DiagnosticResult expected2 = new() {
				Id = "RG0031",
				Message = "Do not use dynamic type",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 6, 29)
					}
			};

			DiagnosticResult expected3 = new() {
				Id = "RG0031",
				Message = "Do not use dynamic type",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 7, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2, expected3);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
