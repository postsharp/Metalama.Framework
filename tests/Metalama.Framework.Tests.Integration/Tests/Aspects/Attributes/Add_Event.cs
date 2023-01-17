using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event;

#pragma warning disable CS8618, CS0169, CS0067

public class MyAttribute : Attribute { }

public class MyAspect : EventAspect
{
    public override void BuildAspect( IAspectBuilder<IEvent> builder )
    {
        builder.Advise.IntroduceAttribute( builder.Target, AttributeConstruction.Create( typeof(MyAttribute) ) );

        if (builder.Target.AddMethod != null)
        {
            builder.Advise.IntroduceAttribute( builder.Target.AddMethod, AttributeConstruction.Create( typeof(MyAttribute) ) );
        }

        if (builder.Target.RemoveMethod != null)
        {
            builder.Advise.IntroduceAttribute( builder.Target.RemoveMethod, AttributeConstruction.Create( typeof(MyAttribute) ) );
        }
    }
}

// <target>
internal class C
{
    [MyAspect]
    private event Action Event1, Event2;

    [MyAspect]
    private event Action Event3;

    [MyAspect]
    private event Action Event4
    {
        add { }
        remove { }
    }
}