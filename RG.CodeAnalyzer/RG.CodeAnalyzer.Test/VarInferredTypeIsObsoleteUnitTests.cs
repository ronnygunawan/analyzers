using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class VarInferredTypeIsObsoleteUnitTests : CodeFixVerifier {

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
        [Obsolete]
        class A {
        }
        
        class B {
#pragma warning disable CS0612
            public A A { get; } = new A();
#pragma warning restore CS0612
        }
        
        class C {
            public void Test() {
                var b = new B();
                var a1 = b.A;
                A a2 = b.A;
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0010",
				Message = string.Format("'{0}' is obsolete.", "A"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 24, 17)
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
        [Obsolete(""Do not use"")]
        class A {
        }
        
        class B {
#pragma warning disable CS0612
            public A A { get; } = new A();
#pragma warning restore CS0612
        }
        
        class C {
            public void Test() {
                var b = new B();
                var a1 = b.A;
                A a2 = b.A;
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0010",
				Message = string.Format("'{0}' is obsolete: '{1}'", "A", "Do not use"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 24, 17)
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
        [Obsolete(""Do not use"", false)]
        class A {
        }
        
        class B {
#pragma warning disable CS0612
            public A A { get; } = new A();
#pragma warning restore CS0612
        }
        
        class C {
            public void Test() {
                var b = new B();
                var a1 = b.A;
                A a2 = b.A;
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0010",
				Message = string.Format("'{0}' is obsolete: '{1}'", "A", "Do not use"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 24, 17)
						}
			};

			VerifyCSharpDiagnostic(test, expected);
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
        [Obsolete(""Do not use"", true)]
        class A {
        }
        
        class B {
#pragma warning disable CS0612
            public A A { get; } = new A();
#pragma warning restore CS0612
        }
        
        class C {
            public void Test() {
                var b = new B();
                var a1 = b.A;
                A a2 = b.A;
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0010",
				Message = string.Format("'{0}' is obsolete: '{1}'", "A", "Do not use"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 24, 17)
						}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
