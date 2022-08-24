[Introduction]
[Test]
internal class TargetClass : BaseClass
{
    public int TargetClassProperty
    {
        get
        {
            return this.TargetClassProperty_Source;
        }
    }

    private int TargetClassProperty_Source
    {
        get => 42;
    }

    public global::System.Int32 BaseClassProperty
    {
        get
        {
            return base.BaseClassProperty;
        }
    }
}