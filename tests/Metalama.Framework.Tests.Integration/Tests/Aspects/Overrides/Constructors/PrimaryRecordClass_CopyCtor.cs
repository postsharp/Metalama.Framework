using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.PrimaryRecordClass_CopyCtor;

/*
 * Tests single OverrideConstructor advice on a copy-ctor constructor of a record struct (should get an eligibility error).
 */

public class OverrideAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach (var constructor in builder.Target.Constructors)
        {
            if (!constructor.IsImplicitlyDeclared)
            {
                continue;
            }

            builder.Advice.Override(constructor, nameof(Template));
        }
    }

    [Template]
    public void Template()
    {
        meta.Proceed();
    }
}

// <target>
[Override]
public record class TargetClass(int X, int Y)
{
}