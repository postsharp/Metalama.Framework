using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.Target_RecordPrimaryConstructor_Statement;

#pragma warning disable CS0169 // field is never used

public class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.AddInitializer( builder.Target.PrimaryConstructor, StatementFactory.Parse("x = 13;") );
        builder.Advice.AddInitializer(builder.Target.PrimaryConstructor, StatementFactory.Parse("Y = 27;"));
    }
}

// <target>
[Aspect]
public record TargetRecord()
{
    private readonly int x = 0;

    public int Y { get; } = 0;

    public int Foo() => x;
}