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
        return base.BaseClassAbstractMethod();
    }
    
    public new global::System.Int32 BaseClassAbstractSealedMethod()
    {
        return base.BaseClassAbstractSealedMethod();
    }
    
    public new global::System.Int32 BaseClassMethod()
    {
        return base.BaseClassMethod();
    }
    
    public static new global::System.Int32 BaseClassMethod_Static()
    {
        return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew.BaseClass.BaseClassMethod_Static();
    }
    
    public new global::System.Int32 BaseClassVirtualMethod()
    {
        return base.BaseClassVirtualMethod();
    }
    
    public new global::System.Int32 BaseClassVirtualOverridenMethod()
    {
        return base.BaseClassVirtualOverridenMethod();
    }
    
    public new global::System.Int32 BaseClassVirtualSealedMethod()
    {
        return base.BaseClassVirtualSealedMethod();
    }
    
    public new global::System.Int32 DerivedClassMethod()
    {
        return base.DerivedClassMethod();
    }
    
    public static new global::System.Int32 DerivedClassMethod_Static()
    {
        return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew.DerivedClass.DerivedClassMethod_Static();
    }
    
    public new global::System.Int32 DerivedClassVirtualMethod()
    {
        return base.DerivedClassVirtualMethod();
    }
    
    public new global::System.Int32 DerivedClassVirtualSealedMethod()
    {
        return base.DerivedClassVirtualSealedMethod();
    }
    
    public new global::System.Int32 HiddenBaseClassMethod()
    {
        return base.HiddenBaseClassMethod();
    }
    
    public static new global::System.Int32 HiddenBaseClassMethod_Static()
    {
        return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew.DerivedClass.HiddenBaseClassMethod_Static();
    }
    
    public new global::System.Int32 HiddenBaseClassVirtualMethod()
    {
        return base.HiddenBaseClassVirtualMethod();
    }
    
    public new global::System.Int32 HiddenVirtualBaseClassVirtualMethod()
    {
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