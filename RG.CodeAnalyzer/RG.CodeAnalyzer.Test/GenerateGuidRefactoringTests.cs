using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class GenerateGuidRefactoringTests : DiagnosticVerifier {
		[TestMethod]
		public void TestEmptyStringLiteral_OffersRefactoring() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class TypeName
	{
		public void Foo() {
			string s = """";
		}
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			// Position should be on the empty string literal ""
			int position = test.IndexOf("\"\"");
			TextSpan span = new TextSpan(position, 2);

			List<CodeAction> actions = GetRefactorings(document, span);

			Assert.IsTrue(actions.Any(a => a.Title == "Generate GUID"), "Should offer 'Generate GUID' refactoring");
		}

		[TestMethod]
		public void TestNonEmptyStringLiteral_DoesNotOfferRefactoring() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class TypeName
	{
		public void Foo() {
			string s = ""hello"";
		}
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("\"hello\"");
			TextSpan span = new TextSpan(position, 7);

			List<CodeAction> actions = GetRefactorings(document, span);

			Assert.IsFalse(actions.Any(a => a.Title == "Generate GUID"), "Should not offer 'Generate GUID' refactoring for non-empty string");
		}

		[TestMethod]
		public void TestApplyRefactoring_ReplacesWithGuid() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class TypeName
	{
		public void Foo() {
			string s = """";
		}
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("\"\"");
			TextSpan span = new TextSpan(position, 2);

			List<CodeAction> actions = GetRefactorings(document, span);
			CodeAction? guidAction = actions.FirstOrDefault(a => a.Title == "Generate GUID");

			Assert.IsNotNull(guidAction, "Should find the Generate GUID action");

			Document newDocument = ApplyRefactoring(document, guidAction);
			string newCode = GetStringFromDocument(newDocument);

			// Verify that the empty string has been replaced with a GUID
			Assert.IsFalse(newCode.Contains("string s = \"\";"), "Empty string should be replaced");
			
			// Verify the result contains a string that looks like a GUID
			// GUIDs are 36 characters with hyphens: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
			string pattern = @"string s = ""[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"";";
			Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(newCode, pattern), 
				$"Should contain a GUID pattern. Actual code: {newCode}");
		}

		private List<CodeAction> GetRefactorings(Document document, TextSpan span) {
			GenerateGuidRefactoringProvider provider = new();
			List<CodeAction> actions = new();
			
			CodeRefactoringContext context = new(
				document,
				span,
				(a) => actions.Add(a),
				CancellationToken.None);

			provider.ComputeRefactoringsAsync(context).Wait();

			return actions;
		}

		private Document ApplyRefactoring(Document document, CodeAction codeAction) {
			var operations = codeAction.GetOperationsAsync(CancellationToken.None).Result;
			var solution = operations.OfType<Microsoft.CodeAnalysis.CodeActions.ApplyChangesOperation>().Single().ChangedSolution;
			return solution.GetDocument(document.Id)!;
		}

		private string GetStringFromDocument(Document document) {
			SyntaxNode root = document.GetSyntaxRootAsync().Result!;
			return root.ToFullString();
		}
	}
}
