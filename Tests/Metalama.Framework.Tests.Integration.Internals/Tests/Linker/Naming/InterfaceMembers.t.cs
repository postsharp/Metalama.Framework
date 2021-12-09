    class Target : ITest
    {


private int _foo;
        int ITest.Foo {get    {
        return this.Foo_Source;
    }

set    {
this.Foo_Source= value;
    }
}

private int Foo_Source
{
    get
    {
        return this._foo;
    }

    set
    {
        this._foo = value;
    }
}
        int ITest.Bar()
{
    return this.Bar_Source();
}

private int Bar_Source()
        {
            return 42;
        }


        event EventHandler ITest.Quz
{add    {
this.Quz_Source+= value;
    }

remove    {
this.Quz_Source-= value;
    }
}

private event EventHandler Quz_Source
        {
            add { }
            remove { }
        }

    }