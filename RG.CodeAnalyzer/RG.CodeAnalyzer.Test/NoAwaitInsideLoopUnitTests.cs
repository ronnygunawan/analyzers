using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using RG.CodeAnalyzer;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class NoAwaitInsideLoopUnitTest : CodeFixVerifier {

		//No diagnostics expected to show up
		[TestMethod]
		public void TestMethod1() {
			var test = @"";

			VerifyCSharpDiagnostic(test);
		}

		//Diagnostic and CodeFix both triggered and checked for
		[TestMethod]
		public void TestMethod2() {
			var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
			public async Task MethodName()
			{
				for(;;)
				{
					await Task.Delay(100);
				}
			}
        }
    }";
			var expected = new DiagnosticResult {
				Id = "RG0001",
				Message = String.Format("Asynchronous operation awaited inside {0}.", "for loop"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 17, 6)
						}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
