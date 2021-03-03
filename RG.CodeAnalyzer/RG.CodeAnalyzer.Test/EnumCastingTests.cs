using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class EnumCastingTests : CodeFixVerifier {
		[TestMethod]
		public void TestCastingBetweenIdenticalEnums() {
			string test = @"
namespace Namespace {
	enum X { A, B = 1, C }
	enum Y { A, B, C }

	class ClassName {
		void MethodName() {
			X x = (X)Y.A;
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestCastingBetweenEquivalentEnums() {
			string test = @"
namespace Namespace {
	enum X { A, B, C }
	enum Y { I, J, K }

	class ClassName {
		void MethodName() {
			X x = (X)Y.J;
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestCastingToSubsetEnum() {
			string test = @"
namespace Namespace {
	enum X { A, B, C }
	enum Y { A, B, C, D }

	class ClassName {
		void MethodName() {
			X x = (X)Y.A;
		}
	}
}
";
			DiagnosticResult expected = new() {
				Id = "RG0025",
				Message = string.Format("Casting to an incompatible enum. Value {0} is missing from '{1}'", "3", "X"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 10)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestCastingToSupersetEnum() {
			string test = @"
namespace Namespace {
	enum X { A, B, C }
	enum Y { A, B, C, D }

	class ClassName {
		void MethodName() {
			Y y = (Y)X.A;
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestCastingToNonSupersetEnum() {
			string test = @"
namespace Namespace {
	enum X { A = -1, B, C }
	enum Y { A, B, C }

	class ClassName {
		void MethodName() {
			X x = (X)Y.A;
		}
	}
}
";
			DiagnosticResult expected = new() {
				Id = "RG0025",
				Message = string.Format("Casting to an incompatible enum. Value {0} is missing from '{1}'", "2", "X"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 10)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestCastingToEnumWithSwappedOrder() {
			string test = @"
namespace Namespace {
	enum X { A, B, C }
	enum Y { A, C, B }

	class ClassName {
		void MethodName() {
			X x = (X)Y.A;
		}
	}
}
";
			DiagnosticResult expected = new() {
				Id = "RG0026",
				Message = string.Format("Possibly casting to an incompatible enum. '{0}' doesn't have the same value in '{1}' and in '{2}'", "C", "X", "Y"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 10)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestCastingToEnumWithDifferentValueNames() {
			string test = @"
namespace Namespace {
	enum X { A, B, C }
	enum Y { A, BB, C }

	class ClassName {
		void MethodName() {
			X x = (X)Y.A;
		}
	}
}
";
			DiagnosticResult expected = new() {
				Id = "RG0027",
				Message = string.Format("Possibly casting to an incompatible enum. Value {0} doesn't have the same name in '{1}' and in '{2}'", "1", "X", "Y"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 10)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
