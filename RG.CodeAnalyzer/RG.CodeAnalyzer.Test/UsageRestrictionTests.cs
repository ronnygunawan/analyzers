using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class UsageRestrictionTests : CodeFixVerifier {
		[TestMethod]
		public void TestRestrictedClassUsedInAllowedNamespace() {
			string test = @"
using RG.Annotations;

namespace Baz {
	[RestrictTo(""Baz"")]
	public class Foo { }

	class C : Foo { }
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestRestrictedClassUsedInDisallowedNamespace() {
			string test = @"
using RG.Annotations;

[RestrictTo(""Baz"")]
public class Foo { }

namespace Bar {
	class C : Foo { }
}
";

			DiagnosticResult expected = new() {
				Id = "RG0037",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Foo", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 12)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestRestrictedClassUsedInSubNamespace() {
			string test = @"
using RG.Annotations;

namespace Baz {
	[RestrictTo(""Baz"")]
	public class Foo { }
}

namespace Baz.Sub {
	class C : Foo { }
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestRestrictedClassFieldAccess() {
			string test = @"
using RG.Annotations;

namespace Baz {
	[RestrictTo(""Baz"")]
	public class Foo {
		public static int Value = 42;
	}
}

namespace Bar {
	class C {
		void M() {
			int x = Foo.Value;
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0037",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Foo", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 12)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestRestrictedClassVariableDeclaration() {
			string test = @"
using RG.Annotations;

namespace Baz {
	[RestrictTo(""Baz"")]
	public class Foo { }
}

namespace Bar {
	class C {
		void M() {
			Foo x = null;
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0037",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Foo", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 12, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestRestrictedStruct() {
			string test = @"
using RG.Annotations;

namespace Baz {
	[RestrictTo(""Baz"")]
	public struct Foo { }
}

namespace Bar {
	class C {
		void M() {
			Foo x = default;
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0037",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Foo", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 12, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestRestrictedEnum() {
			string test = @"
using RG.Annotations;

namespace Baz {
	[RestrictTo(""Baz"")]
	public enum Foo { A, B }
}

namespace Bar {
	class C {
		void M() {
			Foo x = Foo.A;
		}
	}
}
";

			DiagnosticResult expected1 = new() {
				Id = "RG0037",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Foo", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 12, 4)
					}
			};

			DiagnosticResult expected2 = new() {
				Id = "RG0037",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Foo", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 12, 12)
					}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2);
		}

		[TestMethod]
		public void TestRestrictedProperty() {
			string test = @"
using RG.Annotations;

namespace Baz {
	public class Foo {
		[RestrictTo(""Baz"")]
		public static int Value { get; set; }
	}
}

namespace Bar {
	class C {
		void M() {
			int x = Foo.Value;
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0037",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Value", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 16)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestRestrictedField() {
			string test = @"
using RG.Annotations;

namespace Baz {
	public class Foo {
		[RestrictTo(""Baz"")]
		public static int Value = 42;
	}
}

namespace Bar {
	class C {
		void M() {
			int x = Foo.Value;
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0037",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Value", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 16)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestRestrictedMethod() {
			string test = @"
using RG.Annotations;

namespace Baz {
	public class Foo {
		[RestrictTo(""Baz"")]
		public static void DoSomething() { }
	}
}

namespace Bar {
	class C {
		void M() {
			Foo.DoSomething();
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0037",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "DoSomething", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 8)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestNoRestriction() {
			string test = @"
using RG.Annotations;

namespace Baz {
	public class Foo { }
}

namespace Bar {
	class C : Foo { }
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestRestrictedRecord() {
			string test = @"
using RG.Annotations;

namespace Baz {
	[RestrictTo(""Baz"")]
	public record Foo { }
}

namespace Bar {
	class C {
		void M() {
			Foo x = null;
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0037",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Foo", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 12, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestRestrictedDelegate() {
			string test = @"
using RG.Annotations;

namespace Baz {
	[RestrictTo(""Baz"")]
	public delegate void Foo();
}

namespace Bar {
	class C {
		void M() {
			Foo x = null;
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0037",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Foo", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 12, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
