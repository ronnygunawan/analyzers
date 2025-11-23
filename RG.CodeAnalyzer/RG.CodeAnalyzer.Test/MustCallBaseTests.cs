using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class MustCallBaseTests : CodeFixVerifier {
		[TestMethod]
		public void TestEmptyCode() {
			string test = @"";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestMethodWithoutMustCallBase() {
			string test = @"
namespace TestNamespace {
	class A {
		protected virtual void Foo() {
		}
	}

	class B : A {
		protected override void Foo() {
		}
	}
}
";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestOverrideCallsBase() {
			string test = @"
namespace RG.Annotations {
	using System;
	[AttributeUsage(AttributeTargets.Method)]
	public class MustCallBaseAttribute : Attribute { }
}

namespace TestNamespace {
	using RG.Annotations;

	class A {
		[MustCallBase]
		protected virtual void Foo() {
		}
	}

	class B : A {
		protected override void Foo() {
			base.Foo();
		}
	}
}
";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestOverrideDoesNotCallBase() {
			string test = @"
namespace RG.Annotations {
	using System;
	[AttributeUsage(AttributeTargets.Method)]
	public class MustCallBaseAttribute : Attribute { }
}

namespace TestNamespace {
	using RG.Annotations;

	class A {
		[MustCallBase]
		protected virtual void Foo() {
		}
	}

	class B : A {
		protected override void Foo() {
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0040",
				Message = "Method 'Foo' must call base.Foo() because the base method is marked with [MustCallBase]",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 18, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestOverrideWithExpressionBodyCallsBase() {
			string test = @"
namespace RG.Annotations {
	using System;
	[AttributeUsage(AttributeTargets.Method)]
	public class MustCallBaseAttribute : Attribute { }
}

namespace TestNamespace {
	using RG.Annotations;

	class A {
		[MustCallBase]
		protected virtual void Foo() {
		}
	}

	class B : A {
		protected override void Foo() => base.Foo();
	}
}
";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestOverrideWithExpressionBodyDoesNotCallBase() {
			string test = @"
namespace RG.Annotations {
	using System;
	[AttributeUsage(AttributeTargets.Method)]
	public class MustCallBaseAttribute : Attribute { }
}

namespace TestNamespace {
	using RG.Annotations;
	using System;

	class A {
		[MustCallBase]
		protected virtual void Foo() {
		}
	}

	class B : A {
		protected override void Foo() => Console.WriteLine(""Test"");
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0040",
				Message = "Method 'Foo' must call base.Foo() because the base method is marked with [MustCallBase]",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 19, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestOverrideCallsBaseInComplexBody() {
			string test = @"
namespace RG.Annotations {
	using System;
	[AttributeUsage(AttributeTargets.Method)]
	public class MustCallBaseAttribute : Attribute { }
}

namespace TestNamespace {
	using RG.Annotations;
	using System;

	class A {
		[MustCallBase]
		protected virtual void Foo() {
		}
	}

	class B : A {
		protected override void Foo() {
			Console.WriteLine(""Before"");
			base.Foo();
			Console.WriteLine(""After"");
		}
	}
}
";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestNestedOverrides() {
			string test = @"
namespace RG.Annotations {
	using System;
	[AttributeUsage(AttributeTargets.Method)]
	public class MustCallBaseAttribute : Attribute { }
}

namespace TestNamespace {
	using RG.Annotations;

	class A {
		[MustCallBase]
		protected virtual void Foo() {
		}
	}

	class B : A {
		protected override void Foo() {
			base.Foo();
		}
	}

	class C : B {
		protected override void Foo() {
			base.Foo();
		}
	}
}
";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestNestedOverridesWithoutBaseCall() {
			string test = @"
namespace RG.Annotations {
	using System;
	[AttributeUsage(AttributeTargets.Method)]
	public class MustCallBaseAttribute : Attribute { }
}

namespace TestNamespace {
	using RG.Annotations;

	class A {
		[MustCallBase]
		protected virtual void Foo() {
		}
	}

	class B : A {
		protected override void Foo() {
			base.Foo();
		}
	}

	class C : B {
		protected override void Foo() {
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0040",
				Message = "Method 'Foo' must call base.Foo() because the base method is marked with [MustCallBase]",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 24, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
