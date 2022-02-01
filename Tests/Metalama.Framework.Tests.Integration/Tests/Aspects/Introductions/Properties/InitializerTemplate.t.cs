[Introduction]
internal class TargetClass
{

    private global::System.String _introducedProperty = "IntroducedProperty";


    public global::System.String IntroducedProperty
    {
        get
        {
            return this._introducedProperty;
        }

        set
        {
            this._introducedProperty = value;
        }
    }

    private static global::System.String _introducedProperty_Static = "IntroducedProperty_Static";


    public static global::System.String IntroducedProperty_Static
    {
        get
        {
            return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.InitializerTemplate.TargetClass._introducedProperty_Static;
        }

        set
        {
            global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.InitializerTemplate.TargetClass._introducedProperty_Static = value;
        }
    }
}