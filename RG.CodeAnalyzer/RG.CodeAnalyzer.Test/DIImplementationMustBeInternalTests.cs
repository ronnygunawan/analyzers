using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class DIImplementationMustBeInternalTests : CodeFixVerifier {
		[TestMethod]
		public void TestEmptyCode() {
			string test = @"";
			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestPublicClassWithAddSingleton() {
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
	public class MyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddSingleton<MyService>();
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0036",
				Message = "Class 'MyService' is registered with DI and should be internal",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 20, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestInternalClassWithAddSingleton() {
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
	internal class MyService {
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
		public void TestPublicClassWithAddScoped() {
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
	public class MyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddScoped<MyService>();
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0036",
				Message = "Class 'MyService' is registered with DI and should be internal",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 20, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestPublicClassWithAddTransient() {
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
	public class MyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddTransient<MyService>();
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0036",
				Message = "Class 'MyService' is registered with DI and should be internal",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 20, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestPublicInterfaceWithInternalImplementation() {
			string test = @"
namespace Microsoft.Extensions.DependencyInjection {
	interface IServiceCollection { }
	
	static class ServiceCollectionServiceExtensions {
		public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services) 
			where TImplementation : TService => services;
	}
}

namespace TestNamespace {
	using Microsoft.Extensions.DependencyInjection;
	using RG.Annotations;

	[Transient]
	public interface IMyService {
	}

	[Transient]
	internal class MyService : IMyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddTransient<IMyService, MyService>();
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestPublicInterfaceWithPublicImplementation() {
			string test = @"
namespace Microsoft.Extensions.DependencyInjection {
	interface IServiceCollection { }
	
	static class ServiceCollectionServiceExtensions {
		public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services) 
			where TImplementation : TService => services;
	}
}

namespace TestNamespace {
	using Microsoft.Extensions.DependencyInjection;
	using RG.Annotations;

	[Transient]
	public interface IMyService {
	}

	[Transient]
	public class MyService : IMyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddTransient<IMyService, MyService>();
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0036",
				Message = "Class 'MyService' is registered with DI and should be internal",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 25, 4)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestCodeFix() {
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
	public class MyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddSingleton<MyService>();
		}
	}
}
";

			string fixedCode = @"
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
	internal class MyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddSingleton<MyService>();
		}
	}
}
";

			VerifyCSharpFix(test, fixedCode);
		}

		[TestMethod]
		public void TestCodeFixWithInterfaceImplementation() {
			string test = @"
namespace Microsoft.Extensions.DependencyInjection {
	interface IServiceCollection { }
	
	static class ServiceCollectionServiceExtensions {
		public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services) 
			where TImplementation : TService => services;
	}
}

namespace TestNamespace {
	using Microsoft.Extensions.DependencyInjection;
	using RG.Annotations;

	[Transient]
	public interface IMyService {
	}

	[Transient]
	public class MyService : IMyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddTransient<IMyService, MyService>();
		}
	}
}
";

			string fixedCode = @"
namespace Microsoft.Extensions.DependencyInjection {
	interface IServiceCollection { }
	
	static class ServiceCollectionServiceExtensions {
		public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services) 
			where TImplementation : TService => services;
	}
}

namespace TestNamespace {
	using Microsoft.Extensions.DependencyInjection;
	using RG.Annotations;

	[Transient]
	public interface IMyService {
	}

	[Transient]
	internal class MyService : IMyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddTransient<IMyService, MyService>();
		}
	}
}
";

			VerifyCSharpFix(test, fixedCode);
		}

		[TestMethod]
		public void TestPublicInterfaceIsNotFlagged() {
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
	public interface IMyService {
	}

	class Startup {
		void ConfigureServices(IServiceCollection services) {
			services.AddTransient<IMyService>();
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider() {
			return new MakeDIImplementationInternalCodeFixProvider();
		}
	}
}
