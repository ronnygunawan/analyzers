using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class NoAwaitInsideLoopUnitTest : CodeFixVerifier {

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
			DiagnosticResult expected = new() {
				Id = "RG0001",
				Message = string.Format("Asynchronous operation awaited inside {0}", "for loop"),
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 17, 6)
						}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod3() {
			string test = @"
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
				List<int> numbers = new List<int> { 1, 2, 3 };
				foreach (int i in numbers)
				{
					await Task.Delay(100);
				}
			}
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0001",
				Message = string.Format("Asynchronous operation awaited inside {0}", "foreach loop"),
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 18, 6)
						}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod4() {
			string test = @"
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
				foreach (int i in await GetNumbersAsync())
				{
					Console.WriteLine(i);
				}
			}

			public Task<int> GetNumbersAsync() {
				return Task.FromResult(new List<int> { 1, 2, 3 });
			}
        }
    }";

			VerifyCSharpDiagnostic(test);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
