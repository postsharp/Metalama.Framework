[Introduction]
[Override]
internal class TargetClass
{

    public global::System.Int32 Field
    {
        get
        {
            global::System.Console.WriteLine("Override");
            return this.Field;

        }
        set
        {
            global::System.Console.WriteLine("Override");
            this.Field = value;

        }
    }
}