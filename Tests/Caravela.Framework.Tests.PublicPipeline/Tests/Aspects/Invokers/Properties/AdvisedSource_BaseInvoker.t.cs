internal class TargetClass
    {


private int _property;
        [Test]
        public int Property {get    {
        return this.Property_Source;
    }

set    {
        this.Property_Source= value;
    }
}

private int Property_Source
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