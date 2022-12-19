public class C
{
    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp11.Required_Override_NotInlined.TheAspect]
    public required global::System.Int32 Field
    {
        get
        {
            return this.Field_Source;
        }

        set
        {
            if (value != this.Field_Source)
            {
                global::System.Console.WriteLine("Changed");
                this.Field_Source = value;
            }
        }
    }

    private global::System.Int32 Field_Source { get; set; }

    [TheAspect]
    public required int Property
    {
        get
        {
            return this.Property_Source;
        }

        set
        {
            if (value != this.Property_Source)
            {
                global::System.Console.WriteLine("Changed");
                this.Property_Source = value;
            }
        }
    }

    private int Property_Source { get; set; }
}
