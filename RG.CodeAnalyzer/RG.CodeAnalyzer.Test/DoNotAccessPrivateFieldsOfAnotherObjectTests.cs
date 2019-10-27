using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class DoNotAccessPrivateFieldsOfAnotherObjectTests : CodeFixVerifier {

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

    namespace ConsoleApplication1
    {
        public class TypeName
        {
			private int _count = 0;

			public SetCount(TypeName obj, int count) {
				obj._count = count;
				_count = count;
				this._count = count;
			}
        }
    }";
			var expected = new DiagnosticResult {
				Id = "RG0004",
				Message = String.Format("Private field '{0}' should not be accessed directly.", "_count"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 16, 9)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
