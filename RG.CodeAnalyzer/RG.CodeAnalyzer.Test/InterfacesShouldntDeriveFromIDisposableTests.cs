using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class InterfacesShouldntDeriveFromIDisposableTests : CodeFixVerifier {
		[TestMethod]
		public void TestMethod1() {
			string test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod2() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        interface I { }
        interface J : IEnumerable { }
    }";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod3() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        interface I : IDisposable { }
        interface J : IEnumerable, IDisposable { }
    }";
			DiagnosticResult expected1 = new() {
				Id = "RG0011",
				Message = string.Format("'{0}' derives from IDisposable", "I"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 6, 9)
						}
			};
			DiagnosticResult expected2 = new() {
				Id = "RG0011",
				Message = string.Format("'{0}' derives from IDisposable", "J"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 7, 9)
						}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
