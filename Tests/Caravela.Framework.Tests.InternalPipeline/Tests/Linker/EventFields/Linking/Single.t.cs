class Target
    {
        delegate void Handler();
        void Test(string s)
        {
        }

private Handler? _foo;


        event Handler? Foo{add    {
        this.__Foo__OriginalImpl+= value;
    }

remove    {
        this.__Foo__OriginalImpl-= value;
    }
}

private event Handler? __Foo__OriginalImpl
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