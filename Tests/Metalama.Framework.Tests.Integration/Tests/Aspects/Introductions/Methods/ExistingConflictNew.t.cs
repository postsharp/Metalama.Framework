    [Introduction]
        internal class TargetClass : DerivedClass
        {
            public int ExistingMethod()
    {
                return 27;
    }
    
            public static int ExistingMethod_Static()
    {
                return 27;
    }
    
            public virtual int ExistingVirtualMethod()
    {
                return 27;
    }
    
    
    public new global::System.Int32 BaseClassAbstractMethod()
    {
        // Should call the base class method.
    
        return base.BaseClassAbstractMethod();
    }
    
    public new global::System.Int32 BaseClassAbstractSealedMethod()
    {
        // Should call the base class method.
    
        return base.BaseClassAbstractSealedMethod();
    }
    
    public new global::System.Int32 BaseClassHiddenByInaccessibleMethod()
    {
        // Should call the base class method.
    
        return base.BaseClassHiddenByInaccessibleMethod();
    }
    
    public global::System.Int32 BaseClassInaccessibleMethod()
    {
        return default(global::System.Int32);
    }
    
    public new global::System.Int32 BaseClassMethod()
    {
        // Should call the base class method.
    
        return base.BaseClassMethod();
    }
    
    public static new global::System.Int32 BaseClassMethod_Static()
    {
        // Should call the base class method.
    
        return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew.BaseClass.BaseClassMethod_Static();
    }
    
    public new global::System.Int32 BaseClassVirtualMethod()
    {
        // Should call the base class method.
    
        return base.BaseClassVirtualMethod();
    }
    
    public new global::System.Int32 BaseClassVirtualOverridenMethod()
    {
        // Should call the base class method.
    
        return base.BaseClassVirtualOverridenMethod();
    }
    
    public new global::System.Int32 BaseClassVirtualSealedMethod()
    {
        // Should call the base class method.
    
        return base.BaseClassVirtualSealedMethod();
    }
    
    public global::System.Int32 DerivedClassInaccessibleMethod()
    {
        return default(global::System.Int32);
    }
    
    public new global::System.Int32 DerivedClassMethod()
    {
        // Should call the derived class method.
    
        return base.DerivedClassMethod();
    }
    
    public static new global::System.Int32 DerivedClassMethod_Static()
    {
        // Should call the derived class method.
    
        return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew.DerivedClass.DerivedClassMethod_Static();
    }
    
    public new global::System.Int32 DerivedClassVirtualMethod()
    {
        // Should call the derived class method.
    
        return base.DerivedClassVirtualMethod();
    }
    
    public new global::System.Int32 DerivedClassVirtualSealedMethod()
    {
        // Should call the derived class method.
    
        return base.DerivedClassVirtualSealedMethod();
    }
    
    public new global::System.Int32 HiddenBaseClassMethod()
    {
        // Should call the derived class method.
    
        return base.HiddenBaseClassMethod();
    }
    
    public static new global::System.Int32 HiddenBaseClassMethod_Static()
    {
        // Should call the derived class method.
    
        return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew.DerivedClass.HiddenBaseClassMethod_Static();
    }
    
    public new global::System.Int32 HiddenBaseClassVirtualMethod()
    {
        // Should call the derived class method.
    
        return base.HiddenBaseClassVirtualMethod();
    }
    
    public new global::System.Int32 HiddenVirtualBaseClassVirtualMethod()
    {
        // Should call the derived class method.
    
        return base.HiddenVirtualBaseClassVirtualMethod();
    }
    
    public global::System.Int32 NonExistentMethod()
    {
        return default(global::System.Int32);
    }
    
    public static global::System.Int32 NonExistentMethod_Static()
    {
        return default(global::System.Int32);
    }    }