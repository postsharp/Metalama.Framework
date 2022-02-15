    [Introduction]
    [Override]
    public class TargetClass:global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ImplicitMembers_Override_NotInlineable.IInterface{ 

private global::System.Int32 _autoProperty;


public global::System.Int32 AutoProperty { get
{ 
        global::System.Console.WriteLine("This is overridden method.");
        _ = this.AutoProperty_Source;
        return this.AutoProperty_Source;

}
set
{ 
        global::System.Console.WriteLine("This is overridden method.");
        this.AutoProperty_Source = value;
        this.AutoProperty_Source = value;
        
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

public global::System.Int32 Property_Introduction
{
    get
    {
            global::System.Console.WriteLine("This is introduced interface member.");
        return (global::System.Int32)42;
    
    }

    set
    {
            global::System.Console.WriteLine("This is introduced interface member.");
        }
}

public global::System.Int32 Property
{
    get
    {
        global::System.Console.WriteLine("This is overridden method.");
        _ = this.Property_Introduction;
        return this.Property_Introduction;
    }

    set
    {
        global::System.Console.WriteLine("This is overridden method.");
        this.Property_Introduction = value;
        this.Property_Introduction = value;
            }
}

public global::System.Int32 InterfaceMethod_Introduction()
{
    global::System.Console.WriteLine("This is introduced interface member.");
        return default(global::System.Int32);
}

public global::System.Int32 InterfaceMethod()
{
    global::System.Console.WriteLine("This is overridden method.");
    _ = this.InterfaceMethod_Introduction();
    return this.InterfaceMethod_Introduction();
}

public event global::System.EventHandler? Event_Introduction
{
    add
    {
            global::System.Console.WriteLine("This is introduced interface member.");
    
    }

    remove
    {
            global::System.Console.WriteLine("This is introduced interface member.");
        }
}

public event global::System.EventHandler? Event
{
    add
    {
        global::System.Console.WriteLine("This is overridden method.");
        this.Event_Introduction += value;
        this.Event_Introduction += value;
            }

    remove
    {
        global::System.Console.WriteLine("This is overridden method.");
        this.Event_Introduction -= value;
        this.Event_Introduction -= value;
            }
}
private global::System.EventHandler? _eventField;



public event global::System.EventHandler? EventField
{
add
{
        global::System.Console.WriteLine("This is overridden method.");
        this.EventField_Source += value;
        this.EventField_Source += value;
        }
remove
{
        global::System.Console.WriteLine("This is overridden method.");
        this.EventField_Source -= value;
        this.EventField_Source -= value;
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
}}