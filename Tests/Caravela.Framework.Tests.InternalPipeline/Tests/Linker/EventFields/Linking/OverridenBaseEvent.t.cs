class Target : Base
    {
        public event EventHandler Foo
{add
{
    this.Foo += value;
}remove
{
    this.Foo -= value;
}}
    
    
public event EventHandler Foo_Override0
{add    {
        // Should invoke base declaration.
        base.Bar += value;
        // Should invoke base declaration.
        base.Bar += value;
        // Should invoke base declaration.
        base.Bar += value;
        // Should invoke the final declaration.
        this.Bar+= value;
    }
    
remove    {
        // Should invoke base declaration.
        base.Bar -= value;
        // Should invoke base declaration.
        base.Bar -= value;
        // Should invoke base declaration.
        base.Bar -= value;
        // Should invoke the final declaration.
        this.Bar-= value;
    }
}
    
public override event EventHandler Bar
{add
{
    this.Bar += value;
}remove
{
    this.Bar -= value;
}}
    
private event EventHandler Bar_Override1
{add    {
        // Should invoke base declaration.
        base.Bar += value;
        // Should invoke base declaration.
        base.Bar += value;
        // Should invoke override 1.
        this.Bar_Override1+= value;
        // Should invoke the final declaration.
        this.Bar+= value;
    }
    
remove    {
        // Should invoke base declaration.
        base.Bar -= value;
        // Should invoke base declaration.
        base.Bar -= value;
        // Should invoke override 1.
        this.Bar_Override1-= value;
        // Should invoke the final declaration.
        this.Bar-= value;
    }
}
    
private event EventHandler Bar_Override3
{add    {
        // Should invoke base declaration.
        base.Bar += value;
        // Should invoke override 1.
        this.Bar_Override1+= value;
        // Should invoke override 3.
        this.Bar_Override3+= value;
        // Should invoke the final declaration.
        this.Bar+= value;
    }
    
remove    {
        // Should invoke base declaration.
        base.Bar -= value;
        // Should invoke override 1.
        this.Bar_Override1-= value;
        // Should invoke override 3.
        this.Bar_Override3-= value;
        // Should invoke the final declaration.
        this.Bar-= value;
    }
}
    
private event EventHandler Bar_Override5
{add    {
        // Should invoke base declaration.
        base.Bar += value;
        // Should invoke override 3.
        this.Bar_Override3+= value;
        // Should invoke the final declaration.
        this.Bar+= value;
        // Should invoke the final declaration.
        this.Bar+= value;
    }
    
remove    {
        // Should invoke base declaration.
        base.Bar -= value;
        // Should invoke override 3.
        this.Bar_Override3-= value;
        // Should invoke the final declaration.
        this.Bar-= value;
        // Should invoke the final declaration.
        this.Bar-= value;
    }
}
    
public event EventHandler Foo_Override2
{add    {
        // Should invoke base declaration.
        base.Bar += value;
        // Should invoke override 1.
        this.Bar_Override1+= value;
        // Should invoke override 1.
        this.Bar_Override1+= value;
        // Should invoke the final declaration.
        this.Bar+= value;
    }
    
remove    {
        // Should invoke base declaration.
        base.Bar -= value;
        // Should invoke override 1.
        this.Bar_Override1-= value;
        // Should invoke override 1.
        this.Bar_Override1-= value;
        // Should invoke the final declaration.
        this.Bar-= value;
    }
}
    
public event EventHandler Foo_Override4
{add    {
        // Should invoke base declaration.
        base.Bar += value;
        // Should invoke override 3.
        this.Bar_Override3+= value;
        // Should invoke override 3.
        this.Bar_Override3+= value;
        // Should invoke the final declaration.
        this.Bar+= value;
    }
    
remove    {
        // Should invoke base declaration.
        base.Bar -= value;
        // Should invoke override 3.
        this.Bar_Override3-= value;
        // Should invoke override 3.
        this.Bar_Override3-= value;
        // Should invoke the final declaration.
        this.Bar-= value;
    }
}
    
public event EventHandler Foo_Override6
{add    {
        // Should invoke base declaration.
        base.Bar += value;
        // Should invoke the final declaration.
        this.Bar+= value;
        // Should invoke the final declaration.
        this.Bar+= value;
        // Should invoke the final declaration.
        this.Bar+= value;
    }
    
remove    {
        // Should invoke base declaration.
        base.Bar -= value;
        // Should invoke the final declaration.
        this.Bar-= value;
        // Should invoke the final declaration.
        this.Bar-= value;
        // Should invoke the final declaration.
        this.Bar-= value;
    }
}    }