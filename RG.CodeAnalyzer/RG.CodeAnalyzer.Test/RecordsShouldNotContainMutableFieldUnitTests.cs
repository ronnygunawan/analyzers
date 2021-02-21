using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class RecordsShouldNotContainMutableFieldUnitTests : CodeFixVerifier {
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
        public record RecordName
        {
            private int _x;
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0016",
				Message = string.Format("'{0}' should not be mutable because it's declared in a record", "_x"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 25)
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
        public record RecordName
        {
            private int _x, _y;
        }
    }";
			DiagnosticResult expected1 = new() {
				Id = "RG0016",
				Message = string.Format("'{0}' should not be mutable because it's declared in a record", "_x"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 25)
					}
			};

			DiagnosticResult expected2 = new() {
				Id = "RG0016",
				Message = string.Format("'{0}' should not be mutable because it's declared in a record", "_y"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 29)
					}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2);
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
        public record RecordName
        {
            private const int X;
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod5() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            private readonly int _x;
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod6() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            private static int _x;
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0016",
				Message = string.Format("'{0}' should not be mutable because it's declared in a record", "_x"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 32)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod7() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            private static readonly int _x;
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod8() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            private const int? X;
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod9() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            private const object X;
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0018",
				Message = string.Format("'{0}' is {1} type and should not be used in a record", "object", "object"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 27)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod10() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Reflection;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            private readonly Type? T;
            private readonly MethodInfo? M; 
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
