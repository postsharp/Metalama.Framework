internal class TargetClass
    {


private int _property;
        [Test]
        public int Property {get    {
        return this.__Property__OriginalImpl;
    }

set    {
        this.__Property__OriginalImpl= value;
    }
}

private int __Property__OriginalImpl
{
    get
    {
        return this._property;
    }

    set
    {
        this._property = value;
    }
}    }