[Introduction]
    internal class TargetClass
    {


public global::System.Int32 IntroducedProperty_Auto
{get    {
    return this._introducedProperty_Auto;
    }

set    {
this._introducedProperty_Auto=value;    }
}private global::System.Int32 _introducedProperty_Auto;

public static global::System.Int32 IntroducedProperty_Auto_Static
{get    {
    return _introducedProperty_Auto_Static;
    }

set    {
_introducedProperty_Auto_Static=value;    }
}private static global::System.Int32 _introducedProperty_Auto_Static;

public global::System.Int32 IntroducedProperty_Accessors
{get    {
        global::System.Console.WriteLine("Get");
        return (int)42;
    }

set    {
        global::System.Console.WriteLine(value);
    }
}    }