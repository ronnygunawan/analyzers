using System;

namespace RG.Annotations {
	/// <summary>
	/// Marks a service as having scoped lifetime for dependency injection
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class ScopedAttribute : Attribute {
	}
}
