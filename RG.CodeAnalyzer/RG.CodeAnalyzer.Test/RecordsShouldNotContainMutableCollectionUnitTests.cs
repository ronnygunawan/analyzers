using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class RecordsShouldNotContainMutableCollectionUnitTests : CodeFixVerifier {
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
            public int[]? Values { get; init; }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0017",
				Message = string.Format("'{0}' is a mutable collection and should not be used in a record", "int[]"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 20)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
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
            public List<int> Values { get; init; }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0017",
				Message = string.Format("'{0}' is a mutable collection and should not be used in a record", "List<int>"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 20)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod4() {
			string test = @"
    using System;
    using System.Collections;
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
            public IEnumerable Values { get; init; }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0017",
				Message = string.Format("'{0}' is a mutable collection and should not be used in a record", "IEnumerable"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 20)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
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
            public IReadOnlyList<int> Values { get; init; }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0017",
				Message = string.Format("'{0}' is a mutable collection and should not be used in a record", "IReadOnlyList<int>"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 20)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
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
            public Dictionary<int, int> Values { get; init; }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0017",
				Message = string.Format("'{0}' is a mutable collection and should not be used in a record", "Dictionary<int, int>"),
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
            public ImmutableList<int> Values { get; init; }
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
            public ImmutableList<List<int>> Values { get; init; }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0017",
				Message = string.Format("'{0}' is a mutable collection and should not be used in a record", "List<int>"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 34)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod9() {
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
        public record RecordName()
        {
            public ImmutableList<int[]> Values { get; init; }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0017",
				Message = string.Format("'{0}' is a mutable collection and should not be used in a record", "int[]"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 34)
					}
			};

			VerifyCSharpDiagnostic(test, expected, expected);
		}

		[TestMethod]
		public void TestMethod10() {
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
            public Memory<int> Values { get; init; }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0017",
				Message = string.Format("'{0}' is a mutable collection and should not be used in a record", "Memory<int>"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 20)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod11() {
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
        (
            ImmutableList<int> Values
        );
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod12() {
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
        (
            List<int> Values
        );
    }";
			DiagnosticResult expected = new() {
				Id = "RG0017",
				Message = string.Format("'{0}' is a mutable collection and should not be used in a record", "List<int>"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 13)
					}
			};

			VerifyCSharpDiagnostic(test, expected, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
