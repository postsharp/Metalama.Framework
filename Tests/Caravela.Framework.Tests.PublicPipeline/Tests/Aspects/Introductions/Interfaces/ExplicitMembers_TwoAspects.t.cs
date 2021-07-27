// Warning CS8615 on `EventField`: `Nullability of reference types in type doesn't match implemented member 'event EventHandler? IInterface.EventField'.`
// Warning CS8601 on `this._eventField-=value`: `Possible null reference assignment.`
[Introduction]
    [Override]
    public class TargetClass:global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_TwoAspects.IInterface    {
    
    
global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_TwoAspects.IInterface.InterfaceMethod()
{
    global::System.Console.WriteLine("This is overridden method.");
    global::System.Console.WriteLine("This is introduced interface member.");
    return default(global::System.Int32);
}
    
global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_TwoAspects.IInterface.Property
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
    
private global::System.Int32 _autoProperty;
    
    
global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_TwoAspects.IInterface.AutoProperty
{get    {
        global::System.Console.WriteLine("This is overridden method.");
return this._autoProperty;    }
    
set    {
        global::System.Console.WriteLine("This is overridden method.");
this._autoProperty=value;        return;
    }
}
    
event global::System.EventHandler global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_TwoAspects.IInterface.Event
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
private global::System.EventHandler _eventField;
    
    
    
event global::System.EventHandler global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_TwoAspects.IInterface.EventField
{add    {
        global::System.Console.WriteLine("This is overridden method.");
this._eventField+=value;        return;
    }
    
remove    {
        global::System.Console.WriteLine("This is overridden method.");
this._eventField-=value;        return;
    }
}    }