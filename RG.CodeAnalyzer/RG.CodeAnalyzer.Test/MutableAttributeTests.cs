using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class MutableAttributeTests : CodeFixVerifier {
		[TestMethod]
		public void MutableFieldsAllowedInMutableRecord() {
			string test = @"
using RG.Annotations;

namespace NamespaceName {
	[Mutable]
	record RecordName {
		int X;
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void MutablePropertiesAllowedInMutableRecord() {
			string test = @"
using RG.Annotations;

namespace NamespaceName {
	[Mutable]
	record RecordName {
		int X { get; set; }
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void MutableTypesAllowedInMutableRecord() {
			string test = @"
using RG.Annotations;

namespace NamespaceName {
	[Mutable]
	record RecordName {
		readonly object X;
		object Y { get; init; }
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void MutableCollectionsAllowedInMutableRecord() {
			string test = @"
using RG.Annotations;

namespace NamespaceName {
	[Mutable]
	record RecordName {
		List<int> X { get; init; }
		int[] Y { get; init; }
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void MutableTypesNotAllowedInImmutableRecords() {
			string test = @"
using RG.Annotations;

namespace NamespaceName {
	[Mutable]
	record RecordName {
	}

	record ImmutableRecordName {
		readonly RecordName X;
		RecordName Y { get; init; }
		ImmutableList<RecordName> Z { get; init; }
	}
}
";

			DiagnosticResult expected1 = new() {
				Id = "RG0018",
				Message = string.Format("'{0}' is {1} type and should not be used in a record", "RecordName", "mutable"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 10, 12)
					}
			};

			DiagnosticResult expected2 = new() {
				Id = "RG0018",
				Message = string.Format("'{0}' is {1} type and should not be used in a record", "RecordName", "mutable"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 11, 3)
					}
			};

			DiagnosticResult expected3 = new() {
				Id = "RG0018",
				Message = string.Format("'{0}' is {1} type and should not be used in a record", "RecordName", "mutable"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 12, 17)
					}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2, expected3);
		}

		[TestMethod]
		public void MutableRecordsAllowedInMutableRecords() {
			string test = @"
using RG.Annotations;

namespace NamespaceName {
	[Mutable]
	record RecordName {
	}

	[Mutable]
	record Record2 {
		RecordName X;
		List<RecordName> Y;
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
