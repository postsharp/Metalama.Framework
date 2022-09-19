[Introduction]
    internal class TargetClass { 

[global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.CopyAttributeToIntroducedProperty.FooAttribute]
public global::System.Int32 IntroducedProperty_Accessors
{
    [return: global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.CopyAttributeToIntroducedProperty.FooAttribute]
    get
    {
                global::System.Console.WriteLine("Get");
        return (global::System.Int32)42;
    


    }

    [return: global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.CopyAttributeToIntroducedProperty.FooAttribute]
    [param: global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.CopyAttributeToIntroducedProperty.FooAttribute]
    set
    {
                global::System.Console.WriteLine(value);
    
    }
}

[global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.CopyAttributeToIntroducedProperty.FooAttribute]
public global::System.Int32 IntroducedProperty_Auto { get; set; }

[global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.CopyAttributeToIntroducedProperty.FooAttribute]
public global::System.Int32 IntroducedProperty_Auto_Initializer { get; set; } = (global::System.Int32)42;}