[Introduction]
[Test]
internal class TargetClass : BaseClass
{
    public int TargetClassProperty
    {
        get
        {
            global::System.Console.WriteLine("This is introduced property.");
            return this.TargetClassProperty_Source;
        }
    }

    private int TargetClassProperty_Source
    {
        get => 42;
    }

    public new global::System.Int32 BaseClassProperty
    {
        get
        {
            global::System.Console.WriteLine("This is introduced property.");
            return base.BaseClassProperty;
        }
    }
}