// Warning CS8615 on `EventField`: `Nullability of reference types in type doesn't match implemented member 'event EventHandler? IInterface.EventField'.`
// Warning CS8601 on `this._eventField -= value`: `Possible null reference assignment.`
[Introduction]
    public class TargetClass:global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers.IInterface    {


global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers.IInterface.AutoProperty
{
    get;
    set;
}

global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers.IInterface.Property
{get    {
        global::System.Console.WriteLine("This is introduced interface member.");
        return default(global::System.Int32);
    }

set    {
        global::System.Console.WriteLine("This is introduced interface member.");
    }
}

global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers.IInterface.InterfaceMethod()
{
    global::System.Console.WriteLine("This is introduced interface member.");
    return default(global::System.Int32);
}

event global::System.EventHandler global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers.IInterface.Event
{add    {
        global::System.Console.WriteLine("This is introduced interface member.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced interface member.");
    }
}
private global::System.EventHandler _eventField;

event global::System.EventHandler global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers.IInterface.EventField
{
    add
    {
        this._eventField += value;
    }

    remove
    {
        this._eventField -= value;
    }
}    }