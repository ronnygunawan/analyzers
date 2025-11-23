using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class NullableReferenceTypeNotEnabledTests : CodeFixVerifier {

		[TestMethod]
		public void TestEmptyCode() {
			string test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestNullableEnabled() {
			string test = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		public string? Name { get; set; }
	}
}
";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestNullableDisabled() {
			string test = @"
namespace ConsoleApplication1
{
	public class TypeName
	{
		public string Name { get; set; }
	}
}
";

			DiagnosticResult expected = new() {
				Id = "RG0039",
				Message = "Nullable reference type static analysis is not enabled",
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", 1, 1)
					}
			};

			Document document = CreateDocumentWithNullableDisabled(test);
			Diagnostic[] diagnostics = GetSortedDiagnosticsFromDocuments(GetCSharpDiagnosticAnalyzer(), new[] { document });

			Assert.AreEqual(1, diagnostics.Length, "Expected exactly one diagnostic");
			Assert.AreEqual(expected.Id, diagnostics[0].Id, "Diagnostic ID mismatch");
			Assert.AreEqual(expected.Severity, diagnostics[0].Severity, "Diagnostic severity mismatch");
			Assert.AreEqual(expected.Message, diagnostics[0].GetMessage(), "Diagnostic message mismatch");
		}

		private static Document CreateDocumentWithNullableDisabled(string source) {
			string fileNamePrefix = "Test";
			string fileExt = "cs";
			string testProjectName = "TestProject";

			var projectId = ProjectId.CreateNewId(debugName: testProjectName);

			var solution = new Microsoft.CodeAnalysis.AdhocWorkspace()
				.CurrentSolution
				.AddProject(projectId, testProjectName, testProjectName, LanguageNames.CSharp);

			var project = solution.GetProject(projectId)!;
			
			if (project.CompilationOptions is CSharpCompilationOptions csharpOptions) {
				project = project.WithCompilationOptions(
					csharpOptions.WithNullableContextOptions(NullableContextOptions.Disable)
				);
			}

			project = project.AddMetadataReferences(new[] {
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
			});

			string newFileName = fileNamePrefix + "0." + fileExt;
			DocumentId documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
			project = project.Solution.AddDocument(documentId, newFileName, SourceText.From(source)).GetProject(projectId)!;

			return project.GetDocument(documentId)!;
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
			return new RGDiagnosticAnalyzer();
		}
	}
}
