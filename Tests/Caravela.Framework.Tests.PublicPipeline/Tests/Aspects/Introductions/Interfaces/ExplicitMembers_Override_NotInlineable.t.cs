    [Introduction]
    [Override]
    public class TargetClass:global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override_NotInlineable.IInterface    {


global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override_NotInlineable.IInterface.InterfaceMethod()
{
    global::System.Console.WriteLine("This is overridden method.");
    _ = this.IInterface_InterfaceMethod_Introduction();
    return this.IInterface_InterfaceMethod_Introduction();
}

global::System.Int32 IInterface_InterfaceMethod_Introduction()
{
    global::System.Console.WriteLine("This is introduced interface member.");
    return default(global::System.Int32);
}

global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override_NotInlineable.IInterface.Property
{get    {
        global::System.Console.WriteLine("This is overridden method.");
        _ = this.IInterface_Property_Introduction;
        return this.IInterface_Property_Introduction;
    }

set    {
        global::System.Console.WriteLine("This is overridden method.");
this.IInterface_Property_Introduction= value;
this.IInterface_Property_Introduction= value;
        return;
    }
}

global::System.Int32 IInterface_Property_Introduction
{get    {
        global::System.Console.WriteLine("This is introduced interface member.");
        return default(global::System.Int32);
    }

set    {
        global::System.Console.WriteLine("This is introduced interface member.");
    }
}

private global::System.Int32 _autoProperty;


global::System.Int32 global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override_NotInlineable.IInterface.AutoProperty
{get    {
        global::System.Console.WriteLine("This is overridden method.");
        _ = this.AutoProperty_Source;
        return this.AutoProperty_Source;
    }

set    {
        global::System.Console.WriteLine("This is overridden method.");
this.AutoProperty_Source= value;
this.AutoProperty_Source= value;
        return;
    }
}
private global::System.Int32 AutoProperty_Source
{
    get
    {
        return this._autoProperty;
    }

    set
    {
        this._autoProperty = value;
    }
}

event global::System.EventHandler? global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override_NotInlineable.IInterface.Event
{add    {
        global::System.Console.WriteLine("This is overridden method.");
this.IInterface_Event_Introduction+= value;
this.IInterface_Event_Introduction+= value;
        return;
    }

remove    {
        global::System.Console.WriteLine("This is overridden method.");
this.IInterface_Event_Introduction-= value;
this.IInterface_Event_Introduction-= value;
        return;
    }
}

event global::System.EventHandler? IInterface_Event_Introduction
{add    {
        global::System.Console.WriteLine("This is introduced interface member.");
    }

remove    {
        global::System.Console.WriteLine("This is introduced interface member.");
    }
}
private global::System.EventHandler? _eventField;



event global::System.EventHandler? global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override_NotInlineable.IInterface.EventField
{add    {
        global::System.Console.WriteLine("This is overridden method.");
this.EventField_Source+= value;
this.EventField_Source+= value;
        return;
    }

remove    {
        global::System.Console.WriteLine("This is overridden method.");
this.EventField_Source-= value;
this.EventField_Source-= value;
        return;
    }
}
private event global::System.EventHandler? EventField_Source
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