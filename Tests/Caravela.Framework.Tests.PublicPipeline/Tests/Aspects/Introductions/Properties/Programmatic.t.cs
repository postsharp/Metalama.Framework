[Introduction]
    internal class TargetClass
    {
private global::System.Int32 _autoProperty;
    
public global::System.Int32 AutoProperty
{get    {
return this._autoProperty;    }
    
set    {
this._autoProperty=value;    }
}
    
public global::System.Int32 Property
{get    {
        global::System.Console.WriteLine("Get");
        return default(global::System.Int32);
    }
    
set    {
        global::System.Console.WriteLine("Set");
    }
}
    
public global::System.Int32 PropertyFromAccessors
{get    {
        global::System.Console.WriteLine("Get");
        return default(global::System.Int32);
    }
    
set    {
        global::System.Console.WriteLine("Set");
    }
}    }