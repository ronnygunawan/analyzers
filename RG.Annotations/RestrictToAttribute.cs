using System;

namespace RG.Annotations {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Event)]
	public class RestrictToAttribute : Attribute {
		public string Namespace { get; }

		public RestrictToAttribute(string @namespace) {
			Namespace = @namespace;
		}
	}
}
