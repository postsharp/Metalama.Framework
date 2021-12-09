class Target
    {
        event EventHandler? Foo{add    {
        Console.WriteLine("Override2 Start");
        this.Foo_Override1+= value;
        Console.WriteLine("Override2 End");
    }
    
remove    {
        Console.WriteLine("Override2 Start");
        this.Foo_Override1-= value;
        Console.WriteLine("Override2 End");
    }
}
    
    
event EventHandler Foo_Override1
{add    {
        Console.WriteLine("Override1");
    }
    
remove    {
        Console.WriteLine("Override1");
    }
}    }