class Target
    {
    
private EventHandler? _foo;
    
        event EventHandler? Foo{add
{
    this.Foo_Override2 += value;
}remove
{
    this.Foo_Override2 -= value;
}}
    
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
}
    
event EventHandler? Foo_Override1
{add    {
        Console.WriteLine("Before1");
        this.Foo_Source+= value;
        Console.WriteLine("After1");
    }
    
remove    {
        Console.WriteLine("Before1");
        this.Foo_Source-= value;
        Console.WriteLine("After1");
    }
}
    
event EventHandler? Foo_Override2
{add    {
        Console.WriteLine("Before2");
        this.Foo_Override1+= value;
        Console.WriteLine("After2");
    }
    
remove    {
        Console.WriteLine("Before2");
        this.Foo_Override1-= value;
        Console.WriteLine("After2");
    }
}    }