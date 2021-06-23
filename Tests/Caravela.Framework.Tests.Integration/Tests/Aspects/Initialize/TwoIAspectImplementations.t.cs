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

        [Log]
        public int Property {get    {
    return this.__Property__BackingField;
    }

set    {
        global::System.Console.WriteLine("Assigning Caravela.Framework.Tests.Integration.Aspects.Initialize.TwoIAspectImplementations.TargetCode.Property.set");
        global::System.Int32 _;
this.__Property__BackingField=value;    }
}
private int __Property__BackingField;
        [Log]
        public string? Field {get    {
    return this.__Field__BackingField;
    }

set    {
        global::System.Console.WriteLine("Assigning Caravela.Framework.Tests.Integration.Aspects.Initialize.TwoIAspectImplementations.TargetCode.Field.set");
        global::System.String? _;
this.__Field__BackingField=value;    }
}
private string? __Field__BackingField;    }