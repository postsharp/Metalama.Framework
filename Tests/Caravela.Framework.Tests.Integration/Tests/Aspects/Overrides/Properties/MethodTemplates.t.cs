internal class TargetClass
    {
private int _autoProperty;        [Override]
        public int AutoProperty {get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
return this._autoProperty;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._autoProperty=value;    }
}
private static int _static_AutoProperty;
        [Override]
        public static int Static_AutoProperty {get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
return global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.MethodTemplates.TargetClass._static_AutoProperty;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
global::Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.MethodTemplates.TargetClass._static_AutoProperty=value;    }
}
private int _autoProperty_Init;
        [Override]
        public int AutoProperty_Init {get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
return this._autoProperty_Init;    }
    
set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
this._autoProperty_Init=value;    }
}
private int _autoProperty_GetOnly;
        [Override]
        public int AutoProperty_GetOnly {get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
return this._autoProperty_GetOnly;    }
}
    
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
    }
}
    
        [Override]
        public int InitProperty
{get    {
        global::System.Console.WriteLine($"This is the overridden getter.");
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