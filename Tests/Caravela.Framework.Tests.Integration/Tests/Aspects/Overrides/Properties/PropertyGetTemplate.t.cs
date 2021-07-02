internal class TargetClass
    {
        [Override]
        public int AutoProperty {get    {
        global::System.Console.WriteLine($"This is the overridden setter.");
    return this._autoProperty;
    }

set    {
this._autoProperty=value;    }
}
private int _autoProperty;
        [Override]
        public static int Static_AutoProperty {get    {
        global::System.Console.WriteLine($"This is the overridden setter.");
    return _static_AutoProperty;
    }

set    {
_static_AutoProperty=value;    }
}
private static int _static_AutoProperty;
        [Override]
        public int AutoProperty_Init {get    {
        global::System.Console.WriteLine($"This is the overridden setter.");
    return this._autoProperty_Init;
    }

init    {
this._autoProperty_Init=value;    }
}
private int _autoProperty_Init;
        [Override]
        public int AutoProperty_GetOnly {get    {
        global::System.Console.WriteLine($"This is the overridden setter.");
    return this._autoProperty_GetOnly;
    }
}
private readonly int _autoProperty_GetOnly;
        [Override]
        public int Property
{get    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine("This is the original getter.");
                return 42;
    }

set    {
                Console.WriteLine($"This is the original setter, setting {value}.");
    }
}

        [Override]
        public static int Static_Property
{get    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine("This is the original getter.");
                return 42;
    }

set    {
                Console.WriteLine($"This is the original setter, setting {value}.");
    }
}

        [Override]
        public int InitProperty
{get    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine("This is the original getter.");
                return 42;
    }

init    {
                Console.WriteLine($"This is the original setter, setting {value}.");
    }
}

        [Override]
        public int Property_GetOnly
{get    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine("This is the original getter.");
                return 42;
    }
}

        [Override]
        public int Property_SetOnly
{set    {
                Console.WriteLine($"This is the original setter, setting {value}.");
    }
}

        [Override]
        public int Property_InitOnly
{init    {
                Console.WriteLine($"This is the original setter, setting {value}.");
    }
}
    }