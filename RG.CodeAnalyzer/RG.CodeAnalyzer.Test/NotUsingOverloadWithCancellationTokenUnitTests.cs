using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class NotUsingOverloadWithCancellationTokenUnitTests : CodeFixVerifier {
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
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync(CancellationToken cancellationToken)
            {
                await BarAsync(1);
            }

            public async Task BarAsync(int id, CancellationToken cancellationToken = default)
            {
            }
        }
    }";
			var expected = new DiagnosticResult {
				Id = "RG0009",
				Message = "This method has an overload that accepts CancellationToken.",
                Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 16, 23)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync(CancellationToken cancellationToken)
            {
                await BarAsync(1, cancellationToken);
            }

            public async Task BarAsync(int id, CancellationToken cancellationToken = default)
            {
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
		}

        [TestMethod]
        public void TestMethod3() {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync(CancellationToken cancellationToken)
            {
                await BarAsync(1);
            }

            public async Task BarAsync(int id)
            {
            }

            public async Task BarAsync(string s, CancellationToken cancellationToken)
            {
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
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync(CancellationToken cancellationToken)
            {
                await BarAsync(1);
            }

            public async Task BarAsync(int id)
            {
            }

            public async Task BarAsync(int d, CancellationToken cancellationToken)
            {
            }
        }
    }";
            var expected = new DiagnosticResult {
                Id = "RG0009",
                Message = "This method has an overload that accepts CancellationToken.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 16, 23)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync(CancellationToken cancellationToken)
            {
                await BarAsync(1, cancellationToken);
            }

            public async Task BarAsync(int id)
            {
            }

            public async Task BarAsync(int d, CancellationToken cancellationToken)
            {
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() {
			return new AddCancellationTokenCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
