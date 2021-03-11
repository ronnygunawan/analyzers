using System;

namespace RG.Annotations {
	[AttributeUsage(AttributeTargets.Parameter)]
	public class MustBeLockedAttribute : Attribute {
		public string ObjectName { get; }

		public MustBeLockedAttribute(string objectName) {
			ObjectName = objectName;
		}
	}
}
