using System;
using System.ComponentModel;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue31071;

public abstract class RootXPObject
{
    public virtual void AfterConstruction() { }
}

public abstract class BaseXPObject : RootXPObject
{
    public override void AfterConstruction()
    {
        base.AfterConstruction();
    }
}

// <target>
[XpoDefaultValueAutoImplementation]
public sealed class MyXpObject : BaseXPObject { }

public sealed class XpoDefaultValueAutoImplementationAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceMethod(
            builder.Target,
            nameof(AfterConstruction),
            whenExists: OverrideStrategy.Override );
    }

    [Template]
    public void AfterConstruction()
    {
        Console.WriteLine( "Overridden!" );

        meta.Proceed();
    }

    private TypedConstant GetDefaultValue( IFieldOrProperty field )
    {
        var defaultValueAttribute = field.Attributes.FirstOrDefault( a => a.Type.Is( typeof(DefaultValueAttribute) ) );

        if (defaultValueAttribute is null)
        {
            throw new InvalidOperationException( "Could not get DefaultValueAttribute!?" );
        }

        var value = defaultValueAttribute.ConstructorArguments.Length == 1
            ? defaultValueAttribute.ConstructorArguments[0]
            : defaultValueAttribute.ConstructorArguments[1];

        return value;
    }
}