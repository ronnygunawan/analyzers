using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class UseOverloadWithoutCancellationTokenIfNoneSuppliedUnitTests : CodeFixVerifier {
		[TestMethod]
		public void TestMethod1_EmptyCode_NoDiagnostic() {
			string test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod2_CancellationTokenNone_WithOverload_Diagnostic() {
			string test = @"
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                await BarAsync(1, CancellationToken.None);
            }

            public async Task BarAsync(int id)
            {
                await Task.Delay(1);
            }

            public async Task BarAsync(int id, CancellationToken cancellationToken)
            {
                await Task.Delay(1, cancellationToken);
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = "Use overload without CancellationToken parameter instead",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 35)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod3_DefaultLiteral_WithOverload_Diagnostic() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                await BarAsync(1, default);
            }

            public async Task BarAsync(int id)
            {
            }

            public async Task BarAsync(int id, CancellationToken cancellationToken)
            {
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = "Use overload without CancellationToken parameter instead",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 35)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod4_DefaultExpression_WithOverload_Diagnostic() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                await BarAsync(1, default(CancellationToken));
            }

            public async Task BarAsync(int id)
            {
            }

            public async Task BarAsync(int id, CancellationToken cancellationToken)
            {
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = "Use overload without CancellationToken parameter instead",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 35)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMethod5_CancellationTokenNone_NoOverload_NoDiagnostic() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                await BarAsync(1, CancellationToken.None);
            }

            public async Task BarAsync(int id, CancellationToken cancellationToken)
            {
            }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod6_ActualCancellationToken_WithOverload_NoDiagnostic() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

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

            public async Task BarAsync(int id, CancellationToken cancellationToken)
            {
            }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod7_CancellationTokenNone_DifferentOverloadSignature_NoDiagnostic() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TypeName
        {
            public async Task FooAsync()
            {
                await BarAsync(1, CancellationToken.None);
            }

            public async Task BarAsync(string s)
            {
            }

            public async Task BarAsync(int id, CancellationToken cancellationToken)
            {
            }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethod8_RealWorldExample_ToListAsync_Diagnostic() {
			string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    namespace ConsoleApplication1
    {
        public class Item { }

        public class DbContext : Microsoft.EntityFrameworkCore.DbContext
        {
            public DbSet<Item> Items { get; set; }
        }

        public class TypeName
        {
            private DbContext _dbContext;

            public async Task<List<Item>> GetItemsAsync()
            {
                return await _dbContext.Items.ToListAsync(CancellationToken.None);
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = "Use overload without CancellationToken parameter instead",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 24, 64)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
