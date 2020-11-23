using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class WithExpressionShouldntBeUsedOutsideRecordDeclarationUnitTests : CodeFixVerifier {

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
        record A(int X);
        
        class B {
            public void Test() {
                var a = new A(0);
                a = a with { X = 1 };
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0013",
				Message = "'with' used outside 'ConsoleApplication1.A'",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 16, 23)
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
        record A(int X) {
            public A WithX(int x) => this with { X = x };
        }
        
        class B {
            public void Test() {
                var a = new A(0);
                a = a with { X = 1 };
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0013",
				Message = "'with' used outside 'ConsoleApplication1.A'",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 18, 23)
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
        record A(int X) {
            public A WithX(int x) => this with { X = x };
        }
        
        class B {
            public void Test() {
                var a = new A(0);
                a = a.WithX(1);
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
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        record A(int X, int Y) {
            public B WithZ(int z) => new(this, z);
        }

        record B(int X, int Y, int Z) : A(X, Y) {
            public B(A a, int z) : this(a.X, a.Y, z) { }
        }
        
        class C {
            public void Test() {
                var a = new A(1, 2);
                var b = a.WithZ(3);
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
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        record A(int X, int Y) {
            public B WithZ(int z) => new(this, z);
        }

        record B(int X, int Y, int Z) : A(X, Y) {
            public B(A a, int z) : this(a.X, a.Y, z) { }
            public B WithXY(int x, int y) {
                A a = (A)this;
                return a with { X = x, Y = y }.WithZ(Z);
            }
        }
        
        class C {
            public void Test() {
                var a = new A(1, 2);
                var b = a.WithZ(3);
            }
        }
    }";

			DiagnosticResult expected = new() {
				Id = "RG0013",
				Message = "'with' used outside 'ConsoleApplication1.A'",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 19, 26)
						}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
