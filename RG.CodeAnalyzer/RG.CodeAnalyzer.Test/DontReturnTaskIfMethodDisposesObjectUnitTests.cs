using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class DontReturnTaskIfMethodDisposesObjectUnitTests : CodeFixVerifier {

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
        class TypeName
        {
            public Task<int> MethodName()
            {
                using (var cts = new CancellationTokenSource())
                {
                    return Task.FromResult(0);
                }
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0002",
				Message = string.Format("Method '{0}' disposes an object and shouldn't return Task", "MethodName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 13)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public async Task<int> MethodName()
            {
                using (var cts = new CancellationTokenSource())
                {
                    return await Task.FromResult(0);
                }
            }
        }
    }";

			VerifyCSharpFix(test, fixtest);
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
        class TypeName
        {
			public async Task<int> MethodName()
			{
				using(var cts = new CancellationTokenSource())
				{
					return await Task.FromResult(0);
				}
			}
        }
    }";

			VerifyCSharpDiagnostic(test);
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
        class TypeName
        {
            public Task<int> MethodName()
            {
                using var cts = new CancellationTokenSource();
                return Task.FromResult(0);
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0002",
				Message = string.Format("Method '{0}' disposes an object and shouldn't return Task", "MethodName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 13)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public async Task<int> MethodName()
            {
                using var cts = new CancellationTokenSource();
                return await Task.FromResult(0);
            }
        }
    }";
			VerifyCSharpFix(test, fixtest);
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
        class TypeName
        {
            public Task<int> MethodName()
            {
                using (var cts = new CancellationTokenSource())
                {
                    return Task.FromResult(0);
                }
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0002",
				Message = string.Format("Method '{0}' disposes an object and shouldn't return Task", "MethodName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 13)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public async Task<int> MethodName()
            {
                using (var cts = new CancellationTokenSource())
                {
                    return await Task.FromResult(0);
                }
            }
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		/// <summary>
		/// Related issue: https://github.com/ronnygunawan/analyzers/issues/6
		/// </summary>
		[TestMethod]
		public void TestMethod6() {
			string test = @"
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public Task MethodName()
            {
                using (var cts = new CancellationTokenSource())
                {
                    return Task.CompletedTask;
                }
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0002",
				Message = string.Format("Method '{0}' disposes an object and shouldn't return Task", "MethodName"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 13)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public async Task MethodName()
            {
                using (var cts = new CancellationTokenSource())
                {
                    return ;
                }
            }
        }
    }";

			VerifyCSharpFix(test, fixtest);
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
        class TypeName
        {
            public Task<int> MethodName()
            {
                int i = 0;
                return Task.FromResult(0);
            }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider() {
			return new MakeMethodAsyncCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
