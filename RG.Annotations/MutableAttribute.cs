using System;

namespace RG.Annotations {
	/// <summary>
	/// Skip immutability checks and treat annotated type as a mutable class or record
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class MutableAttribute : Attribute {
	}
}
