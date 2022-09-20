class Target
{
    public event EventHandler Foo
    {
        add
        {
            this.Foo_Override7 += value;
        }
        remove
        {
            this.Foo_Override7 -= value;
        }
    }


    public event EventHandler? Bar
    {
        add
        {
            this.Bar_Override6 += value;
        }
        remove
        {
            this.Bar_Override6 -= value;
        }
    }
    private EventHandler? Bar_Source;

    private event EventHandler? Bar_Empty
    {
        add
        {
        }

        remove
        {
        }
    }

    public event EventHandler Foo_Override0
    {
        add
        {
            // Should invoke empty code.
            this.Bar_Empty += value;
            // Should invoke empty code.
            this.Bar_Empty += value;
            // Should invoke empty code.
            this.Bar_Empty += value;
            // Should invoke the final declaration.
            this.Bar += value;

        }

        remove
        {
            // Should invoke empty code.
            this.Bar_Empty -= value;
            // Should invoke empty code.
            this.Bar_Empty -= value;
            // Should invoke empty code.
            this.Bar_Empty -= value;
            // Should invoke the final declaration.
            this.Bar -= value;
        }
    }

    public event EventHandler Foo_Override3
    {
        add
        {
            // Should invoke empty code.
            this.Bar_Empty += value;
            // Should invoke override 2.
            this.Bar_Override2 += value;
            // Should invoke override 2.
            this.Bar_Override2 += value;
            // Should invoke the final declaration.
            this.Bar += value;

        }

        remove
        {
            // Should invoke empty code.
            this.Bar_Empty -= value;
            // Should invoke override 2.
            this.Bar_Override2 -= value;
            // Should invoke override 2.
            this.Bar_Override2 -= value;
            // Should invoke the final declaration.
            this.Bar -= value;
        }
    }

    public event EventHandler Foo_Override5
    {
        add
        {
            // Should invoke empty code.
            this.Bar_Empty += value;
            // Should invoke override 4.
            this.Bar_Override4 += value;
            // Should invoke override 4.
            this.Bar_Override4 += value;
            // Should invoke the final declaration.
            this.Bar += value;

        }

        remove
        {
            // Should invoke empty code.
            this.Bar_Empty -= value;
            // Should invoke override 4.
            this.Bar_Override4 -= value;
            // Should invoke override 4.
            this.Bar_Override4 -= value;
            // Should invoke the final declaration.
            this.Bar -= value;
        }
    }

    public event EventHandler Foo_Override7
    {
        add
        {
            // Should invoke empty code.
            this.Bar_Empty += value;
            // Should invoke the final declaration.
            this.Bar += value;
            // Should invoke the final declaration.
            this.Bar += value;
            // Should invoke the final declaration.
            this.Bar += value;

        }

        remove
        {
            // Should invoke empty code.
            this.Bar_Empty -= value;
            // Should invoke the final declaration.
            this.Bar -= value;
            // Should invoke the final declaration.
            this.Bar -= value;
            // Should invoke the final declaration.
            this.Bar -= value;
        }
    }

    private event EventHandler? Bar_Override2
    {
        add
        {
            // Should invoke empty code.
            this.Bar_Empty += value;
            // Should invoke source code.
            this.Bar_Source += value;
            // Should invoke override 2.
            this.Bar_Override2 += value;
            // Should invoke the final declaration.
            this.Bar += value;

        }

        remove
        {
            // Should invoke empty code.
            this.Bar_Empty -= value;
            // Should invoke introduced event field.
            this.Bar_Source -= value;
            // Should invoke override 2.
            this.Bar_Override2 -= value;
            // Should invoke the final declaration.
            this.Bar -= value;
        }
    }

    private event EventHandler? Bar_Override4
    {
        add
        {
            // Should invoke empty code.
            this.Bar_Empty += value;
            // Should invoke override 2.
            this.Bar_Override2 += value;
            // Should invoke override 4.
            this.Bar_Override4 += value;
            // Should invoke the final declaration.
            this.Bar += value;

        }

        remove
        {
            // Should invoke empty code.
            this.Bar_Empty -= value;
            // Should invoke override 2.
            this.Bar_Override2 -= value;
            // Should invoke override 4.
            this.Bar_Override4 -= value;
            // Should invoke the final declaration.
            this.Bar -= value;
        }
    }

    private event EventHandler? Bar_Override6
    {
        add
        {
            // Should invoke empty code.
            this.Bar_Empty += value;
            // Should invoke override 4.
            this.Bar_Override4 += value;
            // Should invoke the final declaration.
            this.Bar += value;
            // Should invoke the final declaration.
            this.Bar += value;

        }

        remove
        {
            // Should invoke empty code.
            this.Bar_Empty -= value;
            // Should invoke override 4.
            this.Bar_Override4 -= value;
            // Should invoke the final declaration.
            this.Bar -= value;
            // Should invoke the final declaration.
            this.Bar -= value;
        }
    }
}
