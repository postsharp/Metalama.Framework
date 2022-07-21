[Introduction]
internal class TargetClass
{
    private global::System.Int32 _introducedField;

    [global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Fields.CopyAttributeToIntroducedField.OverrideAttribute]
    public global::System.Int32 IntroducedField
    {
        get
        {
            global::System.Console.WriteLine("Overriden.");
            return this._introducedField;
        }
        set
        {
            global::System.Console.WriteLine("Overriden.");
            this._introducedField = value;
        }
    }



    private global::System.Int32 _introducedField_Initialized = 42;

    [global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Fields.CopyAttributeToIntroducedField.OverrideAttribute]
    public global::System.Int32 IntroducedField_Initializer
    {
        get
        {
            global::System.Console.WriteLine("Overriden.");
            return this._introducedField_Static;
        }
        set
        {
            global::System.Console.WriteLine("Overriden.");
            this._introducedField_Static = value;
        }
    }



    private global::System.Int32 _introducedField_Static;

    [global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Fields.CopyAttributeToIntroducedField.OverrideAttribute]
    public static global::System.Int32 IntroducedField_Static
    {
        get
        {
            global::System.Console.WriteLine("Overriden.");
            return this._introducedField_Static;
        }
        set
        {
            global::System.Console.WriteLine("Overriden.");
            this._introducedField_Static = value;
        }
    }

    private static global::System.Int32 _introducedField_Static_Initialized = 42;


    [global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Fields.CopyAttributeToIntroducedField.OverrideAttribute]
    public static global::System.Int32 IntroducedField_Static_Initializer
    {
        get
        {
            global::System.Console.WriteLine("Overriden.");
            return this._introducedField_Static;
        }
        set
        {
            global::System.Console.WriteLine("Overriden.");
            this._introducedField_Static = value;
        }
    }
}
