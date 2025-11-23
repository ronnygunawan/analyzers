using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class FileShouldNotExceedLineLimitTests : CodeFixVerifier {

		[TestMethod]
		public void TestEmptyFile() {
			string test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestFileWithLessThan1000Lines() {
			string test = @"
    using System;

    namespace TestNamespace
    {
        class TestClass
        {
            public void Method1() { }
            public void Method2() { }
            public void Method3() { }
        }
    }";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestFileWithExactly1000Lines() {
			string test = @"
    using System;

    namespace TestNamespace
    {
        class TestClass
        {
            public void Method1() { }
";
			for (int i = 2; i <= 991; i++) {
				test += $"            public void Method{i}() {{ }}\n";
			}
			test += @"        }
    }";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestFileWith1001Lines() {
			string test = @"
    using System;

    namespace TestNamespace
    {
        class TestClass
        {
            public void Method1() { }
";
			for (int i = 2; i <= 992; i++) {
				test += $"            public void Method{i}() {{ }}\n";
			}
			test += @"        }
    }";

			DiagnosticResult expected = new() {
				Id = "RG0043",
				Message = string.Format("File '{0}' contains {1} lines, which exceeds the maximum recommended limit of 1000 lines", "Test0.cs", 1001),
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 1, 1)
						}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestFileWith2000Lines() {
			string test = @"
    using System;

    namespace TestNamespace
    {
        class TestClass
        {
            public void Method1() { }
";
			for (int i = 2; i <= 1991; i++) {
				test += $"            public void Method{i}() {{ }}\n";
			}
			test += @"        }
    }";

			DiagnosticResult expected = new() {
				Id = "RG0043",
				Message = string.Format("File '{0}' contains {1} lines, which exceeds the maximum recommended limit of 1000 lines", "Test0.cs", 2000),
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 1, 1)
						}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
