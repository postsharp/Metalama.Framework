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
    
    
public new global::System.Int32 BaseClassMethod()
{
    return (int)base.BaseClassMethod();
}
    
public new static global::System.Int32 BaseClassMethod_Static()
{
    return (int)global::Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew.BaseClass.BaseClassMethod_Static();
}
    
public new global::System.Int32 HiddenBaseClassMethod()
{
    return (int)base.HiddenBaseClassMethod();
}
    
public new static global::System.Int32 HiddenBaseClassMethod_Static()
{
    return (int)global::Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew.DerivedClass.HiddenBaseClassMethod_Static();
}
    
public new global::System.Int32 HiddenBaseClassVirtualMethod()
{
    return (int)base.HiddenBaseClassVirtualMethod();
}
    
public new global::System.Int32 HiddenVirtualBaseClassVirtualMethod()
{
    return (int)base.HiddenVirtualBaseClassVirtualMethod();
}
    
public new global::System.Int32 BaseClassVirtualMethod()
{
    return (int)base.BaseClassVirtualMethod();
}
    
public new global::System.Int32 BaseClassVirtualSealedMethod()
{
    return (int)base.BaseClassVirtualSealedMethod();
}
    
public new global::System.Int32 BaseClassVirtualOverridenMethod()
{
    return (int)base.BaseClassVirtualOverridenMethod();
}
    
public new global::System.Int32 BaseClassAbstractMethod()
{
    return (int)base.BaseClassAbstractMethod();
}
    
public new global::System.Int32 BaseClassAbstractSealedMethod()
{
    return (int)base.BaseClassAbstractSealedMethod();
}
    
public new global::System.Int32 DerivedClassMethod()
{
    return (int)base.DerivedClassMethod();
}
    
public new static global::System.Int32 DerivedClassMethod_Static()
{
    return (int)global::Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew.DerivedClass.DerivedClassMethod_Static();
}
    
public new global::System.Int32 DerivedClassVirtualMethod()
{
    return (int)base.DerivedClassVirtualMethod();
}
    
public new global::System.Int32 DerivedClassVirtualSealedMethod()
{
    return (int)base.DerivedClassVirtualSealedMethod();
}
    
public global::System.Int32 NonExistentMethod()
{
    return default(global::System.Int32);
}
    
public static global::System.Int32 NonExistentMethod_Static()
{
    return default(global::System.Int32);
}    }