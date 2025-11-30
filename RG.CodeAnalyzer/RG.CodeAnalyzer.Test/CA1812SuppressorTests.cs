using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace RG.CodeAnalyzer.Test {
	[TestClass]
	public class CA1812SuppressorTests {
		[TestMethod]
		public void Suppressor_SupportsSingleSuppression() {
			CA1812Suppressor suppressor = new();
			
			Assert.AreEqual(1, suppressor.SupportedSuppressions.Length);
		}

		[TestMethod]
		public void Suppressor_SuppressionId_IsRGS001() {
			CA1812Suppressor suppressor = new();
			
			Assert.AreEqual("RGS001", suppressor.SupportedSuppressions[0].Id);
		}

		[TestMethod]
		public void Suppressor_SuppressesCA1812() {
			CA1812Suppressor suppressor = new();
			
			Assert.AreEqual("CA1812", suppressor.SupportedSuppressions[0].SuppressedDiagnosticId);
		}

		[TestMethod]
		public void Suppressor_JustificationMentionsTypeArgument() {
			CA1812Suppressor suppressor = new();
			
			var justification = suppressor.SupportedSuppressions[0].Justification.ToString();
			
			Assert.IsTrue(justification.Contains("type argument"), 
				$"Justification should mention 'type argument'. Actual: {justification}");
		}
	}
}

