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
	[RestrictTo(Namespace = ""Baz"")]
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

[RestrictTo(Namespace = ""Baz"")]
public class Foo { }

namespace Bar {
	class C : Foo { }
}
";

			DiagnosticResult expected = new() {
				Id = "RG0034",
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
	[RestrictTo(Namespace = ""Baz"")]
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
	[RestrictTo(Namespace = ""Baz"")]
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
				Id = "RG0034",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Foo", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 11)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestRestrictedClassVariableDeclaration() {
			string test = @"
using RG.Annotations;

namespace Baz {
	[RestrictTo(Namespace = ""Baz"")]
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
				Id = "RG0034",
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
	[RestrictTo(Namespace = ""Baz"")]
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
				Id = "RG0034",
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
	[RestrictTo(Namespace = ""Baz"")]
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
				Id = "RG0034",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Foo", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 12, 4)
					}
			};

			DiagnosticResult expected2 = new() {
				Id = "RG0034",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Foo", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 12, 11)
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
		[RestrictTo(Namespace = ""Baz"")]
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
				Id = "RG0034",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Value", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 15)
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
		[RestrictTo(Namespace = ""Baz"")]
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
				Id = "RG0034",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Value", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 15)
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
		[RestrictTo(Namespace = ""Baz"")]
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
				Id = "RG0034",
				Message = string.Format("Usage of '{0}' is only allowed in namespace '{1}'", "Foo", "Baz"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 14, 4)
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
	[RestrictTo(Namespace = ""Baz"")]
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
				Id = "RG0034",
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
	[RestrictTo(Namespace = ""Baz"")]
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
				Id = "RG0034",
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
