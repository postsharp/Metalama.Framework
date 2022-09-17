[Introduction]
[Test]
internal class TargetClass : BaseClass
{
    public int TargetClassProperty
    {
        get
        {
            global::System.Console.WriteLine("Override");
            return this.TargetClassProperty;

        }
    }


    public new global::System.Int32 BaseClassProperty
    {
        get
        {
            global::System.Console.WriteLine("Override");
            return this.BaseClassProperty;
        }
    }
}