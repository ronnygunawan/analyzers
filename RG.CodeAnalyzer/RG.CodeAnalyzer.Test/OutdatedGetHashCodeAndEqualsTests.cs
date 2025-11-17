using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class OutdatedGetHashCodeAndEqualsTests : CodeFixVerifier {

		[TestMethod]
		public void TestGetHashCodeWithXorPattern() {
			string test = @"
namespace ConsoleApplication1
{
	public class Person
	{
		public string Name { get; set; }
		public int Age { get; set; }

		public override int GetHashCode()
		{
			return Name.GetHashCode() ^ Age.GetHashCode();
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = "GetHashCode implementation uses outdated pattern; Consider using HashCode.Combine instead",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 9, 23)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestGetHashCodeWithMultiplicationPattern() {
			string test = @"
namespace ConsoleApplication1
{
	public class Person
	{
		public string Name { get; set; }
		public int Age { get; set; }

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + Name.GetHashCode();
				hash = hash * 23 + Age.GetHashCode();
				return hash;
			}
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = "GetHashCode implementation uses outdated pattern; Consider using HashCode.Combine instead",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 9, 23)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestGetHashCodeWithTupleCreatePattern() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	public class Person
	{
		public string Name { get; set; }
		public int Age { get; set; }

		public override int GetHashCode()
		{
			return Tuple.Create(Name, Age).GetHashCode();
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = "GetHashCode implementation uses outdated pattern; Consider using HashCode.Combine instead",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 11, 23)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestModernGetHashCode_NoDiagnostic() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	public class Person
	{
		public string Name { get; set; }
		public int Age { get; set; }

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, Age);
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestEqualsWithDirectCastWithoutNullCheck() {
			string test = @"
namespace ConsoleApplication1
{
	public class Person
	{
		public string Name { get; set; }

		public override bool Equals(object obj)
		{
			var other = (Person)obj;
			return Name == other.Name;
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0033",
				Message = "Equals implementation uses outdated pattern; Consider using 'is' pattern matching with null/type checks",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 24)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestEqualsWithAsCastWithoutNullCheck() {
			string test = @"
namespace ConsoleApplication1
{
	public class Person
	{
		public string Name { get; set; }

		public override bool Equals(object obj)
		{
			var other = obj as Person;
			return Name == other.Name;
		}
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0033",
				Message = "Equals implementation uses outdated pattern; 'as' cast without proper null check; Consider using 'is' pattern matching",
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 8, 24)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestModernEquals_NoDiagnostic() {
			string test = @"
namespace ConsoleApplication1
{
	public class Person
	{
		public string Name { get; set; }

		public override bool Equals(object obj)
		{
			return obj is Person other && Name == other.Name;
		}
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestEqualsWithNullCheck_NoDiagnostic() {
			string test = @"
namespace ConsoleApplication1
{
	public class Person
	{
		public string Name { get; set; }

		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			var other = (Person)obj;
			return Name == other.Name;
		}
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
