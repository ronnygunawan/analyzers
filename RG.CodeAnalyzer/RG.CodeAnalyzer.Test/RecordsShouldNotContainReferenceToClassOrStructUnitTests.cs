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
	public class RecordsShouldNotContainReferenceToClassOrStructUnitTests : CodeFixVerifier {
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
    using System.Collections.Immutable;
    using System
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public DateTime Value { get; init; }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod3() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public DateTime? Value { get; init; }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod4() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public DateTimeOffset Value { get; init; }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod5() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public TimeSpan Value { get; init; }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod6() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public (string A, string B) Value { get; init; }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0018",
				Message = string.Format("'{0}' is {1} type and should not be used in a record", "(string A, string B)", "tuple"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 20)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod7() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public Action Value { get; init; }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod8() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public record RecordName
        {
            public SecondRecordName Value { get; init; }
        }

        public record SecondRecordName
        {
            public int X { get; init; }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
