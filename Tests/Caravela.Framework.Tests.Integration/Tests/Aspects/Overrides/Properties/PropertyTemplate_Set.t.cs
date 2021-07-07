internal class TargetClass
{
    [Override]
    public int Property
    {
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    [Override]
    private int PrivateProperty
    {
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    [Override]
    public static int Static_Property
    {
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    [Override]
    public int ExpressionProperty
    {
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    [Override]
    private int PrivateExpressionProperty
    {
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }

    [Override]
    public int Static_ExpressionProperty
    {
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::System.Int32 discard;
            Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }
}