internal class TargetClass
{
    [Override]
    public int Property
    {
        init
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    [Override]
    private int PrivateProperty
    {
        init
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    [Override]
    public int ExpressionProperty
    {
        init
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    [Override]
    private int PrivateExpressionProperty
    {
        init
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }
}