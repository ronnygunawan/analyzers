using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class ValueTypeRecordPropertyShouldBeInitializedUnitTests : CodeFixVerifier {
		[TestMethod]
		public void TestEmptyCode() {
			string test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestValueTypePropertyWithoutInitializerNotInitialized() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        public record Foo
        {
            public int X { get; init; }
            public int Y { get; init; } = 0;
        }

        public static class Program
        {
            public static void Main()
            {
                Foo foo = new Foo { };
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0020",
				Message = string.Format("'{0}' is a value type property without initializer and should be initialized", "X"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 16, 35)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestValueTypePropertyWithoutInitializerNotInitializedImplicitCreation() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        public record Foo
        {
            public int X { get; init; }
            public int Y { get; init; } = 0;
        }

        public static class Program
        {
            public static void Main()
            {
                Foo foo = new() { };
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0020",
				Message = string.Format("'{0}' is a value type property without initializer and should be initialized", "X"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 16, 33)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestValueTypePropertyWithoutInitializerNotInitializedNoInitializer() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        public record Foo
        {
            public int X { get; init; }
            public int Y { get; init; } = 0;
        }

        public static class Program
        {
            public static void Main()
            {
                Foo foo = new Foo();
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0020",
				Message = string.Format("'{0}' is a value type property without initializer and should be initialized", "X"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 16, 27)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestValueTypePropertyWithoutInitializerInitialized() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        public record Foo
        {
            public int X { get; init; }
            public int Y { get; init; } = 0;
        }

        public static class Program
        {
            public static void Main()
            {
                Foo foo = new Foo { X = 1 };
            }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestValueTypePropertyWithInitializer() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        public record Foo
        {
            public int X { get; init; }
            public int Y { get; init; } = 0;
        }

        public static class Program
        {
            public static void Main()
            {
                Foo foo = new Foo { };
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0020",
				Message = string.Format("'{0}' is a value type property without initializer and should be initialized", "X"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 16, 35)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestReferenceTypePropertyNotReported() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        public record Foo
        {
            public string Name { get; init; }
            public int Age { get; init; }
        }

        public static class Program
        {
            public static void Main()
            {
                Foo foo = new Foo { };
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0020",
				Message = string.Format("'{0}' is a value type property without initializer and should be initialized", "Age"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 16, 35)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestMutableRecordNotReported() {
			string test = @"
    using System;
    using RG.Annotations;

    namespace ConsoleApplication1
    {
        [Mutable]
        public record Foo
        {
            public int X { get; init; }
            public int Y { get; init; } = 0;
        }

        public static class Program
        {
            public static void Main()
            {
                Foo foo = new Foo { };
            }
        }
    }";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMultipleValueTypePropertiesWithoutInitializers() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        public record Foo
        {
            public int X { get; init; }
            public int Y { get; init; }
            public int Z { get; init; } = 0;
        }

        public static class Program
        {
            public static void Main()
            {
                Foo foo = new Foo { };
            }
        }
    }";
			DiagnosticResult expected1 = new() {
				Id = "RG0020",
				Message = string.Format("'{0}' is a value type property without initializer and should be initialized", "X"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 17, 35)
					}
			};
			DiagnosticResult expected2 = new() {
				Id = "RG0020",
				Message = string.Format("'{0}' is a value type property without initializer and should be initialized", "Y"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 17, 35)
					}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2);
		}

		[TestMethod]
		public void TestPartialInitialization() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        public record Foo
        {
            public int X { get; init; }
            public int Y { get; init; }
            public int Z { get; init; } = 0;
        }

        public static class Program
        {
            public static void Main()
            {
                Foo foo = new Foo { X = 1 };
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0020",
				Message = string.Format("'{0}' is a value type property without initializer and should be initialized", "Y"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 17, 35)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestNullableValueTypeNotReported() {
			string test = @"
    using System;

    namespace ConsoleApplication1
    {
        public record Foo
        {
            public int? X { get; init; }
            public int Y { get; init; }
        }

        public static class Program
        {
            public static void Main()
            {
                Foo foo = new Foo { };
            }
        }
    }";
			DiagnosticResult expected = new() {
				Id = "RG0020",
				Message = string.Format("'{0}' is a value type property without initializer and should be initialized", "Y"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 16, 35)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
