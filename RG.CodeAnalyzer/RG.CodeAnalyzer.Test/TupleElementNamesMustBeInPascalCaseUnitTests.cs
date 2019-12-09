using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class TupleElementNamesMustBeInPascalCaseUnitTests : CodeFixVerifier {
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
            public void Test()
            {
                (int firstItem, string secondItem) tuple = (0, ""Zero"");
                (int a, string b) = (tuple.firstItem, tuple.secondItem);
            }
        }
    }";
			var expected1 = new DiagnosticResult {
				Id = "RG0008",
				Message = String.Format("'{0}' is not a proper name of a tuple element. Convert it to PascalCase.", "firstItem"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 18)
					}
			};
            var expected2 = new DiagnosticResult {
				Id = "RG0008",
				Message = String.Format("'{0}' is not a proper name of a tuple element. Convert it to PascalCase.", "secondItem"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 33)
					}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2);

			// Disabled until following issue is fixed:
			// https://github.com/dotnet/roslyn/issues/14115

			//var fixtest = @"
			// using System;
			// using System.Collections.Generic;
			// using System.Linq;
			// using System.Text;
			// using System.Threading.Tasks;
			// using System.Diagnostics;

			// namespace ConsoleApplication1
			// {
			//     public class TypeName
			//     {
			//         public void Test()
			//         {
			//             (int FirstItem, string SecondItem) tuple = (0, ""Zero"");
			//             (int a, string b) = (tuple.FirstItem, tuple.SecondItem);
			//         }
			//     }
			// }";
			//         VerifyCSharpFix(test, fixtest);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider() {
			return new MakeTupleElementPascalCaseCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
