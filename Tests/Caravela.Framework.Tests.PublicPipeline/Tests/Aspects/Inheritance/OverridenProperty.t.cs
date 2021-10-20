internal class Targets
    {
        private class BaseClass
        {


private int _p;
            [Aspect]
            public virtual int P {get    {
        return (global::System.Int32)(this.P_Source+ 1);
    }

set    {
        this.P_Source= value - 1;
    }
}

private int P_Source
{
    get
    {
        return this._p;
    }

    set
    {
        this._p = value;
    }
}        }

        private class DerivedClass : BaseClass
        {


private int _p;
            public override int P {get    {
        return (global::System.Int32)(this.P_Source+ 1);
    }

set    {
        this.P_Source= value - 1;
    }
}

private int P_Source
{
    get
    {
        return this._p;
    }

    set
    {
        this._p = value;
    }
}        }
    }
