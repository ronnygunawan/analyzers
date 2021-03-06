﻿using Microsoft.CodeAnalysis;
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
			(int @readonlyLocal2, int @readonlyLocal3) = (0, 0);
			readonlyLocal2 = 1;
			readonlyLocal3 = 1;
		}
	}
}";

			DiagnosticResult expected1 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 6, 5)
				}
			};
			DiagnosticResult expected2 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocal2"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 10, 4)
				}
			};
			DiagnosticResult expected3 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocal3"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 11, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2, expected3);
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

			DiagnosticResult expected1 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 6, 4)
				}
			};
			DiagnosticResult expected2 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 7, 4)
				}
			};
			DiagnosticResult expected3 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 8, 4)
				}
			};
			DiagnosticResult expected4 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 9, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2, expected3, expected4);
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
					new DiagnosticResultLocation("Test0.cs", 6, 4)
				}
			};
			DiagnosticResult expected2 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 7, 4)
				}
			};
			DiagnosticResult expected3 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 8, 4)
				}
			};
			DiagnosticResult expected4 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 9, 4)
				}
			};
			DiagnosticResult expected5 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 10, 4)
				}
			};
			DiagnosticResult expected6 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 11, 4)
				}
			};
			DiagnosticResult expected7 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 12, 4)
				}
			};
			DiagnosticResult expected8 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 13, 4)
				}
			};
			DiagnosticResult expected9 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyNullable"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 15, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2, expected3, expected4, expected5, expected6, expected7, expected8, expected9);
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
			ref int @readonlyLocal4 = ref MutableFieldName; // not allowed
			ref readonly int @readonlyLocal5 = ref mutableLocalName; // not allowed
			ref readonly int @readonlyLocal6 = ref MethodName(in readonlyLocalName); // allowed
			ref readonly int @readonlyLocal7 = ref MethodName(in mutableLocalName); // not allowed
			ref mutableLocalName = ref readonlyLocalName; // not allowed
			ref readonlyLocal2 = ref readonlyLocalName; // allowed
			ref readonlyLocal3 = ref mutableLocalName; // allowed
			ref readonlyLocal4 = ref MutableFieldName; // not allowed
		}
		int MutableFieldName = 0;
		ref readonly int MethodName(in int value) {
			return ref value;
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
			DiagnosticResult expected3 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocal4"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 9, 4)
				}
			};
			DiagnosticResult expected4 = new() {
				Id = "RG0021",
				Message = string.Format("'{0}' is a readonly local variable", "readonlyLocal5"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 10, 4)
				}
			};
			DiagnosticResult expected5 = new() {
				Id = "RG0024",
				Message = string.Format("'in' argument '{0}' should be readonly", "mutableLocalName"),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 12, 54)
				}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2, expected3, expected4, expected5);
		}

		[TestMethod]
		public void TestRefArgument() {
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
		public void TestOutArgument() {
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
		public void TestReturnRef() {
			string test = @"
namespace Namespace {
	class ClassName {
		void MethodName() {
			int value = 0;
			
		}
		ref int Method2(ref int value) {
			ref int @readonlyLocalName = ref value;
			return ref readonlyLocalName;
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
				Message = string.Format("'{0}' is a readonly local variable", "ReadonlyLocalName"),
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
					new DiagnosticResultLocation("Test0.cs", 5, 29)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
