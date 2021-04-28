using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNew
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int BaseClassMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public static int BaseClassMethod_Static()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int HiddenBaseClassMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public static int HiddenBaseClassMethod_Static()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int HiddenBaseClassVirtualMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int HiddenVirtualBaseClassVirtualMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int BaseClassVirtualMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int BaseClassVirtualSealedMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int BaseClassVirtualOverridenMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int BaseClassAbstractMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int BaseClassAbstractSealedMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int DerivedClassMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public static int DerivedClassMethod_Static()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int DerivedClassVirtualMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int DerivedClassVirtualSealedMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int ExistingMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public static int ExistingMethod_Static()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int ExistingVirtualMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int NonExistentMethod()
        {
            return proceed();
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public static int NonExistentMethod_Static()
        {
            return proceed();
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
    }

    [TestOutput]
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
