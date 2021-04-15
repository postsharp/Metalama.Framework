using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

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
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int HiddenBaseClassMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int HiddenBaseClassVirtualMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int HiddenVirtualBaseClassVirtualMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int BaseClassVirtualMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int BaseClassVirtualSealedMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int BaseClassVirtualOverridenMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int BaseClassAbstractMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int BaseClassAbstractSealedMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int DerivedClassMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int DerivedClassVirtualMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int DerivedClassVirtualSealedMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int ExistingMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int ExistingVirtualMethod()
        {
            return 21;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.New)]
        public int NonExistingMethod()
        {
            return 21;
        }
    }

    internal abstract class BaseClass
    {
        public int BaseClassMethod()
        {
            return 42;
        }

        public int HiddenBaseClassMethod()
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

        public virtual int ExistingVirtualMethod()
        {
            return 27;
        }
    }
}
