using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class NeverAsyncAttributeTests : CodeFixVerifier {
		[TestMethod]
		public void TestNeverAsyncSuppressesRG0006() {
			string test = @"
using System;
using System.Threading.Tasks;
using RG.Annotations;

namespace ConsoleApplication1
{
	public class TypeName
	{
		[NeverAsync]
		private Task Foo() => Task.FromResult(10);

		public void Bar() {
			Foo().Wait();
		}
	}
}";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestNeverAsyncSuppressesRG0007() {
			string test = @"
using System;
using System.Threading.Tasks;
using RG.Annotations;

namespace ConsoleApplication1
{
	public class TypeName
	{
		[NeverAsync]
		private Task<int> Foo() => Task.FromResult(10);

		public void Bar() {
			int x = Foo().Result;
		}
	}
}";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestWithoutNeverAsyncRG0006Fires() {
			string test = @"
using System;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
	public class TypeName
	{
		private Task Foo() => Task.FromResult(10);

		public void Bar() {
			Foo().Wait();
		}
	}
}";
			DiagnosticResult expected = new() {
				Id = "RG0006",
				Message = "Calling Task.Wait() blocks current thread and is not recommended; Use await instead",
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 12, 4)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestWithoutNeverAsyncRG0007Fires() {
			string test = @"
using System;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
	public class TypeName
	{
		private Task<int> Foo() => Task.FromResult(10);

		public void Bar() {
			int x = Foo().Result;
		}
	}
}";
			DiagnosticResult expected = new() {
				Id = "RG0007",
				Message = "Accessing Task<>.Result blocks current thread and is not recommended; Use await instead",
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 12, 12)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestNeverAsyncOnAsyncMethodFails() {
			string test = @"
using System;
using System.Threading.Tasks;
using RG.Annotations;

namespace ConsoleApplication1
{
	public class TypeName
	{
		[NeverAsync]
		async Task Foo() => await Task.FromResult(10);
	}
}";
			DiagnosticResult expected = new() {
				Id = "RG0041",
				Message = "Method decorated with [NeverAsync] cannot use 'async' modifier",
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 10, 3)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestNeverAsyncCallingNonNeverAsyncFails() {
			string test = @"
using System;
using System.Threading.Tasks;
using RG.Annotations;

namespace ConsoleApplication1
{
	public class TypeName
	{
		Task Baz() => Task.FromResult(10);

		[NeverAsync]
		Task Bar() => Baz();
	}
}";
			DiagnosticResult expected = new() {
				Id = "RG0041",
				Message = "Method decorated with [NeverAsync] calls another Task-returning method that is not decorated with [NeverAsync]",
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] {
					new DiagnosticResultLocation("Test0.cs", 12, 3)
				}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestNeverAsyncCallingNeverAsyncSucceeds() {
			string test = @"
using System;
using System.Threading.Tasks;
using RG.Annotations;

namespace ConsoleApplication1
{
	public class TypeName
	{
		[NeverAsync]
		Task Baz() => Task.CompletedTask;

		[NeverAsync]
		Task Bar() => Baz();

		[NeverAsync]
		Task Asd() => Bar();
	}
}";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestNeverAsyncOnMethodWithNoTaskReturn() {
			string test = @"
using System;
using RG.Annotations;

namespace ConsoleApplication1
{
	public class TypeName
	{
		[NeverAsync]
		void Foo() { }
	}
}";
			// This should not cause an error - the attribute is just ignored on non-Task methods
			VerifyCSharpDiagnostic(test);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
