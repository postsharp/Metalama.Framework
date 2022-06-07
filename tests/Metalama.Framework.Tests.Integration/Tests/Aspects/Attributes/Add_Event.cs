using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event;

public class MyAttribute : Attribute { }

public class MyAspect : EventAspect
{
    public override void BuildAspect( IAspectBuilder<IEvent> builder )
    {
        builder.Advice.AddAttribute( builder.Target, AttributeConstruction.Create( typeof(MyAttribute) ) );
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