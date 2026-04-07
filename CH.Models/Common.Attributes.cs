using System;

namespace CH.Models.Common
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TypescriptIncludeAttribute : Attribute
	{
		public string Namespace { get; }
		public string ModelName { get; }

		public TypescriptIncludeAttribute(string modelNamespace = null, string modelName = null)
		{
			this.Namespace = modelNamespace;
			this.ModelName = modelName;
		}
	}

	/// <summary>
	/// This attribute can be added to a class to skip the class when generating
	/// client typescript models.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class TypescriptIgnoreAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Property)]
	public class TypescriptOptionalAttribute : Attribute
	{ }

	#region MyPHA

	[AttributeUsage(AttributeTargets.Class)]
	public class MyPhaTypescriptIncludeAttribute : Attribute
	{
		public string Namespace { get; }
		public string ModelName { get; }

		public MyPhaTypescriptIncludeAttribute(string modelNamespace = null, string modelName = null)
		{
			this.Namespace = modelNamespace;
			this.ModelName = modelName;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class MyPhaTypescriptIgnoreAttribute : Attribute
	{ }

	#endregion


	#region Management Portal

	[AttributeUsage(AttributeTargets.Class)]
	public class ManagementPortalTypescriptIncludeAttribute : Attribute
	{
		public string Namespace { get; }
		public string ModelName { get; }

		public ManagementPortalTypescriptIncludeAttribute(string modelNamespace = null, string modelName = null)
		{
			this.Namespace = modelNamespace;
			this.ModelName = modelName;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class ManagementPortalTypescriptIgnoreAttribute : Attribute
	{ }

	#endregion


}
