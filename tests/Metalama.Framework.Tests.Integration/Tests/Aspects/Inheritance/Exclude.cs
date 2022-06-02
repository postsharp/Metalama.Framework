using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Inheritance.Exclude;

[Inherited]
internal class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach (var m in builder.Target.Methods)
        {
            builder.Advice.Override( m, nameof(Template) );
        }
    }

    [Template]
    private dynamic? Template()
    {
        Console.WriteLine( "Overridden!" );

        return meta.Proceed();
    }
}

// <target>
internal class Targets
{
    [Aspect]
    private class BaseClass
    {
        private void M() { }
    }

    private class DerivedClass : BaseClass
    {
        private void N() { }
    }

    [ExcludeAspect( typeof(Aspect) )]
    private class ExcludedDerivedClass : BaseClass
    {
        private void N() { }
    }
}