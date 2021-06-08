[TestOutput]
internal class TargetClass
{
    private int _field;

    [Override]
    public int Property
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._field;
        }

        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::System.Int32 discard;
            this._field = value;
        }
    }
}