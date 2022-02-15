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
        this._autoProperty=value;        
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
    

            }

    remove
    {
        global::System.Console.WriteLine("This is overridden method.");
                    global::System.Console.WriteLine("This is introduced interface member.");
    
            }
}
private global::System.EventHandler? _eventField;



public event global::System.EventHandler? EventField
{
add
{
        global::System.Console.WriteLine("This is overridden method.");
        this._eventField+=value;
        }
remove
{
        global::System.Console.WriteLine("This is overridden method.");
        this._eventField-=value;
        }
}
}