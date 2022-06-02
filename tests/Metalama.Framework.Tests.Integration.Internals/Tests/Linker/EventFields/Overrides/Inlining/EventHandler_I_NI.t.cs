class Target
    {
    
private EventHandler? _foo;
    
        event EventHandler? Foo{add    {
        Console.WriteLine("Before");
        this.Foo_Source+= value;
        Console.WriteLine("After");
    }
    
remove    {
        Console.WriteLine("Before");
        this.Foo_Source-= value;
        Console.WriteLine("After");
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