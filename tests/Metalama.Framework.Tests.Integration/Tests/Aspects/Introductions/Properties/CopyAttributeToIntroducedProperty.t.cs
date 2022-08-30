[Introduction]
internal class TargetClass
{
    [Override]
    public global::System.Int32 IntroducedProperty_Accessors
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            global::System.Console.WriteLine("Get");
            return (global::System.Int32)42;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Console.WriteLine(value);
        }
    }

    private global::System.Int32 _introducedProperty_Auto;

    [Override]
    public global::System.Int32 IntroducedProperty_Auto
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._introducedProperty_Auto;
        }
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this._introducedProperty_Auto = value;
        }
    }

    private global::System.Int32 _introducedProperty_Auto_GetOnly;

    [Override]
    public global::System.Int32 IntroducedProperty_Auto_GetOnly
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._introducedProperty_Auto_GetOnly;
        }
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this._introducedProperty_Auto_GetOnly = value;
        }
    }

    private global::System.Int32 _introducedProperty_Auto_GetOnly_Initializer = (global::System.Int32)42;

    [Override]
    public global::System.Int32 IntroducedProperty_Auto_GetOnly_Initializer
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._introducedProperty_Auto_GetOnly_Initializer;
        }
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this._introducedProperty_Auto_GetOnly_Initializer = value;
        }
    }

    private global::System.Int32 _introducedProperty_Auto_Initializer = (global::System.Int32)42;
    [Override]
    public global::System.Int32 IntroducedProperty_Auto_Initializer
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._introducedProperty_Auto_Initializer;
        }
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this._introducedProperty_Auto_Initializer = value;
        }
    }

    private static global::System.Int32 _introducedProperty_Auto_Static

    [Override]
    public static global::System.Int32 IntroducedProperty_Auto_Static
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.CopyAttributeToIntroducedProperty.TargetClass._introducedProperty_Auto_Static;
        }
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.CopyAttributeToIntroducedProperty.TargetClass._introducedProperty_Auto_Static = value;
        }
    }
}