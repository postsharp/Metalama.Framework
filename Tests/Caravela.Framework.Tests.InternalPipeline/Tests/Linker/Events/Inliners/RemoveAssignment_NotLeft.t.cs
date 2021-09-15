// Final Compilation.Emit failed. 
// Error CS0079 on `Foo_Source`: `The event 'Target.Foo_Source' can only appear on the left hand side of += or -=`
public class Target
    {
    
private EventHandler? _foo;
    
        event EventHandler? Foo{add    {
        Console.WriteLine("Before");
        EventHandler? x = null;
        x -= this.Foo_Source;
        Console.WriteLine("After");
    }
    
remove    {
    }
}
    
private event EventHandler? Foo_Source
{
    add
    {
        this._foo += value;
    }
    
    remove
    {
        this._foo -= value;
    }
}    }