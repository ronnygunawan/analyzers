using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoDummies;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class ProtobufTests : CodeFixVerifier {
		[TestMethod]
		public void TestUninitializedProperties() {
			string test = @"
using ProtoDummies;

namespace Namespace {
	class ClassName {
		void MethodName() {
			SimpleMessage simpleMessage = new SimpleMessage {
				IntProperty = 1
			};
		}
	}
}
";

			DiagnosticResult expected1 = new() {
				Id = "RG0028",
				Message = string.Format("'{0}' is a required protobuf property and should be initialized", "StringProperty"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 7, 52)
					}
			};

			DiagnosticResult expected2 = new() {
				Id = "RG0028",
				Message = string.Format("'{0}' is a required protobuf property and should be initialized", "StringProperty"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 7, 52)
					}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2);
		}

		[TestMethod]
		public void TestParameterlessConstructorWithoutInitializer() {
			string test = @"
using ProtoDummies;

namespace Namespace {
	class ClassName {
		void MethodName() {
			SimpleMessage simpleMessage = new SimpleMessage();
		}
	}
}
";

			DiagnosticResult expected1 = new() {
				Id = "RG0028",
				Message = string.Format("'{0}' is a required protobuf property and should be initialized", "IntProperty"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 7, 34)
					}
			};

			DiagnosticResult expected2 = new() {
				Id = "RG0028",
				Message = string.Format("'{0}' is a required protobuf property and should be initialized", "StringProperty"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 7, 34)
					}
			};

			DiagnosticResult expected3 = new() {
				Id = "RG0028",
				Message = string.Format("'{0}' is a required protobuf property and should be initialized", "StringProperty"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 7, 34)
					}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2, expected3);
		}

		[TestMethod]
		public void TestCopyConstructor() {
			string test = @"
using ProtoDummies;

namespace Namespace {
	class ClassName {
		void MethodName(SimpleMessage other) {
			SimpleMessage simpleMessage = new SimpleMessage(other);
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestUninitializedRepeatedProperties() {
			string test = @"
using ProtoDummies;

namespace Namespace {
	class ClassName {
		void MethodName() {
			MessageWithRepeatedProperty message = new MessageWithRepeatedProperty {
				IntProperty = 1,
				StringProperty = null
			};
		}
	}
}
";

			DiagnosticResult expected1 = new() {
				Id = "RG0028",
				Message = string.Format("'{0}' is a required protobuf property and should be initialized", "RepeatedIntProperty"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 7, 74)
					}
			};

			DiagnosticResult expected2 = new() {
				Id = "RG0028",
				Message = string.Format("'{0}' is a required protobuf property and should be initialized", "RepeatedStringProperty"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 7, 74)
					}
			};

			VerifyCSharpDiagnostic(test, expected1, expected2);
		}

		[TestMethod]
		public void TestInitializedRepeatedProperties() {
			string test = @"
using ProtoDummies;

namespace Namespace {
	class ClassName {
		void MethodName() {
			MessageWithRepeatedProperty message = new MessageWithRepeatedProperty {
				IntProperty = 1,
				RepeatedIntProperty = { },
				StringProperty = null,
				RepeatedStringProperty = { ""Hello"", ""World"" }
			};
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestUninitializedOneOf() {
			string test = @"
using ProtoDummies;

namespace Namespace {
	class ClassName {
		void MethodName() {
			OneOfMessage oneOfMessage = new OneOfMessage();
		}
	}
}
";

			DiagnosticResult expected1 = new() {
				Id = "RG0028",
				Message = string.Format("'{0}' is a required protobuf property and should be initialized", "RepeatedIntProperty"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 7, 74)
					}
			};

			DiagnosticResult expected2 = new() {
				Id = "RG0028",
				Message = string.Format("'{0}' is a required protobuf property and should be initialized", "RepeatedStringProperty"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 7, 74)
					}
			};

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestProperlyInitializedOneOf() {
			string test = @"
using ProtoDummies;

namespace Namespace {
	class ClassName {
		void MethodName() {
			OneOfMessage oneOfMessage = new OneOfMessage {
				Simple = new SimpleMessage {
					IntProperty = 1,
					StringProperty = null,
					BoolProperty = false
				}
			};
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestImproperlyInitializedOneOf() {
			string test = @"
using ProtoDummies;

namespace Namespace {
	class ClassName {
		void MethodName() {
			OneOfMessage oneOfMessage = new OneOfMessage {
				Simple = new SimpleMessage {
					IntProperty = 1,
					StringProperty = null,
					BoolProperty = false
				},
				Composite = new CompositeMessage {
					Message1 = new SimpleMessage {
						IntProperty = 2,
						StringProperty = ""2"",
						BoolProperty = true
					},
					IntProperty = 2
				}
			};
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0029",
				Message = string.Format("'{0}' cannot be initialized because '{1}' is also being initialized", "Composite", "Simple"),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 13, 5)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
