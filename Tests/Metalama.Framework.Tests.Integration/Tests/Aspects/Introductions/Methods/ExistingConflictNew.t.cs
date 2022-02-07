[Introduction]
internal class TargetClass : DerivedClass
{
    public int ExistingMethod()
    {
        // Should return a constant.


        return 27;
    }

    public static int ExistingMethod_Static()
    {
        // Should return a constant.


        return 27;
    }

    public virtual int ExistingVirtualMethod()
    {
        // Should return a constant.


        return 27;
    }


    public new global::System.Int32 BaseClassAbstractMethod()
    {
        // Should call the base method of the same name.

        return base.BaseClassAbstractMethod();
    }

    public new global::System.Int32 BaseClassAbstractSealedMethod()
    {
        // Should call the base method of the same name.

        return base.BaseClassAbstractSealedMethod();
    }

    public global::System.Int32 BaseClassInaccessibleMethod()
    {
        // Should return a default value.


        return default(global::System.Int32);
    }

    public global::System.Int32 BaseClassInaccessibleMethod_Static()
    {
        // Should return a default value.


        return default(global::System.Int32);
    }

    public new global::System.Int32 BaseClassMethod()
    {
        // Should call the base method of the same name.

        return base.BaseClassMethod();
    }

    public new global::System.Int32 BaseClassMethodHiddenByInaccessibleMethod()
    {
        // Should call the base method of the same name.

        return base.BaseClassMethodHiddenByInaccessibleMethod();
    }

    public new global::System.Int32 BaseClassMethodHiddenByVirtualMethod()
    {
        // Should call the base method of the same name.

        return base.BaseClassMethodHiddenByVirtualMethod();
    }

    public static new global::System.Int32 BaseClassMethod_Static()
    {
        // Should call the base class method of the same name.

        return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew.BaseClass.BaseClassMethod_Static();
    }

    public new global::System.Int32 BaseClassVirtualMethod()
    {
        // Should call the base method of the same name.

        return base.BaseClassVirtualMethod();
    }

    public new global::System.Int32 BaseClassVirtualOverriddenMethod()
    {
        // Should call the base method of the same name.

        return base.BaseClassVirtualOverriddenMethod();
    }

    public new global::System.Int32 BaseClassVirtualSealedMethod()
    {
        // Should call the base method of the same name.

        return base.BaseClassVirtualSealedMethod();
    }

    public global::System.Int32 DerivedClassInaccessibleMethod()
    {
        // Should return a default value.


        return default(global::System.Int32);
    }

    public global::System.Int32 DerivedClassInaccessibleMethod_Static()
    {
        // Should return a default value.


        return default(global::System.Int32);
    }

    public new global::System.Int32 DerivedClassMethod()
    {
        // Should call the base method of the same name.

        return base.DerivedClassMethod();
    }

    public static new global::System.Int32 DerivedClassMethod_Static()
    {
        // Should call the derived class method of the same name.

        return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew.DerivedClass.DerivedClassMethod_Static();
    }

    public new global::System.Int32 DerivedClassVirtualMethod()
    {
        // Should call the base method of the same name.

        return base.DerivedClassVirtualMethod();
    }

    public new global::System.Int32 DerivedClassVirtualSealedMethod()
    {
        // Should call the base method of the same name.

        return base.DerivedClassVirtualSealedMethod();
    }

    public new global::System.Int32 HiddenBaseClassMethod()
    {
        // Should call the base method of the same name.

        return base.HiddenBaseClassMethod();
    }

    public static new global::System.Int32 HiddenBaseClassMethod_Static()
    {
        // Should call the derived class method of the same name.

        return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew.DerivedClass.HiddenBaseClassMethod_Static();
    }

    public new global::System.Int32 HiddenBaseClassVirtualMethod()
    {
        // Should call the base method of the same name.

        return base.HiddenBaseClassVirtualMethod();
    }

    public global::System.Int32 NonExistentMethod()
    {
        // Should return a default value.


        return default(global::System.Int32);
    }

    public static global::System.Int32 NonExistentMethod_Static()
    {
        // Should return a default value.


        return default(global::System.Int32);
    }
}