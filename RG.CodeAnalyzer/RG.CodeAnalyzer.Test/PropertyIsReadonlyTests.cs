using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class PropertyIsReadonlyTests : CodeFixVerifier {
		[TestMethod]
		public void TestAssignment() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName(int @readonlyParamName, int mutableParamName) {
			readonlyParamName = 1;
			mutableParamName = 1;
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0022",
				Message = string.Format("'{0}' is a readonly parameter", "readonlyParamName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 5, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestRefAndOutDeclaration() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName(ref int @param1, out int @param2, in int @param3) {
		}
	}
}";

			DiagnosticResult expected1 = new() {
				Id = "RG0023",
				Message = string.Format("'{0}' parameter '{1}' cannot be readonly", "ref", "param1"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 4, 19)
				}
			};
			DiagnosticResult expected2 = new() {
				Id = "RG0023",
				Message = string.Format("'{0}' parameter '{1}' cannot be readonly", "out", "param2"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 4, 36)
				}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2);
		}

		[TestMethod]
		public void TestOptionalParameterDeclaration() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName(int @readonlyParamName = 0, int mutableParamName = 0) {
			readonlyParamName = 1;
			mutableParamName = 1;
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0022",
				Message = string.Format("'{0}' is a readonly parameter", "readonlyParamName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 5, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestIncrementAndDecrementOperator() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName(int @readonlyParamName, int mutableParamName) {
			readonlyParamName++;
			readonlyParamName--;
			++readonlyParamName;
			--readonlyParamName;
			mutableParamName++;
			mutableParamName--;
			++mutableParamName;
			--mutableParamName;
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0022",
				Message = string.Format("'{0}' is a readonly parameter", "readonlyParamName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 5, 4),
					new DiagnosticResultLocation("Test0.cs", 6, 4),
					new DiagnosticResultLocation("Test0.cs", 7, 4),
					new DiagnosticResultLocation("Test0.cs", 8, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestCompoundAssignment() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName(int @readonlyParamName, bool @readonlyBool, int? @readonlyNullable, int mutableParamName, bool mutableBool, int? mutableNullable) {
			readonlyParamName += 1;
			readonlyParamName -= 1;
			readonlyParamName *= 1;
			readonlyParamName /= 1;
			readonlyParamName %= 1;
			readonlyParamName &= 1;
			readonlyParamName |= 1;
			readonlyParamName ^= 1;
			readonlyBool &&= true;
			readonlyBool ||= true;
			readonlyNullable ??= 1;
			mutableParamName += 1;
			mutableParamName -= 1;
			mutableParamName *= 1;
			mutableParamName /= 1;
			mutableParamName %= 1;
			mutableParamName &= 1;
			mutableParamName |= 1;
			mutableParamName ^= 1;
			mutableBool &&= true;
			mutableBool ||= true;
			mutableNullable ??= 1;
		}
	}
}";

			DiagnosticResult expected1 = new() {
				Id = "RG0022",
				Message = string.Format("'{0}' is a readonly parameter", "readonlyParamName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 5, 4),
					new DiagnosticResultLocation("Test0.cs", 6, 4),
					new DiagnosticResultLocation("Test0.cs", 7, 4),
					new DiagnosticResultLocation("Test0.cs", 8, 4),
					new DiagnosticResultLocation("Test0.cs", 9, 4),
					new DiagnosticResultLocation("Test0.cs", 10, 4),
					new DiagnosticResultLocation("Test0.cs", 11, 4),
					new DiagnosticResultLocation("Test0.cs", 12, 4)
				}
			};
			DiagnosticResult expected2 = new() {
				Id = "RG0022",
				Message = string.Format("'{0}' is a readonly parameter", "readonlyBool"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 13, 4),
					new DiagnosticResultLocation("Test0.cs", 14, 4)
				}
			};
			DiagnosticResult expected3 = new() {
				Id = "RG0022",
				Message = string.Format("'{0}' is a readonly parameter", "readonlyNullable"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 15, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2, expected3);
		}

		[TestMethod]
		public void TestRefAssignment() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName(int @readonlyParamName, int mutableParamName, ref int mutableRefParamName, in int inParamName) {
			ref int mutableLocalName = ref readonlyParamName; // not allowed
			ref int @readonlyLocal1 = ref readonlyParamName; // allowed
			ref int @readonlyLocal2 = ref mutableParamName; // not allowed
			ref readonly int @readonlyLocal3 = ref mutableParamName; // allowed
			ref readonly int @readonlyLocal4 = ref mutableRefParamName; // not allowed
			ref readonly int @readonlyLocal5 = ref inParamName; // allowed
		}
	}
}";

			DiagnosticResult expected1 = new() {
				Id = "RG0022",
				Message = string.Format("'{0}' is a readonly parameter", "readonlyParamName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 5, 31)
				}
			};
			DiagnosticResult expected2 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocal2"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 7, 4)
				}
			};
			DiagnosticResult expected3 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocal4"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 9, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2, expected3);
		}

		[TestMethod]
		public void TestRefArgument() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName(int @readonlyParamName, int mutableParamName) {
			SetToOne(ref readonlyParamName);
			SetToOne(ref mutableParamName);
		}
		void SetToOne(ref int value) {
			value = 1;
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0022",
				Message = string.Format("'{0}' is a readonly parameter", "readonlyParamName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 5, 13)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestOutArgument() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName(int @readonlyParamName, int mutableParamName) {
			SetToOne(out readonlyParamName);
			SetToOne(out mutableParamName);
		}
		void SetToOne(out int value) {
			value = 1;
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0022",
				Message = string.Format("'{0}' is a readonly parameter", "readonlyParamName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 5, 13)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestLambdaParameter() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName() {
			new Action<int, int>((@readonlyParamName, mutableParamName) => {
				readonlyParamName = 1;
				mutableParamName = 1;
			})(0, 0);
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0022",
				Message = string.Format("'{0}' is a readonly parameter", "readonlyParamName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 6, 5)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
