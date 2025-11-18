using System;

namespace RG.Annotations {
	/// <summary>
	/// Marks a service as having singleton lifetime for dependency injection
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class SingletonAttribute : Attribute {
	}
}
