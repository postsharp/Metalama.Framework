using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeReturningRunTimeAccess;

class Aspect : PropertyAspect
{
    public override void BuildAspect(IAspectBuilder<IProperty> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.OverrideAccessors(builder.Target, nameof(OverrideGetter));
    }

    [Template]
    private dynamic? OverrideGetter()
    {
        var field = (meta.Target.Property.ToFieldOrPropertyInfo().GetCustomAttributes(true).SingleOrDefault(x => x is IEntityField) as IEntityField)
            ?? throw new Exception("Unable to retrieve field info.");

        Console.WriteLine(field);

        return meta.Proceed();
    }
}

interface IEntityField { }

class EntityFieldAttribute : Attribute, IEntityField { }

class Target
{
    // <target>
    [Aspect]
    [EntityField]
    public string? Name { get; set; }
}