internal class TargetClass
    {


private global::System.Int32 _field;


[global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.Simple.TestAttribute]
public global::System.Int32 Field 
{ get
{ 
        global::System.Console.WriteLine("This is aspect code.");
        return this._field;
}
set
{ 
        global::System.Console.WriteLine("This is aspect code.");
        this._field=value;
}
}

private static global::System.Int32 _staticField;


[global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.Simple.TestAttribute]
public static global::System.Int32 StaticField 
{ get
{ 
        global::System.Console.WriteLine("This is aspect code.");
        return global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.Simple.TargetClass._staticField;
}
set
{ 
        global::System.Console.WriteLine("This is aspect code.");
        global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.Simple.TargetClass._staticField=value;
}
}    }