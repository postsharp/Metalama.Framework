class Target
    {
    
private EventHandler? _foo;
    
        event EventHandler? Foo{add
{
    this.Foo_Override += value;
}remove
{
    this.Foo_Override -= value;
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
    
event EventHandler? Foo_Override
{add    {
        Console.WriteLine("Before");
        this.Foo_Source+= value;
        Console.WriteLine("After");
    }
    
remove    {
        Console.WriteLine("Before");
        this.Foo_Source-= value;
        Console.WriteLine("After");
    }
}    }