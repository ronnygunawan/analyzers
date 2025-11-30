using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class CA1812SuppressorTests {
		[TestMethod]
		public void Suppressor_SupportsSingleSuppression() {
			CA1812Suppressor suppressor = new();
			
			Assert.AreEqual(1, suppressor.SupportedSuppressions.Length);
		}

		[TestMethod]
		public void Suppressor_SuppressionId_IsRGS001() {
			CA1812Suppressor suppressor = new();
			
			Assert.AreEqual("RGS001", suppressor.SupportedSuppressions[0].Id);
		}

		[TestMethod]
		public void Suppressor_SuppressesCA1812() {
			CA1812Suppressor suppressor = new();
			
			Assert.AreEqual("CA1812", suppressor.SupportedSuppressions[0].SuppressedDiagnosticId);
		}

		[TestMethod]
		public void Suppressor_JustificationMentionsTypeArgument() {
			CA1812Suppressor suppressor = new();
			
			var justification = suppressor.SupportedSuppressions[0].Justification.ToString();
			
			Assert.IsTrue(justification.Contains("type argument"), 
				$"Justification should mention 'type argument'. Actual: {justification}");
		}

		[TestMethod]
		public async Task InternalClassUsedAsGenericTypeArgument_CA1812IsSuppressed() {
			string source = @"
using System.Collections.Generic;

namespace TestNamespace {
    internal class Model {
        public int Id { get; set; }
    }

    public class Consumer {
        public void UseModel() {
            var list = new List<Model>();
        }
    }
}
";
			var diagnostics = await GetCA1812DiagnosticsWithSuppressor(source);
			
			// CA1812 should be suppressed because Model is used as a type argument in List<Model>
			Assert.IsFalse(diagnostics.Any(d => d.Id == "CA1812" && !d.IsSuppressed),
				"CA1812 should be suppressed when internal class is used as type argument");
		}

		[TestMethod]
		public async Task InternalClassUsedInGenericMethodCall_CA1812IsSuppressed() {
			string source = @"
namespace TestNamespace {
    internal class Model {
        public int Id { get; set; }
    }

    public class Consumer {
        public void UseModel() {
            var model = Deserialize<Model>();
        }

        private T Deserialize<T>() where T : new() {
            return new T();
        }
    }
}
";
			var diagnostics = await GetCA1812DiagnosticsWithSuppressor(source);
			
			// CA1812 should be suppressed because Model is used as a type argument in Deserialize<Model>()
			Assert.IsFalse(diagnostics.Any(d => d.Id == "CA1812" && !d.IsSuppressed),
				"CA1812 should be suppressed when internal class is used as type argument in method call");
		}

		[TestMethod]
		public async Task InternalClassUsedInTypeOf_CA1812IsSuppressed() {
			string source = @"
using System;

namespace TestNamespace {
    internal class Model {
        public int Id { get; set; }
    }

    public class Consumer {
        public void UseModel() {
            Type t = typeof(Model);
        }
    }
}
";
			var diagnostics = await GetCA1812DiagnosticsWithSuppressor(source);
			
			// CA1812 should be suppressed because Model is used in typeof(Model)
			Assert.IsFalse(diagnostics.Any(d => d.Id == "CA1812" && !d.IsSuppressed),
				"CA1812 should be suppressed when internal class is used in typeof expression");
		}

		[TestMethod]
		public async Task InternalClassNotUsedAsTypeArgument_CA1812IsNotSuppressed() {
			string source = @"
namespace TestNamespace {
    internal class UnusedModel {
        public int Id { get; set; }
    }

    public class Consumer {
        public void DoSomething() {
            // UnusedModel is never used
        }
    }
}
";
			var diagnostics = await GetCA1812DiagnosticsWithSuppressor(source);
			
			// CA1812 should NOT be suppressed because UnusedModel is never used as a type argument
			var ca1812Diagnostics = diagnostics.Where(d => d.Id == "CA1812").ToList();
			Assert.IsTrue(ca1812Diagnostics.Count > 0, "CA1812 should be reported for unused internal class");
			Assert.IsTrue(ca1812Diagnostics.All(d => !d.IsSuppressed),
				"CA1812 should NOT be suppressed when internal class is not used as type argument");
		}

		private static async Task<ImmutableArray<Diagnostic>> GetCA1812DiagnosticsWithSuppressor(string source) {
			var syntaxTree = CSharpSyntaxTree.ParseText(source);
			
			var references = new List<MetadataReference> {
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
			};

			// Add runtime references
			var runtimeDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;
			var runtimeAssembly = System.IO.Path.Combine(runtimeDir, "System.Runtime.dll");
			if (System.IO.File.Exists(runtimeAssembly)) {
				references.Add(MetadataReference.CreateFromFile(runtimeAssembly));
			}

			var compilation = CSharpCompilation.Create(
				"TestAssembly",
				new[] { syntaxTree },
				references,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

			// Create a mock analyzer that generates CA1812 diagnostics for internal classes
			var ca1812Analyzer = new MockCA1812Analyzer();
			var suppressor = new CA1812Suppressor();

			var compilationWithAnalyzers = compilation.WithAnalyzers(
				ImmutableArray.Create<DiagnosticAnalyzer>(ca1812Analyzer, suppressor));

			return await compilationWithAnalyzers.GetAllDiagnosticsAsync();
		}
	}

	/// <summary>
	/// Mock analyzer that produces CA1812-like diagnostics for internal classes that are not instantiated.
	/// This simulates what the real Microsoft.CodeAnalysis.NetAnalyzers CA1812 does.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	internal class MockCA1812Analyzer : DiagnosticAnalyzer {
		private static readonly DiagnosticDescriptor CA1812 = new(
			id: "CA1812",
			title: "Avoid uninstantiated internal classes",
			messageFormat: "'{0}' is an internal class that is apparently never instantiated",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(CA1812);

		public override void Initialize(AnalysisContext context) {
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context) {
			var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

			// Only analyze internal classes
			if (namedTypeSymbol.DeclaredAccessibility != Accessibility.Internal ||
				namedTypeSymbol.TypeKind != TypeKind.Class) {
				return;
			}

			// Check if the class has a constructor that is directly invoked
			// For simplicity, we report CA1812 for all internal classes without checking instantiation
			// The real CA1812 does more sophisticated analysis
			var diagnostic = Diagnostic.Create(CA1812, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
			context.ReportDiagnostic(diagnostic);
		}
	}
}

