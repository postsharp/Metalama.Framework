class Target
    {
        public int Foo
{get
{
    return this.Foo_Override6;
}set
{
    this.Foo_Override6 = value;
}}


public int Bar
{get
{
    return this.Bar_Override5;
}set
{
    this.Bar_Override5 = value;
}}
private int Bar_Empty
{
    get
    {
        Console.WriteLine("This is introduced code (discarded).");
        return 0;
    }

    set
    {
        Console.WriteLine("This is introduced code (discarded).");
    }
}

public int Foo_Override0
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar_Empty;
        // Should invoke empty code.
        _ = this.Bar_Empty;
        // Should invoke empty code.
        _ = this.Bar_Empty;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    
    }

    set
    {
            // Should invoke empty code.
        this.Bar_Empty= value;
        // Should invoke empty code.
        this.Bar_Empty= value;
        // Should invoke empty code.
        this.Bar_Empty= value;
        // Should invoke the final declaration.
        this.Bar= value;
        }
}

public int Foo_Override2
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar_Empty;
        // Should invoke override 1.
        _ = this.Bar_Override1;
        // Should invoke override 1.
        _ = this.Bar_Override1;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    
    }

    set
    {
            // Should invoke empty code.
        this.Bar_Empty= value;
        // Should invoke override 1.
        this.Bar_Override1= value;
        // Should invoke override 1.
        this.Bar_Override1= value;
        // Should invoke the final declaration.
        this.Bar= value;
        }
}

public int Foo_Override4
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar_Empty;
        // Should invoke override 3.
        _ = this.Bar_Override3;
        // Should invoke override 3.
        _ = this.Bar_Override3;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    
    }

    set
    {
            // Should invoke empty code.
        this.Bar_Empty= value;
        // Should invoke override 3.
        this.Bar_Override3= value;
        // Should invoke override 3.
        this.Bar_Override3= value;
        // Should invoke the final declaration.
        this.Bar= value;
        }
}

public int Foo_Override6
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar_Empty;
        // Should invoke the final declaration.
        _ = this.Bar;
        // Should invoke the final declaration.
        _ = this.Bar;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    
    }

    set
    {
            // Should invoke empty code.
        this.Bar_Empty= value;
        // Should invoke the final declaration.
        this.Bar= value;
        // Should invoke the final declaration.
        this.Bar= value;
        // Should invoke the final declaration.
        this.Bar= value;
        }
}

private int Bar_Override1
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar_Empty;
        // Should invoke empty code.
        _ = this.Bar_Empty;
        // Should invoke override 1.
        _ = this.Bar_Override1;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    
    }

    set
    {
            // Should invoke empty code.
        this.Bar_Empty= value;
        // Should invoke empty code.
        this.Bar_Empty= value;
        // Should invoke override 1.
        this.Bar_Override1= value;
        // Should invoke the final declaration.
        this.Bar= value;
        }
}

private int Bar_Override3
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar_Empty;
        // Should invoke override 1.
        _ = this.Bar_Override1;
        // Should invoke override 3.
        _ = this.Bar_Override3;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    
    }

    set
    {
            // Should invoke empty code.
        this.Bar_Empty= value;
        // Should invoke override 1.
        this.Bar_Override1= value;
        // Should invoke override 3.
        this.Bar_Override3= value;
        // Should invoke the final declaration.
        this.Bar= value;
        }
}

private int Bar_Override5
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar_Empty;
        // Should invoke override 3.
        _ = this.Bar_Override3;
        // Should invoke the final declaration.
        _ = this.Bar;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    
    }

    set
    {
            // Should invoke empty code.
        this.Bar_Empty= value;
        // Should invoke override 3.
        this.Bar_Override3= value;
        // Should invoke the final declaration.
        this.Bar= value;
        // Should invoke the final declaration.
        this.Bar= value;
        }
}    }