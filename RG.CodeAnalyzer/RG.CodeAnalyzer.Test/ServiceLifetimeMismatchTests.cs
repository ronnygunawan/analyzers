using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class ServiceLifetimeMismatchTests : CodeFixVerifier {
		[TestMethod]
		public void TestEmptyCode() {
			string test = @"";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestSingletonDependingOnScoped() {
			string test = @"
using RG.Annotations;

namespace TestNamespace {
	[Scoped]
	class ScopedService {
	}

	[Singleton]
	class SingletonService {
		public SingletonService(ScopedService scopedService) {
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0035",
				Message = "Singleton service 'SingletonService' cannot depend on Scoped service 'ScopedService'",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 11, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestSingletonDependingOnTransient() {
			string test = @"
using RG.Annotations;

namespace TestNamespace {
	[Transient]
	class TransientService {
	}

	[Singleton]
	class SingletonService {
		public SingletonService(TransientService transientService) {
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0035",
				Message = "Singleton service 'SingletonService' cannot depend on Transient service 'TransientService'",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 11, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestScopedDependingOnTransient() {
			string test = @"
using RG.Annotations;

namespace TestNamespace {
	[Transient]
	class TransientService {
	}

	[Scoped]
	class ScopedService {
		public ScopedService(TransientService transientService) {
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0035",
				Message = "Scoped service 'ScopedService' cannot depend on Transient service 'TransientService'",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 11, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestScopedDependingOnSingleton() {
			string test = @"
using RG.Annotations;

namespace TestNamespace {
	[Singleton]
	class SingletonService {
	}

	[Scoped]
	class ScopedService {
		public ScopedService(SingletonService singletonService) {
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestTransientDependingOnSingleton() {
			string test = @"
using RG.Annotations;

namespace TestNamespace {
	[Singleton]
	class SingletonService {
	}

	[Transient]
	class TransientService {
		public TransientService(SingletonService singletonService) {
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestTransientDependingOnScoped() {
			string test = @"
using RG.Annotations;

namespace TestNamespace {
	[Scoped]
	class ScopedService {
	}

	[Transient]
	class TransientService {
		public TransientService(ScopedService scopedService) {
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestServiceWithoutLifetimeAttribute() {
			string test = @"
using RG.Annotations;

namespace TestNamespace {
	class ServiceWithoutAttribute {
	}

	[Singleton]
	class SingletonService {
		public SingletonService(ServiceWithoutAttribute service) {
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestPropertyDependency() {
			string test = @"
using RG.Annotations;

namespace TestNamespace {
	[Transient]
	class TransientService {
	}

	[Singleton]
	class SingletonService {
		public TransientService Service { get; set; }
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0035",
				Message = "Singleton service 'SingletonService' cannot depend on Transient service 'TransientService'",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 11, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestFieldDependency() {
			string test = @"
using RG.Annotations;

namespace TestNamespace {
	[Scoped]
	class ScopedService {
	}

	[Singleton]
	class SingletonService {
		private ScopedService _service;
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0035",
				Message = "Singleton service 'SingletonService' cannot depend on Scoped service 'ScopedService'",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 11, 25)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
