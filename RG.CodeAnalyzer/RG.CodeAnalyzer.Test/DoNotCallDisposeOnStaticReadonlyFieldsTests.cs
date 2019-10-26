using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class DoNotCallDisposeOnStaticReadonlyFieldsTests : CodeFixVerifier {
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
	using System.Net.Http;

    namespace ConsoleApplication1
    {
        public class TypeName : IDisposable
        {
			private static readonly HttpClient _client = new HttpClient();
			private static HttpClient _client2 = new HttpClient();

			public void Dispose() {
				_client.Dispose();
				_client2.Dispose();
			}
        }
    }";
			var expected = new DiagnosticResult {
				Id = "RG0005",
				Message = String.Format("Field '{0}' is marked 'static readonly' and should not be disposed.", "_client"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 18, 5)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
