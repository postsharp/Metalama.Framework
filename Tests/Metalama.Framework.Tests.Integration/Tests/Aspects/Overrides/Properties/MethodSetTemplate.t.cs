internal class TargetClass
    {
private int _autoProperty;        [Override]
        public int AutoProperty {get    {
return this._autoProperty;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._autoProperty=value;    }
}
private static int _static_AutoProperty;
        [Override]
        public static int Static_AutoProperty {get    {
return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.MethodSetTemplate.TargetClass._static_AutoProperty;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.MethodSetTemplate.TargetClass._static_AutoProperty=value;    }
}
private int _autoProperty_Init;
        [Override]
        public int AutoProperty_Init {get    {
return this._autoProperty_Init;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._autoProperty_Init=value;    }
}
private int _autoProperty_GetOnly;
        [Override]
        public int AutoProperty_GetOnly {get    {
return this._autoProperty_GetOnly;    }
}
    
        [Override]
        public int Property
{get    {
                Console.WriteLine("This is the original getter.");
                return 42;
    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine($"This is the original setter, setting {value}.");
    }
}
    
        [Override]
        public static int Static_Property
{get    {
                Console.WriteLine("This is the original getter.");
                return 42;
    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine($"This is the original setter, setting {value}.");
    }
}
    
        [Override]
        public int InitProperty
{get    {
                Console.WriteLine("This is the original getter.");
                return 42;
    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine($"This is the original setter, setting {value}.");
    }
}
    
        [Override]
        public int Property_GetOnly
{get    {
                Console.WriteLine("This is the original getter.");
                return 42;
    }
}
    
        [Override]
        public int Property_SetOnly
{set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine($"This is the original setter, setting {value}.");
    }
}
    
        [Override]
        public int Property_InitOnly
{set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine($"This is the original setter, setting {value}.");
    }
}
    }