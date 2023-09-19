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

[MyOptions("OtherClass")]
internal class OtherClass
{
    [PrintOptionsAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.CrossProject.ActualOptionsAttribute("OtherClass")]
    internal class C
    {
    }
}
