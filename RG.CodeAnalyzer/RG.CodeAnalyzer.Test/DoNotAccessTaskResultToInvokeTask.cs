using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class DoNotAccessTaskResultToInvokeTask : CodeFixVerifier {
		[TestMethod]
		public void TestMethod1() {
			string test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod2() {
			string test = @"
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
			private Task<int> _task = Task.FromResult(0);

			public void Foo() {
				int i = _task.Result;
			}
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0007",
				Message = string.Format("Accessing Task<>.Result blocks current thread and is not recommended; Use await instead"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 16, 13)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
