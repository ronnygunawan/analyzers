using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class LocalIsReadonlyTests : CodeFixVerifier {
		[TestMethod]
		public void TestAssignment() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName() {
			int @readonlyLocalName = 0;
			readonlyLocalName = 1;
			int mutableLocalName = 0;
			mutableLocalName = 1;
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 6, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestAssignmentToTuple() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName() {
			int @readonlyLocalName = 0;
			(readonlyLocalName, _) = (1, 1);
			int mutableLocalName = 0;
			(mutableLocalName, _) = (1, 1);
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 6, 5)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestIncrementAndDecrementOperator() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName() {
			int @readonlyLocalName = 0;
			readonlyLocalName++;
			readonlyLocalName--;
			++readonlyLocalName;
			--readonlyLocalName;
			int mutableLocalName = 0;
			mutableLocalName++;
			mutableLocalName--;
			++mutableLocalName;
			--mutableLocalName;
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 6, 4),
					new DiagnosticResultLocation("Test0.cs", 7, 4),
					new DiagnosticResultLocation("Test0.cs", 8, 4),
					new DiagnosticResultLocation("Test0.cs", 9, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestCompoundAssignment() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName() {
			int @readonlyLocalName = 0;
			readonlyLocalName += 1;
			readonlyLocalName -= 1;
			readonlyLocalName *= 1;
			readonlyLocalName /= 1;
			readonlyLocalName %= 1;
			readonlyLocalName &= 1;
			readonlyLocalName |= 1;
			readonlyLocalName ^= 1;
			bool @readonlyBool = false;
			readonlyBool &&= true;
			readonlyBool ||= true;
			int? @readonlyNullable = null;
			readonlyNullable ??= 1;
			int mutableLocalName = 0;
			mutableLocalName += 1;
			mutableLocalName -= 1;
			mutableLocalName *= 1;
			mutableLocalName /= 1;
			mutableLocalName %= 1;
			mutableLocalName &= 1;
			mutableLocalName |= 1;
			mutableLocalName ^= 1;
			bool mutableBool = false;
			mutableBool &&= true;
			mutableBool ||= true;
			int? mutableNullable = null;
			mutableNullable ??= 1;
		}
	}
}";

			DiagnosticResult expected1 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 6, 4),
					new DiagnosticResultLocation("Test0.cs", 7, 4),
					new DiagnosticResultLocation("Test0.cs", 8, 4),
					new DiagnosticResultLocation("Test0.cs", 9, 4),
					new DiagnosticResultLocation("Test0.cs", 10, 4),
					new DiagnosticResultLocation("Test0.cs", 11, 4),
					new DiagnosticResultLocation("Test0.cs", 12, 4),
					new DiagnosticResultLocation("Test0.cs", 13, 4)
				}
			};
			DiagnosticResult expected2 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyBool"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 15, 4),
					new DiagnosticResultLocation("Test0.cs", 16, 4)
				}
			};
			DiagnosticResult expected3 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyNullable"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 18, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2, expected3);
		}

		[TestMethod]
		public void TestRefAssignment() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName() {
			int @readonlyLocalName = 0;
			ref int mutableLocalName = ref readonlyLocalName; // not allowed
			ref int @readonlyLocal2 = ref readonlyLocalName; // allowed
			ref int @readonlyLocal3 = ref mutableLocalName; // not allowed
		}
	}
}";

			DiagnosticResult expected1 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 6, 31)
				}
			};
			DiagnosticResult expected2 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocal3"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 8, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2);
		}

		[TestMethod]
		public void TestRefParameter() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName() {
			int @readonlyLocalName = 0;
			SetToOne(ref readonlyLocalName);
			int mutableLocalName = 0;
			SetToOne(ref mutableLocalName);
		}
		void SetToOne(ref int value) {
			value = 1;
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 6, 13)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestOutParameter() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName() {
			int @readonlyLocalName = 0;
			SetToOne(out readonlyLocalName);
			int mutableLocalName = 0;
			SetToOne(out mutableLocalName);
		}
		void SetToOne(out int value) {
			value = 1;
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 6, 13)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestDeclarationInTupleDeconstruction() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName() {
			(int @ReadonlyLocalName, int MutableLocalName) = (0, 0);
			ReadonlyLocalName = 1;
			MutableLocalName = 1;
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 6, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestDeclarationInOutParameter() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName() {
			InitToZero(out int @readonlyLocalName);
			readonlyLocalName = 1;
			InitToZero(out int mutableLocalName);
			mutableLocalName = 1;
		}
		void InitToZero(out int value) {
			value = 0;
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 6, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestDeclarationInForLoop() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName() {
			for (int @i = 0; i < 10; i++) {
			}
		}
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "i"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 5, 19)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
