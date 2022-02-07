using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce(WhenExists = OverrideStrategy.New)]
        public int BaseClassMethod()
        {
            meta.InsertComment("Should call the base class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public static int BaseClassMethod_Static()
        {
            meta.InsertComment("Should call the base class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int BaseClassInaccessibleMethod()
        {
            meta.InsertComment("Should return a default value.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int BaseClassVirtualMethod()
        {
            meta.InsertComment("Should call the base class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int BaseClassVirtualSealedMethod()
        {
            meta.InsertComment("Should call the base class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int BaseClassVirtualOverridenMethod()
        {
            meta.InsertComment("Should call the base class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int BaseClassAbstractMethod()
        {
            meta.InsertComment("Should call the base class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int BaseClassAbstractSealedMethod()
        {
            meta.InsertComment("Should call the base class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int HiddenBaseClassMethod()
        {
            meta.InsertComment("Should call the derived class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public static int HiddenBaseClassMethod_Static()
        {
            meta.InsertComment("Should call the derived class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int HiddenBaseClassVirtualMethod()
        {
            meta.InsertComment("Should call the derived class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int HiddenVirtualBaseClassVirtualMethod()
        {
            meta.InsertComment("Should call the derived class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int BaseClassHiddenByInaccessibleMethod()
        {
            meta.InsertComment("Should call the base class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int DerivedClassMethod()
        {
            meta.InsertComment("Should call the derived class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public static int DerivedClassMethod_Static()
        {
            meta.InsertComment("Should call the derived class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int DerivedClassVirtualMethod()
        {
            meta.InsertComment("Should call the derived class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int DerivedClassVirtualSealedMethod()
        {
            meta.InsertComment("Should call the derived class method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int DerivedClassInaccessibleMethod()
        {
            meta.InsertComment("Should return a default value.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int ExistingMethod()
        {
            meta.InsertComment("Should return a constant.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public static int ExistingMethod_Static()
        {
            meta.InsertComment("Should return a constant.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int ExistingVirtualMethod()
        {
            meta.InsertComment("Should return a constant.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public int NonExistentMethod()
        {
            meta.InsertComment("Should return a default value.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.New)]
        public static int NonExistentMethod_Static()
        {
            meta.InsertComment("Should return a default value.");
            return meta.Proceed();
        }
    }

    internal abstract class BaseClass
    {
        public int BaseClassMethod()
        {
            return 42;
        }

        public static int BaseClassMethod_Static()
        {
            return 42;
        }

        public int HiddenBaseClassMethod()
        {
            return 42;
        }

        public static int HiddenBaseClassMethod_Static()
        {
            return 42;
        }

        public int HiddenBaseClassVirtualMethod()
        {
            return 42;
        }

        public int HiddenVirtualBaseClassVirtualMethod()
        {
            return 42;
        }

        private int BaseClassInaccessibleMethod()
        {
            return 42;
        }

        public int BaseClassHiddenByInaccessibleMethod()
        {
            return 42;
        }

        public virtual int BaseClassVirtualMethod()
        {
            return 42;
        }

        public virtual int BaseClassVirtualSealedMethod()
        {
            return 42;
        }

        public virtual int BaseClassVirtualOverridenMethod()
        {
            return 42;
        }

        public abstract int BaseClassAbstractMethod();

        public abstract int BaseClassAbstractSealedMethod();
    }

    internal class DerivedClass : BaseClass
    {
        public new int HiddenBaseClassMethod()
        {
            return 33;
        }

        public new static int HiddenBaseClassMethod_Static()
        {
            return 33;
        }

        public new int HiddenBaseClassVirtualMethod()
        {
            return 33;
        }

        public new virtual int HiddenVirtualBaseClassVirtualMethod()
        {
            return 33;
        }

        private new int BaseClassHiddenByInaccessibleMethod()
        {
            return 42;
        }

        public sealed override int BaseClassVirtualSealedMethod()
        {
            return 33;
        }

        public override int BaseClassVirtualOverridenMethod()
        {
            return 33;
        }

        public override int BaseClassAbstractMethod()
        {
            return 33;
        }

        public sealed override int BaseClassAbstractSealedMethod()
        {
            return 33;
        }

        public int DerivedClassMethod()
        {
            return 33;
        }

        public static int DerivedClassMethod_Static()
        {
            return 33;
        }

        public virtual int DerivedClassVirtualMethod()
        {
            return 33;
        }

        public virtual int DerivedClassVirtualSealedMethod()
        {
            return 33;
        }

        private int DerivedClassInaccessibleMethod()
        {
            return 42;
        }
    }

    // <target>
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
    }
}