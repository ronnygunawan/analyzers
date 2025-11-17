using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class ArgumentMustBeLockedTests : CodeFixVerifier {
		[TestMethod]
		[Ignore("RG0030 not yet implemented - TODO in code")]
		public void TestUnlockedLocal() {
			string test = @"
using RG.Annotations;

namespace Namespace {
	class ClassName {
		void MethodName() {
			object x = new { };
			CalledMethod(x, null);
		}

		void CalledMethod([MustBeLocked] object obj1, object obj2) { }
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0030",
				Message = "Argument must be locked before calling this method",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 17)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestLockedLocal() {
			string test = @"
using RG.Annotations;

namespace Namespace {
	class ClassName {
		void MethodName() {
			object x = new { };
			lock(x) {
				CalledMethod(x, null);
			}
		}

		void CalledMethod([MustBeLocked] object obj1, object obj2) { }
	}
}";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		[Ignore("RG0030 not yet implemented - TODO in code")]
		public void TestUnlockedField() {
			string test = @"
using RG.Annotations;

namespace Namespace {
	class ClassName {
		object _x = new { };

		void MethodName() {
			CalledMethod(_x, null);
		}

		void CalledMethod([MustBeLocked] object obj1, object obj2) { }
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0030",
				Message = "Argument must be locked before calling this method",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 9, 17)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestLockedField() {
			string test = @"
using RG.Annotations;

namespace Namespace {
	class ClassName {
		object _x = new { };

		void MethodName() {
			lock(_x) {
				CalledMethod(_x, null);
			}
		}

		void CalledMethod([MustBeLocked] object obj1, object obj2) { }
	}
}";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		[Ignore("RG0030 not yet implemented - TODO in code")]
		public void TestUnlockedCollectionElement() {
			string test = @"
using RG.Annotations;

namespace Namespace {
	class ClassName {
		void MethodName() {
			object[] x = new[] { new { } };
			CalledMethod(x[0], null);
		}

		void CalledMethod([MustBeLocked] object obj1, object obj2) { }
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0030",
				Message = "Argument must be locked before calling this method",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 17)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestLockedCollectionElement() {
			string test = @"
using RG.Annotations;

namespace Namespace {
	class ClassName {
		void MethodName() {
			object[] x = new[] { new { } };
			lock(x[0]) {
				CalledMethod(x[0], null);
			}
		}

		void CalledMethod([MustBeLocked] object obj1, object obj2) { }
	}
}";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		[Ignore("RG0030 not yet implemented - TODO in code")]
		public void TestUnlockedObjectMember() {
			string test = @"
using RG.Annotations;

namespace Namespace {
	class ClassName {
		void MethodName() {
			object[] x = new[] { new { Y = new { } } };
			CalledMethod(x[0].Y, null);
		}

		void CalledMethod([MustBeLocked] object obj1, object obj2) { }
	}
}";

			DiagnosticResult expected = new() {
				Id = "RG0030",
				Message = "Argument must be locked before calling this method",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 17)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestLockedObjectMember() {
			string test = @"
using RG.Annotations;

namespace Namespace {
	class ClassName {
		void MethodName() {
			object[] x = new[] { new { Y = new { } } };
			locked(x[0].Y) {
				CalledMethod(x[0].Y, null);
			}
		}

		void CalledMethod([MustBeLocked] object obj1, object obj2) { }
	}
}";

			VerifyCSharpDiagnostic(test);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
