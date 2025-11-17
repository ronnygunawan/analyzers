using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class UseOverloadWithoutCancellationTokenIfArgumentIsDefaultUnitTests : CodeFixVerifier {
		[TestMethod]
		public void TestEmptyCode() {
			string test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestCancellationTokenNone() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                var list = await BarAsync(1, CancellationToken.None);
            }

            public async Task<List<int>> BarAsync(int id)
            {
                return new List<int>();
            }

            public async Task<List<int>> BarAsync(int id, CancellationToken cancellationToken)
            {
                return new List<int>();
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0033",
				Message = "Use overload without CancellationToken instead",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 34)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                var list = await BarAsync(1);
            }

            public async Task<List<int>> BarAsync(int id)
            {
                return new List<int>();
            }

            public async Task<List<int>> BarAsync(int id, CancellationToken cancellationToken)
            {
                return new List<int>();
            }
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void TestDefaultLiteral() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                var list = await BarAsync(1, default);
            }

            public async Task<List<int>> BarAsync(int id)
            {
                return new List<int>();
            }

            public async Task<List<int>> BarAsync(int id, CancellationToken cancellationToken)
            {
                return new List<int>();
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0033",
				Message = "Use overload without CancellationToken instead",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 34)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                var list = await BarAsync(1);
            }

            public async Task<List<int>> BarAsync(int id)
            {
                return new List<int>();
            }

            public async Task<List<int>> BarAsync(int id, CancellationToken cancellationToken)
            {
                return new List<int>();
            }
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void TestDefaultExpression() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                var list = await BarAsync(1, default(CancellationToken));
            }

            public async Task<List<int>> BarAsync(int id)
            {
                return new List<int>();
            }

            public async Task<List<int>> BarAsync(int id, CancellationToken cancellationToken)
            {
                return new List<int>();
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0033",
				Message = "Use overload without CancellationToken instead",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 34)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                var list = await BarAsync(1);
            }

            public async Task<List<int>> BarAsync(int id)
            {
                return new List<int>();
            }

            public async Task<List<int>> BarAsync(int id, CancellationToken cancellationToken)
            {
                return new List<int>();
            }
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void TestNoWarningWhenActualTokenPassed() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync(CancellationToken cancellationToken)
            {
                var list = await BarAsync(1, cancellationToken);
            }

            public async Task<List<int>> BarAsync(int id)
            {
                return new List<int>();
            }

            public async Task<List<int>> BarAsync(int id, CancellationToken cancellationToken)
            {
                return new List<int>();
            }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestNoWarningWhenNoOverloadExists() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                var list = await BarAsync(1, CancellationToken.None);
            }

            public async Task<List<int>> BarAsync(int id, CancellationToken cancellationToken)
            {
                return new List<int>();
            }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestNoWarningWhenOverloadHasDifferentSignature() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                var list = await BarAsync(1, CancellationToken.None);
            }

            public async Task<List<int>> BarAsync(string id)
            {
                return new List<int>();
            }

            public async Task<List<int>> BarAsync(int id, CancellationToken cancellationToken)
            {
                return new List<int>();
            }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestSystemThreadingCancellationToken() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                var list = await BarAsync(1, System.Threading.CancellationToken.None);
            }

            public async Task<List<int>> BarAsync(int id)
            {
                return new List<int>();
            }

            public async Task<List<int>> BarAsync(int id, System.Threading.CancellationToken cancellationToken)
            {
                return new List<int>();
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0033",
				Message = "Use overload without CancellationToken instead",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 34)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                var list = await BarAsync(1);
            }

            public async Task<List<int>> BarAsync(int id)
            {
                return new List<int>();
            }

            public async Task<List<int>> BarAsync(int id, System.Threading.CancellationToken cancellationToken)
            {
                return new List<int>();
            }
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider() {
			return new RemoveCancellationTokenCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
