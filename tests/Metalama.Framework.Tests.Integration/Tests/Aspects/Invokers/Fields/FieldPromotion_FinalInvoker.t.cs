[Promote]
[Test]
internal class TargetClass
{
    private global::System.Int32 _field;

    public global::System.Int32 Field
    {
        get
        {
            global::System.Console.WriteLine("This is aspect code.");
            return this._field;
        }

        set
        {
            global::System.Console.WriteLine("This is aspect code.");
            this._field = value;
        }
    }
}
