using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Generic.OverrideGenericMethodWithConstraints
{
    internal class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            return meta.Proceed();
        }
    }

    internal class Base
    {
        public virtual void VirtualMethod<T>() { }
    }

    // <target>
    internal class TargetCode : Base
    {
        [Aspect]
        private T MethodWithTypeConstraint<T>( T a )
            where T : IDisposable
        {
            return a;
        }

        [Aspect]
        private T MethodWithConstructorConstraint<T>( T a )
            where T : IDisposable, new()
        {
            return a;
        }

        [Aspect]
        private T MethodWithStructConstraint<T>( T a )
            where T : struct, IDisposable
        {
            return a;
        }

        [Aspect]
        private T MethodWithNotNullConstraint<T>( T a )
            where T : notnull, IDisposable
        {
            return a;
        }

        [Aspect]
        private T MethodWithUnmanagedConstraint<T>( T a )
            where T : unmanaged
        {
            return a;
        }

        [Aspect]
        private T MethodWithClassConstraint<T>( T a )
            where T : class, IDisposable
        {
            return a;
        }

        [Aspect]
        private T MethodWithNullableClassConstraint<T>( T a )
            where T : class?, IDisposable
        {
            return a;
        }

        [Aspect]
        public override void VirtualMethod<T>() where T : default { }
    }
}