internal class TargetClass
{
    [Override]
    public int AutoProperty
    {
        get
        {
            return this.__AutoProperty__BackingField;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this.__AutoProperty__BackingField = value;
            return;
        }
    }

    private int __AutoProperty__BackingField;
    [Override]
    public static int Static_AutoProperty
    {
        get
        {
            return __Static_AutoProperty__BackingField;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            __Static_AutoProperty__BackingField = value;
            return;
        }
    }

    private static int __Static_AutoProperty__BackingField;
    [Override]
    public int AutoProperty_Init
    {
        get
        {
            return this.__AutoProperty_Init__BackingField;
        }

        init
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this.__AutoProperty_Init__BackingField = value;
            return;
        }
    }

    private int __AutoProperty_Init__BackingField;
    [Override]
    public int AutoProperty_GetOnly
    {
        get
        {
            return this.__AutoProperty_GetOnly__BackingField;
        }
    }

    private readonly int __AutoProperty_GetOnly__BackingField;
    [Override]
    public int Property
    {
        get
        {
            Console.WriteLine("This is the original getter.");
            return 42;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            Console.WriteLine($"This is the original setter, setting {value}.");
            return;
        }
    }

    [Override]
    public static int Static_Property
    {
        get
        {
            Console.WriteLine("This is the original getter.");
            return 42;
        }

        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            Console.WriteLine($"This is the original setter, setting {value}.");
            return;
        }
    }

    [Override]
    public int InitProperty
    {
        get
        {
            Console.WriteLine("This is the original getter.");
            return 42;
        }

        init
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            Console.WriteLine($"This is the original setter, setting {value}.");
            return;
        }
    }

    [Override]
    public int Property_GetOnly
    {
        get
        {
            Console.WriteLine("This is the original getter.");
            return 42;
        }
    }

    [Override]
    public int Property_SetOnly
    {
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            Console.WriteLine($"This is the original setter, setting {value}.");
            return;
        }
    }

    [Override]
    public int Property_InitOnly
    {
        init
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            Console.WriteLine($"This is the original setter, setting {value}.");
            return;
        }
    }
}