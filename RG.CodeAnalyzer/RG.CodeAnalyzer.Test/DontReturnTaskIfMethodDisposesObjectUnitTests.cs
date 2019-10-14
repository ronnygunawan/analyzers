using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class DontReturnTaskIfMethodDisposesObjectUnitTests : CodeFixVerifier {

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
        class TypeName
        {
			public Task<int> MethodName()
			{
				using(var cts = new CancellationTokenSource())
				{
					return Task.FromResult(0);
				}
			}
        }
    }";
			var expected = new DiagnosticResult {
				Id = "RG0002",
				Message = String.Format("Method '{0}' disposes an object and shouldn't return Task.", "MethodName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 5)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod3() {
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
			public async Task<int> MethodName()
			{
				using(var cts = new CancellationTokenSource())
				{
					return await Task.FromResult(0);
				}
			}
        }
    }";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod4() {
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
			public Task<int> MethodName()
			{
				using var cts = new CancellationTokenSource();
				return Task.FromResult(0);
			}
        }
    }";
			var expected = new DiagnosticResult {
				Id = "RG0002",
				Message = String.Format("Method '{0}' disposes an object and shouldn't return Task.", "MethodName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 5)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
