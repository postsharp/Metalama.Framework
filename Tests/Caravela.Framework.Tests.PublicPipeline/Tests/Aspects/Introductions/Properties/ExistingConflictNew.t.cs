[Introduction]
    internal class TargetClass : DerivedClass
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
    
        public virtual int ExistingVirtualProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
return 27;    }
}
    
    
public new global::System.Int32 BaseClassProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public static new global::System.Int32 BaseClassProperty_Static
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public new global::System.Int32 HiddenBaseClassProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public static new global::System.Int32 HiddenBaseClassProperty_Static
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public new global::System.Int32 HiddenBaseClassVirtualProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public new global::System.Int32 HiddenVirtualBaseClassVirtualProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public new global::System.Int32 BaseClassVirtualProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public new global::System.Int32 BaseClassVirtualSealedProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public new global::System.Int32 BaseClassVirtualOverridenProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public new global::System.Int32 BaseClassAbstractProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public new global::System.Int32 BaseClassAbstractSealedProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public new global::System.Int32 DerivedClassProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public static new global::System.Int32 DerivedClassProperty_Static
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public new global::System.Int32 DerivedClassVirtualProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public new global::System.Int32 DerivedClassVirtualSealedProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public global::System.Int32 NonExistentProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}
    
public static global::System.Int32 NonExistentProperty_Static
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return default(global::System.Int32);
    }
}    }