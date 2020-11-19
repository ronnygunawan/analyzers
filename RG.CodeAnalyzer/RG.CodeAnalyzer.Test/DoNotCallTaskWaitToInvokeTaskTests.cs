using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class DoNotCallTaskWaitToInvokeTaskTests : CodeFixVerifier {
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
			private Task _task = Task.FromResult(0);

			public void Foo() {
				_task.Wait();
			}
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0006",
				Message = string.Format("Calling Task.Wait() blocks current thread and is not recommended; Use await instead"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 16, 5)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
