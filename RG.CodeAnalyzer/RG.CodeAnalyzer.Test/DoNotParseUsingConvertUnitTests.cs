using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class DoNotParseUsingConvertUnitTests : CodeFixVerifier {
		[TestMethod]
		public void TestMethod1() {
			string test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod2() {
			string test = @"
#pragma warning disable CS8019
    using System;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
			public void Foo() {
				int i = Convert.ToInt32(""0"");
			}
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0014",
				Message = string.Format("Parsing '{0}' using 'Convert.{1}'", "int", "ToInt32"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 10, 13)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
#pragma warning disable CS8019
    using System;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
			public void Foo() {
				int i = int.Parse(""0"");
			}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void TestMethod3() {
			string test = @"
#pragma warning disable CS8019
    using System;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
			public void Foo() {
				DateTime d = Convert.ToDateTime("""");
			}
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0014",
				Message = string.Format("Parsing '{0}' using 'Convert.{1}'", "DateTime", "ToDateTime"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 10, 18)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
#pragma warning disable CS8019
    using System;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
			public void Foo() {
				DateTime d = DateTime.Parse("""");
			}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void TestMethod4() {
			string test = @"
#pragma warning disable CS8019
    using System;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
			public void Foo() {
				int i = Convert.ToInt32(0L);
			}
        }
    }";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod5() {
			string test = @"
#pragma warning disable CS8019
    using System;
    using System.Globalization;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
			public void Foo() {
				int i = Convert.ToInt32(""0"", CultureInfo.InvariantCulture);
			}
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0014",
				Message = string.Format("Parsing '{0}' using 'Convert.{1}'", "int", "ToInt32"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 11, 13)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
#pragma warning disable CS8019
    using System;
    using System.Globalization;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
			public void Foo() {
				int i = int.Parse(""0"", CultureInfo.InvariantCulture);
			}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider() {
			return new ChangeToParseCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
