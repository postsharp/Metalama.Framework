internal class TargetClass
    {
        [Override]
        public int AutoProperty {get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
    return this._autoProperty;
    }

set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._autoProperty=value;        return;
    }
}
private int _autoProperty;
        [Override]
        public static int Static_AutoProperty {get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
    return _static_AutoProperty;
    }

set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
_static_AutoProperty=value;        return;
    }
}
private static int _static_AutoProperty;
        [Override]
        public int AutoProperty_Init {get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
    return this._autoProperty_Init;
    }

init    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._autoProperty_Init=value;        return;
    }
}
private int _autoProperty_Init;
        [Override]
        public int AutoProperty_GetOnly {get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
    return this._autoProperty_GetOnly;
    }
}
private readonly int _autoProperty_GetOnly;
        [Override]
        public int Property
{get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
                Console.WriteLine("This is the original getter.");
                return 42;
    }

set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine($"This is the original setter, setting {value}.");
        return;
    }
}

        [Override]
        public static int Static_Property
{get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
                Console.WriteLine("This is the original getter.");
                return 42;
    }

set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine($"This is the original setter, setting {value}.");
        return;
    }
}

        [Override]
        public int InitProperty
{get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
                Console.WriteLine("This is the original getter.");
                return 42;
    }

init    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine($"This is the original setter, setting {value}.");
        return;
    }
}

        [Override]
        public int Property_GetOnly
{get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
                Console.WriteLine("This is the original getter.");
                return 42;
    }
}

        [Override]
        public int Property_SetOnly
{set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine($"This is the original setter, setting {value}.");
        return;
    }
}

        [Override]
        public int Property_InitOnly
{init    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine($"This is the original setter, setting {value}.");
        return;
    }
}
    }