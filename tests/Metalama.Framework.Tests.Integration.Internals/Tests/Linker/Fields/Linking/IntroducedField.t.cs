// Final Compilation.Emit failed. 
// Warning CS0649 on `Bar`: `Field 'Target.Bar' is never assigned to, and will always have its default value 0`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0229 on `Bar`: `Ambiguity between 'Target.Bar' and 'Target.Bar'`
// Error CS0102 on `Bar`: `The type 'Target' already contains a definition for 'Bar'`
class Target
    {
        public int Foo
{get
{
    return this.Foo_Override9;
}set
{
    this.Foo_Override9 = value;
}}


public int Bar;

public int Foo_Override0
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar;
        // Should invoke empty code.
        _ = this.Bar;
        // Should invoke empty code.
        _ = this.Bar;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    
    }

    set
    {
            // Should invoke empty code.
        this.Bar = value;
        // Should invoke empty code.
        this.Bar = value;
        // Should invoke empty code.
        this.Bar = value;
        // Should invoke the final declaration.
        this.Bar = value;
        }
}

public int Foo_Override2
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar;
        // Should invoke source code.
        _ = this.Bar;
        // Should invoke source code.
        _ = this.Bar;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    
    }

    set
    {
            // Should invoke empty code.
        this.Bar = value;
        // Should invoke source code.
        this.Bar = value;
        // Should invoke source code.
        this.Bar = value;
        // Should invoke the final declaration.
        this.Bar = value;
        }
}

public int Foo_Override5
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar;
        // Should invoke override 4.
        _ = this.Bar;
        // Should invoke override 4.
        _ = this.Bar;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    
    }

    set
    {
            // Should invoke empty code.
        this.Bar = value;
        // Should invoke override 4.
        this.Bar = value;
        // Should invoke override 4.
        this.Bar = value;
        // Should invoke the final declaration.
        this.Bar = value;
        }
}

public int Foo_Override7
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar;
        // Should invoke override 6.
        _ = this.Bar;
        // Should invoke override 6.
        _ = this.Bar;
        // Should invoke the final declaration.
        _ = this.Bar;
        return 42;
    
    }

    set
    {
            // Should invoke empty code.
        this.Bar = value;
        // Should invoke override 6.
        this.Bar = value;
        // Should invoke override 6.
        this.Bar = value;
        // Should invoke the final declaration.
        this.Bar = value;
        }
}

public int Foo_Override9
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar;
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
        this.Bar = value;
        // Should invoke the final declaration.
        this.Bar = value;
        // Should invoke the final declaration.
        this.Bar = value;
        // Should invoke the final declaration.
        this.Bar = value;
        }
}

public int Bar_Override8
{
    get
    {
            // Should invoke empty code.
        _ = this.Bar;
        // Should invoke override 6.
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
        this.Bar = value;
        // Should invoke override 6.
        this.Bar = value;
        // Should invoke the final declaration.
        this.Bar = value;
        // Should invoke the final declaration.
        this.Bar = value;
        }
}

public int Bar {get
{
    return this.Bar_Override8;
}set
{
    this.Bar_Override8 = value;
}}    }