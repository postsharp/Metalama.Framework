[Introduction]
    internal class TargetClass : BaseClass
    {
        public int ExistingProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
return 27;    }
}
    
        public static int ExistingProperty_Static
{get    {
        global::System.Console.WriteLine("This is introduced property.");
return 27;    }
}
    
    
public global::System.Int32 NonExistingProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public static global::System.Int32 NonExistingProperty_Static
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}    }