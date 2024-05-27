using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Constructor_Introduced;

internal class TestAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var constructor =
            builder.Advice.IntroduceConstructor(
                builder.Target,
                nameof(ConstructorTemplate),
                buildConstructor: b =>
                {
                    b.AddParameter("p", typeof(object));
                }).Declaration;

        builder.Advice.AddContract(constructor.Parameters.Single(), nameof(ValidationTemplate));
    }

    [Template]
    public void ConstructorTemplate()
    {
    }

    [Template]
    public void ValidationTemplate( dynamic? value )
    {
        if (value == null)
        {
            throw new ArgumentNullException( meta.Target.Parameter.Name );
        }
    }
}

// <target>
[Test]
internal class Target
{
}