[PrintOptionsAspect]
[global::Metalama.Framework.Tests.Integration.Tests.Options.CrossProject.ActualOptionsAttribute("FromBaseClass")]
internal class DerivedClass : BaseClass
{
}

[PrintOptionsAspect]
[global::Metalama.Framework.Tests.Integration.Tests.Options.CrossProject.ActualOptionsAttribute("BaseDeclaringClass")]
internal class DerivedOfNested : BaseNestingClass.BaseNestedClass
{
}
