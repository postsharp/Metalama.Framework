    [Introduction]
    [Override]
    public class TargetClass:global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ImplicitMembers_Override.IInterface{ 

private global::System.Int32 _autoProperty;


public global::System.Int32 AutoProperty { get
{ 
        global::System.Console.WriteLine("This is overridden method.");
        return this._autoProperty;
}
set
{ 
        global::System.Console.WriteLine("This is overridden method.");
        this._autoProperty=value;        return;

}
}

public global::System.Int32 Property
{
    get
    {
        global::System.Console.WriteLine("This is overridden method.");
                    global::System.Console.WriteLine("This is introduced interface member.");
        return (global::System.Int32)42;
    

    }

    set
    {
        global::System.Console.WriteLine("This is overridden method.");
                    global::System.Console.WriteLine("This is introduced interface member.");
    
        return;
    }
}

public global::System.Int32 InterfaceMethod()
{
    global::System.Console.WriteLine("This is overridden method.");
        global::System.Console.WriteLine("This is introduced interface member.");
        return default(global::System.Int32);
}

public event global::System.EventHandler? Event
{
    add
    {
        global::System.Console.WriteLine("This is overridden method.");
                    global::System.Console.WriteLine("This is introduced interface member.");
    

        return;
    }

    remove
    {
        global::System.Console.WriteLine("This is overridden method.");
                    global::System.Console.WriteLine("This is introduced interface member.");
    
        return;
    }
}
private global::System.EventHandler? _eventField;



public event global::System.EventHandler? EventField
{
add
{
        global::System.Console.WriteLine("This is overridden method.");
        this._eventField+=value;
        return;
}
remove
{
        global::System.Console.WriteLine("This is overridden method.");
        this._eventField-=value;
        return;
}
}
}