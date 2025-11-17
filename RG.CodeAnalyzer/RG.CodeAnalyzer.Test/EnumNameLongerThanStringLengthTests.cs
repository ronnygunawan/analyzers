using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class EnumNameLongerThanStringLengthTests : CodeFixVerifier {

		[TestMethod]
		public void TestEmptyCode() {
			string test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestEnumPropertyWithoutStringLength() {
			string test = @"
using System;
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1
{
	public enum FruitStatus {
		Unripe,
		Ripe,
		Rotten
	}

	public class Fruit {
		public FruitStatus Status { get; set; }
	}
}";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestEnumPropertyWithStringLengthThatFits() {
			string test = @"
using System;
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1
{
	public enum FruitStatus {
		Unripe,
		Ripe,
		Rotten
	}

	public class Fruit {
		[StringLength(10)]
		public FruitStatus Status { get; set; }
	}
}";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestEnumPropertyWithStringLengthTooShort() {
			string test = @"
using System;
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1
{
	public enum FruitStatus {
		Unripe,
		Ripe,
		Rotten
	}

	public class Fruit {
		[StringLength(5)]
		public FruitStatus Status { get; set; }
	}
}";
			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = string.Format("Longest enum name '{0}' ({1} characters) exceeds StringLength maximum of {2}", "Rotten", 6, 5),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 16, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestEnumPropertyWithFullyQualifiedStringLength() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	public enum FruitStatus {
		Unripe,
		Ripe,
		Rotten
	}

	public class Fruit {
		[System.ComponentModel.DataAnnotations.StringLength(5)]
		public FruitStatus Status { get; set; }
	}
}";
			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = string.Format("Longest enum name '{0}' ({1} characters) exceeds StringLength maximum of {2}", "Rotten", 6, 5),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 15, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestEnumPropertyWithLongerNames() {
			string test = @"
using System;
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1
{
	public enum OrderStatus {
		Pending,
		Processing,
		Shipped,
		Delivered,
		Cancelled
	}

	public class Order {
		[StringLength(8)]
		public OrderStatus Status { get; set; }
	}
}";
			DiagnosticResult expected = new() {
				Id = "RG0032",
				Message = string.Format("Longest enum name '{0}' ({1} characters) exceeds StringLength maximum of {2}", "Processing", 10, 8),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 18, 3)
					}
			};

			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestNonEnumPropertyWithStringLength() {
			string test = @"
using System;
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1
{
	public class Fruit {
		[StringLength(5)]
		public string Name { get; set; }
	}
}";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestEnumPropertyWithExactLength() {
			string test = @"
using System;
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1
{
	public enum FruitStatus {
		Unripe,
		Ripe,
		Rotten
	}

	public class Fruit {
		[StringLength(6)]
		public FruitStatus Status { get; set; }
	}
}";

			VerifyCSharpDiagnostic(test);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
