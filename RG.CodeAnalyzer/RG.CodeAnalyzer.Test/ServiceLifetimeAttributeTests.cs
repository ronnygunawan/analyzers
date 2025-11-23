using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class ServiceLifetimeAttributeTests : CodeFixVerifier {
		[TestMethod]
		public void TestEmptyCode() {
			string test = @"";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestAddSingletonWithoutAttribute() {
			string test = @"
namespace Microsoft.Extensions.DependencyInjection {
	interface IServiceCollection { }
	
	static class ServiceCollectionServiceExtensions {
		public static IServiceCollection AddSingleton<T>(this IServiceCollection services) => services;
	}
}

namespace TestNamespace {
	using Microsoft.Extensions.DependencyInjection;

	class MyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddSingleton<MyService>();
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestAddSingletonWithCorrectAttribute() {
			string test = @"
namespace Microsoft.Extensions.DependencyInjection {
	interface IServiceCollection { }
	
	static class ServiceCollectionServiceExtensions {
		public static IServiceCollection AddSingleton<T>(this IServiceCollection services) => services;
	}
}

namespace TestNamespace {
	using Microsoft.Extensions.DependencyInjection;
	using RG.Annotations;

	[Singleton]
	class MyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddSingleton<MyService>();
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestAddScopedWithoutAttribute() {
			string test = @"
namespace Microsoft.Extensions.DependencyInjection {
	interface IServiceCollection { }
	
	static class ServiceCollectionServiceExtensions {
		public static IServiceCollection AddScoped<T>(this IServiceCollection services) => services;
	}
}

namespace TestNamespace {
	using Microsoft.Extensions.DependencyInjection;

	class MyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddScoped<MyService>();
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestAddScopedWithCorrectAttribute() {
			string test = @"
namespace Microsoft.Extensions.DependencyInjection {
	interface IServiceCollection { }
	
	static class ServiceCollectionServiceExtensions {
		public static IServiceCollection AddScoped<T>(this IServiceCollection services) => services;
	}
}

namespace TestNamespace {
	using Microsoft.Extensions.DependencyInjection;
	using RG.Annotations;

	[Scoped]
	class MyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddScoped<MyService>();
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestAddTransientWithoutAttribute() {
			string test = @"
namespace Microsoft.Extensions.DependencyInjection {
	interface IServiceCollection { }
	
	static class ServiceCollectionServiceExtensions {
		public static IServiceCollection AddTransient<T>(this IServiceCollection services) => services;
	}
}

namespace TestNamespace {
	using Microsoft.Extensions.DependencyInjection;

	class MyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddTransient<MyService>();
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestAddTransientWithCorrectAttribute() {
			string test = @"
namespace Microsoft.Extensions.DependencyInjection {
	interface IServiceCollection { }
	
	static class ServiceCollectionServiceExtensions {
		public static IServiceCollection AddTransient<T>(this IServiceCollection services) => services;
	}
}

namespace TestNamespace {
	using Microsoft.Extensions.DependencyInjection;
	using RG.Annotations;

	[Transient]
	class MyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddTransient<MyService>();
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestAddSingletonWithWrongAttribute() {
			string test = @"
namespace Microsoft.Extensions.DependencyInjection {
	interface IServiceCollection { }
	
	static class ServiceCollectionServiceExtensions {
		public static IServiceCollection AddSingleton<T>(this IServiceCollection services) => services;
	}
}

namespace TestNamespace {
	using Microsoft.Extensions.DependencyInjection;
	using RG.Annotations;

	[Scoped]
	class MyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddSingleton<MyService>();
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0034",
				Message = "Service 'MyService' registered with AddSingleton must be marked with [Singleton] attribute",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 20, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
