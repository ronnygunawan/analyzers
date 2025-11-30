using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class AddMutableAttributeCodeFixTests : CodeFixVerifier {
		[TestMethod]
		public void TestCodeFixForSetAccessor() {
			string test = @"
using RG.Annotations;

namespace ConsoleApplication1 {
    public record RecordName {
        public int PropertyName { get; set; }
    }
}";

			string fixedCode = @"
using RG.Annotations;

namespace ConsoleApplication1 {
    [Mutable]
    public record RecordName {
        public int PropertyName { get; set; }
    }
}";

			VerifyCSharpFix(test, fixedCode, allowNewCompilerDiagnostics: true);
		}

		[TestMethod]
		public void TestCodeFixForMutableField() {
			string test = @"
using RG.Annotations;

namespace ConsoleApplication1 {
    public record RecordName {
        public int X;
    }
}";

			string fixedCode = @"
using RG.Annotations;

namespace ConsoleApplication1 {
    [Mutable]
    public record RecordName {
        public int X;
    }
}";

			VerifyCSharpFix(test, fixedCode, allowNewCompilerDiagnostics: true);
		}

		[TestMethod]
		public void TestCodeFixForMutableCollection() {
			string test = @"
using RG.Annotations;

namespace ConsoleApplication1 {
    public record RecordName {
        public int[]? Numbers { get; init; }
    }
}";

			string fixedCode = @"
using RG.Annotations;

namespace ConsoleApplication1 {
    [Mutable]
    public record RecordName {
        public int[]? Numbers { get; init; }
    }
}";

			VerifyCSharpFix(test, fixedCode, allowNewCompilerDiagnostics: true);
		}

		[TestMethod]
		public void TestCodeFixForClassReference() {
			string test = @"
using RG.Annotations;

namespace ConsoleApplication1 {
    public class MyClass { }

    public record RecordName {
        public MyClass? Ref { get; init; }
    }
}";

			string fixedCode = @"
using RG.Annotations;

namespace ConsoleApplication1 {
    public class MyClass { }

    [Mutable]
    public record RecordName {
        public MyClass? Ref { get; init; }
    }
}";

			VerifyCSharpFix(test, fixedCode, allowNewCompilerDiagnostics: true);
		}

		[TestMethod]
		public void TestCodeFixPreservesExistingUsing() {
			string test = @"using RG.Annotations;

namespace ConsoleApplication1 {
    public record RecordName {
        public int PropertyName { get; set; }
    }
}";

			string fixedCode = @"using RG.Annotations;

namespace ConsoleApplication1 {
    [Mutable]
    public record RecordName {
        public int PropertyName { get; set; }
    }
}";

			VerifyCSharpFix(test, fixedCode, allowNewCompilerDiagnostics: true);
		}

		[TestMethod]
		public void TestCodeFixPreservesExistingAttributes() {
			string test = @"using System;
using RG.Annotations;

namespace ConsoleApplication1 {
    [Serializable]
    public record RecordName {
        public int PropertyName { get; set; }
    }
}";

			string fixedCode = @"using System;
using RG.Annotations;

namespace ConsoleApplication1 {
    [Mutable]
    [Serializable]
    public record RecordName {
        public int PropertyName { get; set; }
    }
}";

			VerifyCSharpFix(test, fixedCode, allowNewCompilerDiagnostics: true);
		}

		[TestMethod]
		public void TestNoCodeFixWhenMutableAttributeAlreadyPresent() {
			string test = @"
using RG.Annotations;

namespace ConsoleApplication1 {
    [Mutable]
    public record RecordName {
        public int PropertyName { get; set; }
    }
}";

			VerifyCSharpDiagnostic(test);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider() {
			return new AddMutableAttributeCodeFixProvider();
		}
	}
}
