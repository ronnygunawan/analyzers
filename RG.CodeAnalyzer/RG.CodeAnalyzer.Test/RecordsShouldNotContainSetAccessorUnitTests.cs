using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class RecordsShouldNotContainSetAccessorUnitTests : CodeFixVerifier {
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
            public int PropertyName { get; set; }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0015",
				Message = string.Format("'{0}' should not have set accessor because it's declared in a record", "PropertyName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 44)
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
            public int PropertyName { get; private set; }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0015",
				Message = string.Format("'{0}' should not have set accessor because it's declared in a record", "PropertyName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 44)
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
        public record RecordName
        {
            public int PropertyName { get; }
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
            private int _propertyName;
            public int PropertyName
            {
                get { return _propertyName; }
                set { _propertyName = value; }
            }
        }
    }";
			DiagnosticResult expected1 = new() {
				Id = "RG0016",
				Message = string.Format("'{0}' should not be mutable because it's declared in a record", "_propertyName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 25)
					}
			};
			DiagnosticResult expected2 = new() {
				Id = "RG0015",
				Message = string.Format("'{0}' should not have set accessor because it's declared in a record", "PropertyName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 17, 17)
					}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2);
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
            public int PropertyName { get; init; }
        }
    }";
			VerifyCSharpDiagnostic(test);
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
            public int PropertyName => 1;
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
        public class RecordName
        {
            public int PropertyName { get; set; }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
