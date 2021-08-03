class Target
    {        
        EventHandler? field;

        event EventHandler? Foo
{add    {
        this.__Foo__OriginalImpl+= value;
    }

remove    {
        this.__Foo__OriginalImpl-= value;
    }
}

private event EventHandler? __Foo__OriginalImpl
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