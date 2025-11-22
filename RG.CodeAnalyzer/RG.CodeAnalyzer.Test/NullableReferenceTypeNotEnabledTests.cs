using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class NullableReferenceTypeNotEnabledTests : CodeFixVerifier {

		[TestMethod]
		public void TestEmptyCode() {
			string test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestNullableEnabled() {
			string test = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		public string? Name { get; set; }
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
