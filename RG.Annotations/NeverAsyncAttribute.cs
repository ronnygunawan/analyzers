using System;

namespace RG.Annotations {
	/// <summary>
	/// Indicates that the method returns a Task but never executes asynchronously.
	/// Suppresses RG0006 (Task.Wait) and RG0007 (Task.Result) warnings when calling this method.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class NeverAsyncAttribute : Attribute {
	}
}
