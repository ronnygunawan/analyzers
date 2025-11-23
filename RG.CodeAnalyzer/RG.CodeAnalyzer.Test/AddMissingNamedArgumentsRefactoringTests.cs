using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TestHelper;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class AddMissingNamedArgumentsRefactoringTests : DiagnosticVerifier {
		[TestMethod]
		public void TestObjectCreation_OffersRefactoring() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class Person {
		public Person(int id, string firstName, string lastName) { }
	}

	class TypeName
	{
		public void Foo() {
			var person = new Person(1, ""John"", ""Doe"");
		}
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("new Person");
			TextSpan span = new TextSpan(position, "new Person".Length);

			List<CodeAction> actions = GetRefactorings(document, span);

			Assert.IsTrue(actions.Any(a => a.Title.Contains("Add missing named arguments")), 
				"Should offer 'Add missing named arguments' refactoring");
		}

		[TestMethod]
		public void TestObjectCreation_NoParameters_DoesNotOfferRefactoring() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class Person {
		public Person() { }
	}

	class TypeName
	{
		public void Foo() {
			var person = new Person();
		}
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("new Person");
			TextSpan span = new TextSpan(position, "new Person".Length);

			List<CodeAction> actions = GetRefactorings(document, span);

			Assert.IsFalse(actions.Any(a => a.Title.Contains("Add missing named arguments")), 
				"Should not offer refactoring for parameterless constructor");
		}

		[TestMethod]
		public void TestObjectCreation_ApplyRefactoring_AddsNamedArguments() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class Person {
		public Person(int id, string firstName, string lastName) { }
	}

	class TypeName
	{
		public void Foo() {
			var person = new Person(1, ""John"", ""Doe"");
		}
	}
}";
			string expected = @"
using System;

namespace ConsoleApplication1
{
	class Person {
		public Person(int id, string firstName, string lastName) { }
	}

	class TypeName
	{
		public void Foo() {
			var person = new Person(
				id: 1,
				firstName: ""John"",
				lastName: ""Doe""
			);
		}
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("new Person");
			TextSpan span = new TextSpan(position, "new Person".Length);

			List<CodeAction> actions = GetRefactorings(document, span);
			CodeAction? action = actions.FirstOrDefault(a => a.Title.Contains("Add missing named arguments"));

			Assert.IsNotNull(action, "Should find the refactoring action");

			Document newDocument = ApplyRefactoring(document, action);
			string newCode = GetStringFromDocument(newDocument);

			Assert.AreEqual(NormalizeWhitespace(expected), NormalizeWhitespace(newCode));
		}

		[TestMethod]
		public void TestMethodInvocation_OffersRefactoring() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class TypeName
	{
		public void Foo() {
			DoSomething(1, ""test"");
		}

		private void DoSomething(int id, string name) { }
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("DoSomething(1");
			TextSpan span = new TextSpan(position, "DoSomething".Length);

			List<CodeAction> actions = GetRefactorings(document, span);

			Assert.IsTrue(actions.Any(a => a.Title.Contains("Add missing named arguments")), 
				"Should offer 'Add missing named arguments' refactoring for method calls");
		}

		[TestMethod]
		public void TestMethodInvocation_ApplyRefactoring_AddsNamedArguments() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class TypeName
	{
		public void Foo() {
			DoSomething(1, ""test"");
		}

		private void DoSomething(int id, string name) { }
	}
}";
			string expected = @"
using System;

namespace ConsoleApplication1
{
	class TypeName
	{
		public void Foo() {
			DoSomething(
				id: 1,
				name: ""test""
			);
		}

		private void DoSomething(int id, string name) { }
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("DoSomething(1");
			TextSpan span = new TextSpan(position, "DoSomething".Length);

			List<CodeAction> actions = GetRefactorings(document, span);
			CodeAction? action = actions.FirstOrDefault(a => a.Title.Contains("Add missing named arguments"));

			Assert.IsNotNull(action, "Should find the refactoring action");

			Document newDocument = ApplyRefactoring(document, action);
			string newCode = GetStringFromDocument(newDocument);

			Assert.AreEqual(NormalizeWhitespace(expected), NormalizeWhitespace(newCode));
		}

		[TestMethod]
		public void TestObjectCreation_MultipleOverloads_OffersMultipleRefactorings() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class Person {
		public Person(int id) { }
		public Person(int id, string firstName) { }
		public Person(int id, string firstName, string lastName) { }
	}

	class TypeName
	{
		public void Foo() {
			var person = new Person(1);
		}
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("new Person");
			TextSpan span = new TextSpan(position, "new Person".Length);

			List<CodeAction> actions = GetRefactorings(document, span);

			var namedArgActions = actions.Where(a => a.Title.Contains("Add missing named argument")).ToList();
			Assert.AreEqual(3, namedArgActions.Count, 
				$"Should offer 3 refactorings for 3 overloads. Found {namedArgActions.Count}");
			
			Assert.IsTrue(namedArgActions.Any(a => a.Title == "Add missing named argument (id)"),
				"Should offer refactoring for Person(int id)");
			Assert.IsTrue(namedArgActions.Any(a => a.Title == "Add missing named arguments (id, firstName)"),
				"Should offer refactoring for Person(int id, string firstName)");
			Assert.IsTrue(namedArgActions.Any(a => a.Title == "Add missing named arguments (id, firstName, lastName)"),
				"Should offer refactoring for Person(int id, string firstName, string lastName)");
		}

		[TestMethod]
		public void TestSingleParameter_UsesSingularForm() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class Person {
		public Person(int id) { }
	}

	class TypeName
	{
		public void Foo() {
			var person = new Person(1);
		}
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("new Person");
			TextSpan span = new TextSpan(position, "new Person".Length);

			List<CodeAction> actions = GetRefactorings(document, span);

			Assert.IsTrue(actions.Any(a => a.Title.Contains("Add missing named argument") && !a.Title.Contains("arguments")), 
				"Should use singular 'argument' for single parameter");
		}

		[TestMethod]
		public void TestObjectCreation_NoArgumentList_AddsArguments() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class Person {
		public Person(int id, string name) { }
	}

	class TypeName
	{
		public void Foo() {
			var person = new Person();
		}
	}
}";
			// Note: This will result in a compilation error because we're not passing 
			// required parameters, but the refactoring should still work
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("new Person");
			TextSpan span = new TextSpan(position, "new Person".Length);

			List<CodeAction> actions = GetRefactorings(document, span);

			// Should offer refactoring even without existing arguments
			Assert.IsTrue(actions.Any(a => a.Title.Contains("Add missing named argument")));
		}

		[TestMethod]
		public void TestPartialArguments_PreservesExistingAndAddsPlaceholders() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class TypeName
	{
		public void Foo() {
			DoSomething(1);
		}

		private void DoSomething(int id, string name) { }
	}
}";
			string expected = @"
using System;

namespace ConsoleApplication1
{
	class TypeName
	{
		public void Foo() {
			DoSomething(
				id: 1,
				name: _
			);
		}

		private void DoSomething(int id, string name) { }
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("DoSomething(1");
			TextSpan span = new TextSpan(position, "DoSomething".Length);

			List<CodeAction> actions = GetRefactorings(document, span);
			CodeAction? action = actions.FirstOrDefault(a => a.Title.Contains("Add missing named arguments"));

			Assert.IsNotNull(action, "Should find the refactoring action");

			Document newDocument = ApplyRefactoring(document, action);
			string newCode = GetStringFromDocument(newDocument);

			Assert.AreEqual(NormalizeWhitespace(expected), NormalizeWhitespace(newCode));
		}

		[TestMethod]
		public void TestMultipleOverloads_WithPartialArguments_OffersMatchingOverloads() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class TypeName
	{
		public void Foo() {
			DoSomething(1);
		}

		private void DoSomething(int id) { }
		private void DoSomething(int id, string name) { }
		private void DoSomething(int id, string name, bool active) { }
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("DoSomething(1");
			TextSpan span = new TextSpan(position, "DoSomething".Length);

			List<CodeAction> actions = GetRefactorings(document, span);

			var namedArgActions = actions.Where(a => a.Title.Contains("Add missing named argument")).ToList();
			
			// Should offer all three overloads
			Assert.AreEqual(3, namedArgActions.Count, 
				$"Should offer 3 refactorings for 3 overloads that can accept the current argument. Found {namedArgActions.Count}");
			
			// Verify the exact titles
			Assert.IsTrue(namedArgActions.Any(a => a.Title == "Add missing named argument (id)"),
				"Should offer refactoring for DoSomething(int id)");
			Assert.IsTrue(namedArgActions.Any(a => a.Title == "Add missing named arguments (id, name)"),
				"Should offer refactoring for DoSomething(int id, string name)");
			Assert.IsTrue(namedArgActions.Any(a => a.Title == "Add missing named arguments (id, name, active)"),
				"Should offer refactoring for DoSomething(int id, string name, bool active)");
		}

		[TestMethod]
		public void TestObjectCreation_PartialArguments_AddsPlaceholders() {
			string test = @"
using System;

namespace ConsoleApplication1
{
	class Person {
		public Person(int id, string firstName, string lastName) { }
	}

	class TypeName
	{
		public void Foo() {
			var person = new Person(42);
		}
	}
}";
			string expected = @"
using System;

namespace ConsoleApplication1
{
	class Person {
		public Person(int id, string firstName, string lastName) { }
	}

	class TypeName
	{
		public void Foo() {
			var person = new Person(
				id: 42,
				firstName: _,
				lastName: _
			);
		}
	}
}";
			Document document = CreateDocument(test, LanguageNames.CSharp);
			
			int position = test.IndexOf("new Person");
			TextSpan span = new TextSpan(position, "new Person".Length);

			List<CodeAction> actions = GetRefactorings(document, span);
			CodeAction? action = actions.FirstOrDefault(a => a.Title.Contains("Add missing named arguments"));

			Assert.IsNotNull(action, "Should find the refactoring action");

			Document newDocument = ApplyRefactoring(document, action);
			string newCode = GetStringFromDocument(newDocument);

			Assert.AreEqual(NormalizeWhitespace(expected), NormalizeWhitespace(newCode));
		}

		private List<CodeAction> GetRefactorings(Document document, TextSpan span) {
			AddMissingNamedArgumentsRefactoringProvider provider = new();
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

		private string NormalizeWhitespace(string code) {
			SyntaxTree tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
			return tree.GetRoot().NormalizeWhitespace().ToFullString();
		}
	}
}
