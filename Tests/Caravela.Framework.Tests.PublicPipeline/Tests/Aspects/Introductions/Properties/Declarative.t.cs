[Introduction]
    internal class TargetClass
    {
private global::System.Int32 _introducedProperty_Auto;
    
public global::System.Int32 IntroducedProperty_Auto
{get    {
return this._introducedProperty_Auto;    }
    
set    {
this._introducedProperty_Auto=value;    }
}private static global::System.Int32 _introducedProperty_Auto_Static;
    
public static global::System.Int32 IntroducedProperty_Auto_Static
{get    {
return global::Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.Declarative.TargetClass._introducedProperty_Auto_Static;    }
    
set    {
global::Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.Declarative.TargetClass._introducedProperty_Auto_Static=value;    }
}
    
public global::System.Int32 IntroducedProperty_Accessors
{get    {
        global::System.Console.WriteLine("Get");
        return (int)42;
    }
    
set    {
        global::System.Console.WriteLine(value);
    }
}    }