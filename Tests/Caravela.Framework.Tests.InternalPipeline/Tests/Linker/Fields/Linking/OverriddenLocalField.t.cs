class Target
    {
        public int Foo            
{get
{
    return this.Foo_Override7;
}set
{
    this.Foo_Override7 = value;
}}
    
    
public int Foo_Override0
{get    {
        // Should invoke source code.
        _ = this.Bar_Source;
        // Should invoke source code.
        _ = this.Bar_Source;
        // Should invoke source code.
        _ = this.Bar_Source;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    }
    
set    {
        // Should invoke source code.
        this.Bar_Source= value;
        // Should invoke source code.
        this.Bar_Source= value;
        // Should invoke source code.
        this.Bar_Source= value;
        // Should invoke the final declaration.
        this.Bar= value;
    }
}
    
public int Foo_Override3
{get    {
        // Should invoke source code.
        _ = this.Bar_Source;
        // Should invoke override 2.
        _ = this.Bar_Override2;
        // Should invoke override 2.
        _ = this.Bar_Override2;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    }
    
set    {
        // Should invoke source code.
        this.Bar_Source= value;
        // Should invoke override 2.
        this.Bar_Override2= value;
        // Should invoke override 2.
        this.Bar_Override2= value;
        // Should invoke the final declaration.
        this.Bar= value;
    }
}
    
public int Foo_Override5
{get    {
        // Should invoke source code.
        _ = this.Bar_Source;
        // Should invoke override 4.
        _ = this.Bar_Override4;
        // Should invoke override 4.
        _ = this.Bar_Override4;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    }
    
set    {
        // Should invoke source code.
        this.Bar_Source= value;
        // Should invoke override 4.
        this.Bar_Override4= value;
        // Should invoke override 4.
        this.Bar_Override4= value;
        // Should invoke the final declaration.
        this.Bar= value;
    }
}
    
public int Foo_Override7
{get    {
        // Should invoke source code.
        _ = this.Bar_Source;
        // Should invoke the final declaration.
        _ = this.Bar;
        // Should invoke the final declaration.
        _ = this.Bar;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    }
    
set    {
        // Should invoke source code.
        this.Bar_Source= value;
        // Should invoke the final declaration.
        this.Bar= value;
        // Should invoke the final declaration.
        this.Bar= value;
        // Should invoke the final declaration.
        this.Bar= value;
    }
}
    
private int _bar;
    
    
public int Bar
{get
{
    return this.Bar_Override6;
}set
{
    this.Bar_Override6 = value;
}}
private int Bar_Source
{
    get
    {
        return this._bar;
    }
    
    set
    {
        this._bar = value;
    }
}
    
public int Bar_Override2
{get    {
        // Should invoke source code.
        _ = this.Bar_Source;
        // Should invoke source code.
        _ = this.Bar_Source;
        // Should invoke override 2.
        _ = this.Bar_Override2;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    }
    
set    {
        // Should invoke source code.
        this.Bar_Source= value;
        // Should invoke source code.
        this.Bar_Source= value;
        // Should invoke override 2.
        this.Bar_Override2= value;
        // Should invoke the final declaration.
        this.Bar= value;
    }
}
    
public int Bar_Override4
{get    {
        // Should invoke source code.
        _ = this.Bar_Source;
        // Should invoke override 2.
        _ = this.Bar_Override2;
        // Should invoke override 4.
        _ = this.Bar_Override4;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    }
    
set    {
        // Should invoke source code.
        this.Bar_Source= value;
        // Should invoke override 2.
        this.Bar_Override2= value;
        // Should invoke override 4.
        this.Bar_Override4= value;
        // Should invoke the final declaration.
        this.Bar= value;
    }
}
    
public int Bar_Override6
{get    {
        // Should invoke source code.
        _ = this.Bar_Source;
        // Should invoke override 4.
        _ = this.Bar_Override4;
        // Should invoke the final declaration.
        _ = this.Bar;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    }
    
set    {
        // Should invoke source code.
        this.Bar_Source= value;
        // Should invoke override 4.
        this.Bar_Override4= value;
        // Should invoke the final declaration.
        this.Bar= value;
        // Should invoke the final declaration.
        this.Bar= value;
    }
}    }