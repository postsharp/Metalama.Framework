internal class TargetClass
    {
        [Override]
        public int Property
{set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine($"This is the original setter, setting {value}.");
    }
}
    
        [Override]
        private int PrivateProperty
{set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
                Console.WriteLine($"This is the original setter, setting {value}.");
    }
}
    
        [Override]
        public int ExpressionProperty
{set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
Console.WriteLine($"This is the original setter, setting {value}.");    }
}
    
        [Override]
        private int PrivateExpressionProperty
{set    {
        global::System.Console.WriteLine($"This is the overridden setter.");
Console.WriteLine($"This is the original setter, setting {value}.");    }
}
    }