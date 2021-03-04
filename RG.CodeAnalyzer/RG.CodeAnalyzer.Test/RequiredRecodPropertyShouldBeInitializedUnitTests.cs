using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class RequiredRecodPropertyShouldBeInitializedUnitTests : CodeFixVerifier {
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
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public string? FirstName { get; init; }

            [Required]
            public string? LastName { get; init; }

            public readonly int X, Y;
        }

        public static class ClassName
        {
            public static void Main()
            {
                RecordName rec = new RecordName { };
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0019",
				Message = string.Format("'{0}' is a required property and should be initialized", "LastName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 26, 49)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod3() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public string? FirstName { get; init; }

            [Required]
            public string? LastName { get; init; }

            public readonly int X, Y;
        }

        public static class ClassName
        {
            public static void Main()
            {
                RecordName rec = new() { };
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0019",
				Message = string.Format("'{0}' is a required property and should be initialized", "LastName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 26, 40)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod4() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public string? FirstName { get; init; }

            [Required]
            public string? LastName { get; init; }
        }

        public static class ClassName
        {
            public static void Main()
            {
                RecordName rec = new RecordName
                {
                    LastName = ""Doe""
                };
            }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod5() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public string? FirstName { get; init; }

            [Required]
            public string? LastName { get; init; }
        }

        public static class ClassName
        {
            public static void Main()
            {
                RecordName rec = new()
                {
                    LastName = ""Doe""
                };
            }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod6() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public string? FirstName { get; init; }

            public string? @LastName { get; init; }
        }

        public static class ClassName
        {
            public static void Main()
            {
                RecordName rec = new RecordName { };
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0019",
				Message = string.Format("'{0}' is a required property and should be initialized", "LastName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 23, 49)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod7() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public string? FirstName { get; init; }

            [Required]
            public string? LastName { get; init; }

            public readonly int X, Y;
        }

        public static class ClassName
        {
            public static void Main()
            {
                RecordName rec = new();
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0019",
				Message = string.Format("'{0}' is a required property and should be initialized", "LastName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 26, 34)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
