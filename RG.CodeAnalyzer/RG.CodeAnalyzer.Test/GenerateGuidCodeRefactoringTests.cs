using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class GenerateGuidCodeRefactoringTests {
		[TestMethod]
		public async Task TestGenerateGuidInEmptyString() {
			string source = @"
using System;

namespace ConsoleApplication1
{
	public class TypeName
	{
		public void Foo() {
			string id = """";
		}
	}
}";

			// The position of the empty string ""
			int position = source.IndexOf("\"\"");
			TextSpan span = new TextSpan(position, 2);

			var refactoringProvider = new GenerateGuidCodeRefactoringProvider();
			var document = CreateDocument(source);
			var actions = new List<CodeAction>();

			var context = new CodeRefactoringContext(
				document,
				span,
				a => actions.Add(a),
				CancellationToken.None);

			await refactoringProvider.ComputeRefactoringsAsync(context);

			Assert.AreEqual(1, actions.Count, "Expected exactly one refactoring action");
			Assert.AreEqual("Generate GUID", actions[0].Title);

			// Apply the refactoring
			var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
			var operation = operations.OfType<ApplyChangesOperation>().Single();
			var changedSolution = operation.ChangedSolution;
			var changedDocument = changedSolution.GetDocument(document.Id);
			var changedText = (await changedDocument!.GetTextAsync()).ToString();

			// Verify that the empty string was replaced with a GUID
			Assert.IsFalse(changedText.Contains("\"\""), "Empty string should be replaced");
			
			// Verify that there's a string that looks like a GUID (format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
			var guidPattern = new System.Text.RegularExpressions.Regex(@"""[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}""");
			Assert.IsTrue(guidPattern.IsMatch(changedText), "Should contain a GUID-formatted string");
		}

		[TestMethod]
		public async Task TestNoRefactoringForNonEmptyString() {
			string source = @"
using System;

namespace ConsoleApplication1
{
	public class TypeName
	{
		public void Foo() {
			string id = ""not empty"";
		}
	}
}";

			// The position of the string
			int position = source.IndexOf("\"not empty\"");
			TextSpan span = new TextSpan(position, 11);

			var refactoringProvider = new GenerateGuidCodeRefactoringProvider();
			var document = CreateDocument(source);
			var actions = new List<CodeAction>();

			var context = new CodeRefactoringContext(
				document,
				span,
				a => actions.Add(a),
				CancellationToken.None);

			await refactoringProvider.ComputeRefactoringsAsync(context);

			Assert.AreEqual(0, actions.Count, "Should not offer refactoring for non-empty strings");
		}

		private static Document CreateDocument(string source) {
			var projectId = ProjectId.CreateNewId(debugName: "TestProject");
			var documentId = DocumentId.CreateNewId(projectId, debugName: "Test.cs");

			var solution = new AdhocWorkspace()
				.CurrentSolution
				.AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
				.AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
				.AddDocument(documentId, "Test.cs", SourceText.From(source));

			return solution.GetDocument(documentId)!;
		}
	}
}
