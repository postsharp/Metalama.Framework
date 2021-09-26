[Introduction]
    [Override]
    public class TargetClass:global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override.IInterface    {


private global::System.Int32 _autoProperty;


global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override.IInterface.AutoProperty
{get    {
        global::System.Console.WriteLine("This is overridden method.");
return this._autoProperty;    }

set    {
        global::System.Console.WriteLine("This is overridden method.");
this._autoProperty=value;        return;
    }
}

global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override.IInterface.Property
{get    {
        global::System.Console.WriteLine("This is overridden method.");
        global::System.Console.WriteLine("This is introduced interface member.");
        return default(global::System.Int32);
    }

set    {
        global::System.Console.WriteLine("This is overridden method.");
        global::System.Console.WriteLine("This is introduced interface member.");
        return;
    }
}

global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override.IInterface.InterfaceMethod()
{
    global::System.Console.WriteLine("This is overridden method.");
    global::System.Console.WriteLine("This is introduced interface member.");
    return default(global::System.Int32);
}

event global::System.EventHandler? global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override.IInterface.Event
{add    {
        global::System.Console.WriteLine("This is overridden method.");
        global::System.Console.WriteLine("This is introduced interface member.");
        return;
    }

remove    {
        global::System.Console.WriteLine("This is overridden method.");
        global::System.Console.WriteLine("This is introduced interface member.");
        return;
    }
}
private global::System.EventHandler? _eventField;



event global::System.EventHandler? global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override.IInterface.EventField
{add    {
        global::System.Console.WriteLine("This is overridden method.");
this._eventField+=value;        return;
    }

remove    {
        global::System.Console.WriteLine("This is overridden method.");
this._eventField-=value;        return;
    }
}    }