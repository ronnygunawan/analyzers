using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class StaticClassWithExtensionMethodsShouldHaveExtensionsSuffixUnitTests : CodeFixVerifier {
		[TestMethod]
		public void TestEmptyCode() {
			string test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestStaticClassWithExtensionMethodsWithoutSuffix() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        static class Utilities
        {
            public static void Foo(this string bar)
            {
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = string.Format("'{0}' contains extension methods and should have an 'Extensions' suffix", "Utilities"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 6, 22)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
    using System;

    namespace ConsoleApplication1
    {
        static class UtilitiesExtensions
        {
            public static void Foo(this string bar)
            {
            }
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void TestStaticClassWithExtensionMethodsWithSuffix() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        static class StringExtensions
        {
            public static void Foo(this string bar)
            {
            }
        }
    }";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestStaticClassWithoutExtensionMethods() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        static class Utilities
        {
            public static void Foo()
            {
            }
        }
    }";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestNonStaticClass() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        public class Utilities
        {
            public void Foo(this string bar)
            {
            }
        }
    }";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestStaticClassWithMultipleExtensionMethods() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        static class Helper
        {
            public static void Foo(this string bar)
            {
            }

            public static int Count(this string bar)
            {
                return bar.Length;
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = string.Format("'{0}' contains extension methods and should have an 'Extensions' suffix", "Helper"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 6, 22)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
    using System;

    namespace ConsoleApplication1
    {
        static class HelperExtensions
        {
            public static void Foo(this string bar)
            {
            }

            public static int Count(this string bar)
            {
                return bar.Length;
            }
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void TestStaticClassWithMixedMethods() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        static class Utils
        {
            public static void Foo(this string bar)
            {
            }

            public static void Bar()
            {
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = string.Format("'{0}' contains extension methods and should have an 'Extensions' suffix", "Utils"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 6, 22)
					}
			};

			VerifyCSharpDiagnostic(test, expected);

			string fixtest = @"
    using System;

    namespace ConsoleApplication1
    {
        static class UtilsExtensions
        {
            public static void Foo(this string bar)
            {
            }

            public static void Bar()
            {
            }
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider() {
			return new RenameClassWithExtensionsSuffixCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
