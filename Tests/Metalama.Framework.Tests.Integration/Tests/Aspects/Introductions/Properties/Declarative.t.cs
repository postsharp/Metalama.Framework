[Introduction]
    internal class TargetClass
    {


public global::System.Int32 IntroducedProperty_Accessors
{get    {
        global::System.Console.WriteLine("Get");
        return (global::System.Int32)42;
    }

set    {
        global::System.Console.WriteLine(value);
    }
}

private global::System.Int32 _introducedProperty_Auto;


public global::System.Int32 IntroducedProperty_Auto
{get    {
return this._introducedProperty_Auto;    }

set    {
this._introducedProperty_Auto=value;    }
}

private global::System.Int32 _introducedProperty_Auto_GetOnly;


public global::System.Int32 IntroducedProperty_Auto_GetOnly
{get    {
return this._introducedProperty_Auto_GetOnly;    }
}

private global::System.Int32 _introducedProperty_Auto_GetOnly_Initializer = 42;


public global::System.Int32 IntroducedProperty_Auto_GetOnly_Initializer
{get    {
return this._introducedProperty_Auto_GetOnly_Initializer;    }
}

private global::System.Int32 _introducedProperty_Auto_Initializer = 42;


public global::System.Int32 IntroducedProperty_Auto_Initializer
{get    {
return this._introducedProperty_Auto_Initializer;    }

set    {
this._introducedProperty_Auto_Initializer=value;    }
}

private static global::System.Int32 _introducedProperty_Auto_Static;


public static global::System.Int32 IntroducedProperty_Auto_Static
{get    {
return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.Declarative.TargetClass._introducedProperty_Auto_Static;    }

set    {
global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.Declarative.TargetClass._introducedProperty_Auto_Static=value;    }
}    }