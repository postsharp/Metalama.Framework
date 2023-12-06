using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.PrimaryConstructor_Record_CopyConstructor;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach (var constructor in builder.Target.Constructors)
        {
            builder.Advice.IntroduceParameter( constructor, "p", typeof(int), TypedConstant.Create( 15 ) );
        }
    }
}

public record A(int x)
{
    public int X { get; set; } = x;
}

// <target>
[MyAspect]
public record C(int x) : A(42)
{
    public int Y { get; } = x;
}