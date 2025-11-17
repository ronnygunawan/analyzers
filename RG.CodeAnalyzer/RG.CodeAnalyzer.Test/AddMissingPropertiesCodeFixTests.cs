using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class AddMissingPropertiesCodeFixTests : CodeFixVerifier {
		[TestMethod]
		public void TestAddMissingPropertiesToEmptyInitializer() {
			string test = @"
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1 {
	public record Person {
		public int Id { get; init; }
		public string? FirstName { get; init; }
		
		[Required]
		public string? LastName { get; init; }
	}

	public static class Program {
		public static void Main() {
			var person = new Person {
			};
		}
	}
}";

			string fixedAllProperties = @"
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1 {
	public record Person {
		public int Id { get; init; }
		public string? FirstName { get; init; }
		
		[Required]
		public string? LastName { get; init; }
	}

	public static class Program {
		public static void Main() {
			var person = new Person {

                Id = _,
                FirstName = _,
                LastName = _
            };
		}
	}
}";

			VerifyCSharpFix(test, fixedAllProperties, codeFixIndex: 0);
		}

		[TestMethod]
		public void TestAddMissingRequiredPropertiesOnly() {
			string test = @"
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1 {
	public record Person {
		public int Id { get; init; }
		public string? FirstName { get; init; }
		
		[Required]
		public string? LastName { get; init; }
	}

	public static class Program {
		public static void Main() {
			var person = new Person {
			};
		}
	}
}";

			string fixedRequiredOnly = @"
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1 {
	public record Person {
		public int Id { get; init; }
		public string? FirstName { get; init; }
		
		[Required]
		public string? LastName { get; init; }
	}

	public static class Program {
		public static void Main() {
			var person = new Person {

                LastName = _
            };
		}
	}
}";

			VerifyCSharpFix(test, fixedRequiredOnly, codeFixIndex: 1);
		}

		[TestMethod]
		public void TestAddMissingPropertiesWithAtPrefix() {
			string test = @"
namespace ConsoleApplication1 {
	public record Person {
		public int Id { get; init; }
		public string? FirstName { get; init; }
		public string? @LastName { get; init; }
	}

	public static class Program {
		public static void Main() {
			var person = new Person {
			};
		}
	}
}";

			string fixedAllProperties = @"
namespace ConsoleApplication1 {
	public record Person {
		public int Id { get; init; }
		public string? FirstName { get; init; }
		public string? @LastName { get; init; }
	}

	public static class Program {
		public static void Main() {
			var person = new Person {

                Id = _,
                FirstName = _,
                LastName = _
            };
		}
	}
}";

			VerifyCSharpFix(test, fixedAllProperties, codeFixIndex: 0);
		}

		[TestMethod]
		public void TestAddMissingPropertiesWithSomeExisting() {
			string test = @"
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1 {
	public record Person {
		public int Id { get; init; }
		public string? FirstName { get; init; }
		
		[Required]
		public string? LastName { get; init; }
	}

	public static class Program {
		public static void Main() {
			var person = new Person {
				FirstName = ""John""
			};
		}
	}
}";

			string fixedAllProperties = @"
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1 {
	public record Person {
		public int Id { get; init; }
		public string? FirstName { get; init; }
		
		[Required]
		public string? LastName { get; init; }
	}

	public static class Program {
		public static void Main() {
			var person = new Person {
				FirstName = ""John""
,
                Id = _,
                LastName = _
            };
		}
	}
}";

			VerifyCSharpFix(test, fixedAllProperties, codeFixIndex: 0);
		}

		[TestMethod]
		public void TestAddMissingPropertiesWithoutInitializer() {
			string test = @"
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1 {
	public record Person {
		public int Id { get; init; }
		public string? FirstName { get; init; }
		
		[Required]
		public string? LastName { get; init; }
	}

	public static class Program {
		public static void Main() {
			var person = new Person();
		}
	}
}";

			string fixedAllProperties = @"
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1 {
	public record Person {
		public int Id { get; init; }
		public string? FirstName { get; init; }
		
		[Required]
		public string? LastName { get; init; }
	}

	public static class Program {
		public static void Main() {
			var person = new Person()
            {
                Id = _,
                FirstName = _,
                LastName = _
            };
		}
	}
}";

			VerifyCSharpFix(test, fixedAllProperties, codeFixIndex: 0);
		}

		[TestMethod]
		public void TestAddMissingRequiredPropertiesWithImplicitCreation() {
			string test = @"
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1 {
	public record Person {
		public int Id { get; init; }
		public string? FirstName { get; init; }
		
		[Required]
		public string? LastName { get; init; }
	}

	public static class Program {
		public static void Main() {
			Person person = new() {
			};
		}
	}
}";

			string fixedRequiredOnly = @"
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication1 {
	public record Person {
		public int Id { get; init; }
		public string? FirstName { get; init; }
		
		[Required]
		public string? LastName { get; init; }
	}

	public static class Program {
		public static void Main() {
			Person person = new() {

                LastName = _
            };
		}
	}
}";

			VerifyCSharpFix(test, fixedRequiredOnly, codeFixIndex: 1);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider() {
			return new AddMissingPropertiesCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
