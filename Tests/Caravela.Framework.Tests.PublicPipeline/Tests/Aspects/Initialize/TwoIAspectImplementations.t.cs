class TargetCode 
    {
        [Log]
        public int Method(int a, int b)
{
    global::System.Console.WriteLine("Entering Caravela.Framework.Tests.Integration.Aspects.Initialize.TwoIAspectImplementations.TargetCode.Method(int, int)");
    try
    {
            return a + b;
    }
    finally
    {
        global::System.Console.WriteLine("Leaving Caravela.Framework.Tests.Integration.Aspects.Initialize.TwoIAspectImplementations.TargetCode.Method(int, int)");
    }
}

private int _property;
        [Log]
        public int Property {get    {
    return this._property;
    }

set    {
        global::System.Console.WriteLine("Assigning Caravela.Framework.Tests.Integration.Aspects.Initialize.TwoIAspectImplementations.TargetCode.Property.set");
        this._property = value;
    }
}
private string? _field;
        [Log]
        public string? Field {get    {
    return this._field;
    }

set    {
        global::System.Console.WriteLine("Assigning Caravela.Framework.Tests.Integration.Aspects.Initialize.TwoIAspectImplementations.TargetCode.Field.set");
this._field=value;    }
}    }