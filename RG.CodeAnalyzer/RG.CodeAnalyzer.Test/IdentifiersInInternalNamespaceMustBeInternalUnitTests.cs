using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class IdentifiersInInternalNamespaceMustBeInternalUnitTests : CodeFixVerifier {

		[TestMethod]
		public void TestMethod1() {
			var test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod2() {
			var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1.Internal.Models
    {
        public class TypeName
        {
        }
    }";
			var expected = new DiagnosticResult {
				Id = "RG0003",
				Message = String.Format("Identifier '{0}' is declared in '{1}' namespace, and thus must be declared internal.", "TypeName", "ConsoleApplication1.Internal.Models"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 11, 9)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1.Internal.Models
    {
    internal class TypeName
        {
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider() {
			return new MakeInternalCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
