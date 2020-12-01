using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TestHelper {
	/// <summary>
	/// Class for turning strings into documents and getting the diagnostics on them
	/// All methods are static
	/// </summary>
	public abstract partial class DiagnosticVerifier {
		private static readonly MetadataReference CORLIB_REFERENCE = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
		private static readonly MetadataReference SYSTEM_CORE_REFERENCE = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
		private static readonly MetadataReference CSHARP_SYMBOLS_REFERENCE = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
		private static readonly MetadataReference CODE_ANALYSIS_REFERENCE = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
		private static readonly MetadataReference IMMUTABLE_REFERENCE = MetadataReference.CreateFromFile(typeof(ImmutableArray).Assembly.Location);
		private static readonly MetadataReference SERIALIZATION_REFERENCE = MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location);

		internal static string DefaultFilePathPrefix = "Test";
		internal static string CSharpDefaultFileExt = "cs";
		internal static string VisualBasicDefaultExt = "vb";
		internal static string TestProjectName = "TestProject";

		#region  Get Diagnostics

		/// <summary>
		/// Given classes in the form of strings, their language, and an IDiagnosticAnalyzer to apply to it, return the diagnostics found in the string after converting it to a document.
		/// </summary>
		/// <param name="sources">Classes in the form of strings</param>
		/// <param name="language">The language the source classes are in</param>
		/// <param name="analyzer">The analyzer to be run on the sources</param>
		/// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
		private static Diagnostic[] GetSortedDiagnostics(string[] sources, string language, DiagnosticAnalyzer analyzer) {
			return GetSortedDiagnosticsFromDocuments(analyzer, GetDocuments(sources, language));
		}

		/// <summary>
		/// Given an analyzer and a document to apply it to, run the analyzer and gather an array of diagnostics found in it.
		/// The returned diagnostics are then ordered by location in the source document.
		/// </summary>
		/// <param name="analyzer">The analyzer to run on the documents</param>
		/// <param name="documents">The Documents that the analyzer will be run on</param>
		/// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
		protected static Diagnostic[] GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer analyzer, Document[] documents) {
			HashSet<Project> projects = new();
			foreach (Document document in documents) {
				projects.Add(document.Project);
			}

			List<Diagnostic> diagnostics = new();
			foreach (Project project in projects) {
				CompilationWithAnalyzers compilationWithAnalyzers = project.GetCompilationAsync().Result!.WithAnalyzers(ImmutableArray.Create(analyzer));
				ImmutableArray<Diagnostic> diags = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
				foreach (Diagnostic diag in diags) {
					if (diag.Location == Location.None || diag.Location.IsInMetadata) {
						diagnostics.Add(diag);
					} else {
						for (int i = 0; i < documents.Length; i++) {
							Document document = documents[i];
							SyntaxTree tree = document.GetSyntaxTreeAsync().Result!;
							if (tree == diag.Location.SourceTree) {
								diagnostics.Add(diag);
							}
						}
					}
				}
			}

			Diagnostic[] results = SortDiagnostics(diagnostics);
			diagnostics.Clear();
			return results;
		}

		/// <summary>
		/// Sort diagnostics by location in source document
		/// </summary>
		/// <param name="diagnostics">The list of Diagnostics to be sorted</param>
		/// <returns>An IEnumerable containing the Diagnostics in order of Location</returns>
		private static Diagnostic[] SortDiagnostics(IEnumerable<Diagnostic> diagnostics) {
			return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
		}

		#endregion

		#region Set up compilation and documents
		/// <summary>
		/// Given an array of strings as sources and a language, turn them into a project and return the documents and spans of it.
		/// </summary>
		/// <param name="sources">Classes in the form of strings</param>
		/// <param name="language">The language the source code is in</param>
		/// <returns>A Tuple containing the Documents produced from the sources and their TextSpans if relevant</returns>
		private static Document[] GetDocuments(string[] sources, string language) {
			if (language != LanguageNames.CSharp && language != LanguageNames.VisualBasic) {
				throw new ArgumentException("Unsupported Language");
			}

			Project project = CreateProject(sources, language);
			Document[] documents = project.Documents.ToArray();

			if (sources.Length != documents.Length) {
				throw new InvalidOperationException("Amount of sources did not match amount of Documents created");
			}

			return documents;
		}

		/// <summary>
		/// Create a Document from a string through creating a project that contains it.
		/// </summary>
		/// <param name="source">Classes in the form of a string</param>
		/// <param name="language">The language the source code is in</param>
		/// <returns>A Document created from the source string</returns>
		protected static Document CreateDocument(string source, string language = LanguageNames.CSharp) {
			return CreateProject(new[] { source }, language).Documents.First();
		}

		/// <summary>
		/// Create a project using the inputted strings as sources.
		/// </summary>
		/// <param name="sources">Classes in the form of strings</param>
		/// <param name="language">The language the source code is in</param>
		/// <returns>A Project created out of the Documents created from the source strings</returns>
		private static Project CreateProject(string[] sources, string language = LanguageNames.CSharp) {
			string fileNamePrefix = DefaultFilePathPrefix;
			string fileExt = language == LanguageNames.CSharp ? CSharpDefaultFileExt : VisualBasicDefaultExt;

			ProjectId projectId = ProjectId.CreateNewId(debugName: TestProjectName);

			Solution solution = new AdhocWorkspace()
				.CurrentSolution
				.AddProject(projectId, TestProjectName, TestProjectName, language)
				.AddMetadataReference(projectId, CORLIB_REFERENCE)
				.AddMetadataReference(projectId, SYSTEM_CORE_REFERENCE)
				.AddMetadataReference(projectId, CSHARP_SYMBOLS_REFERENCE)
				.AddMetadataReference(projectId, CODE_ANALYSIS_REFERENCE)
				.AddMetadataReference(projectId, IMMUTABLE_REFERENCE)
				.AddMetadataReference(projectId, SERIALIZATION_REFERENCE);

			int count = 0;
			foreach (string source in sources) {
				string newFileName = fileNamePrefix + count + "." + fileExt;
				DocumentId documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
				solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
				count++;
			}
			return solution.GetProject(projectId)!;
		}
		#endregion
	}
}

