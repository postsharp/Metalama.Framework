class Target
    {        
        EventHandler? field;

        event EventHandler? Foo
{add    {
        this.Foo_Source+= value;
    }

remove    {
        this.Foo_Source-= value;
    }
}

private event EventHandler? Foo_Source
        {
            add
            {
                this.field += value;
            }
            remove
            {
                this.field -= value;
            }
        }
    }