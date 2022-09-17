[Introduction]
    [Test]
    internal class TargetClass : BaseClass
    {
        public int TargetClassProperty
        {
            get 
{ 
            global::System.Console.WriteLine("Override");
        return this.TargetClassProperty_Introduction;
    
}
        }

private int TargetClassProperty_Source
        {
            get => 42;
        }


private global::System.Int32 TargetClassProperty_Introduction
{
    get
    {
            global::System.Console.WriteLine("This is introduced property.");
        return this.TargetClassProperty_Source;
        }
}

public new global::System.Int32 BaseClassProperty
{
    get
    {
                global::System.Console.WriteLine("Override");
        return this.BaseClassProperty_Introduction;
    
    }
}

private global::System.Int32 BaseClassProperty_Introduction
{
    get
    {
            global::System.Console.WriteLine("This is introduced property.");
        return base.BaseClassProperty;
        }
}    }