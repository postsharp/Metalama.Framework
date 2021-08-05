class Target
    {
        delegate void Handler();
        void Test(string s)
        {
        }

private Handler? _foo;


        event Handler? Foo{add    {
        this.Foo_Source+= value;
    }

remove    {
        this.Foo_Source-= value;
    }
}

private event Handler? Foo_Source
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

    }