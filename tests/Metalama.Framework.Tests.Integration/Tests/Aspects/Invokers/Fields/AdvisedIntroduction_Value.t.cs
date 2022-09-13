[Introduction]
[Override]
internal class TargetClass
{

    public global::System.Int32 Field
    {
        get
        {
            global::System.Console.WriteLine("Override");
            return this.Field_Source;

        }
        set
        {
            global::System.Console.WriteLine("Override");
            this.Field_Source = value;

        }
    }
    private global::System.Int32 Field_Source
    { get; set; }
}