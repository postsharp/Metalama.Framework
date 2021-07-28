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
        return (int)base.BaseClassProperty;
    }
}

public static new global::System.Int32 BaseClassProperty_Static
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)global::Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.ExistingConflictNew.BaseClass.BaseClassProperty_Static;
    }
}

public new global::System.Int32 HiddenBaseClassProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)base.HiddenBaseClassProperty;
    }
}

public static new global::System.Int32 HiddenBaseClassProperty_Static
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)global::Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.ExistingConflictNew.DerivedClass.HiddenBaseClassProperty_Static;
    }
}

public new global::System.Int32 HiddenBaseClassVirtualProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)base.HiddenBaseClassVirtualProperty;
    }
}

public new global::System.Int32 HiddenVirtualBaseClassVirtualProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)base.HiddenVirtualBaseClassVirtualProperty;
    }
}

public new global::System.Int32 BaseClassVirtualProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)base.BaseClassVirtualProperty;
    }
}

public new global::System.Int32 BaseClassVirtualSealedProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)base.BaseClassVirtualSealedProperty;
    }
}

public new global::System.Int32 BaseClassVirtualOverridenProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)base.BaseClassVirtualOverridenProperty;
    }
}

public new global::System.Int32 BaseClassAbstractProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)base.BaseClassAbstractProperty;
    }
}

public new global::System.Int32 BaseClassAbstractSealedProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)base.BaseClassAbstractSealedProperty;
    }
}

public new global::System.Int32 DerivedClassProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)base.DerivedClassProperty;
    }
}

public static new global::System.Int32 DerivedClassProperty_Static
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)global::Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.ExistingConflictNew.DerivedClass.DerivedClassProperty_Static;
    }
}

public new global::System.Int32 DerivedClassVirtualProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)base.DerivedClassVirtualProperty;
    }
}

public new global::System.Int32 DerivedClassVirtualSealedProperty
{get    {
        global::System.Console.WriteLine("This is introduced property.");
        return (int)base.DerivedClassVirtualSealedProperty;
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