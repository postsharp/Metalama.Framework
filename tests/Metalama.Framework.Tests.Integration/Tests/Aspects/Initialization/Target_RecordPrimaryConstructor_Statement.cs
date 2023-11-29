using System;
using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.Target_RecordPrimaryConstructor_Statement;

public class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.AddInitializer( builder.Target.PrimaryConstructor, StatementFactory.Parse("x = 13;") );
    }
}

// <target>
[Aspect]
public record TargetRecord()
{
    int x;
}