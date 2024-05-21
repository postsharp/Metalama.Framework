using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Signatures
{
    /*
     * Simple case with implicit interface members.
     */

    public interface IInterface
    {
        void VoidMethod();

        int Method( int x, string y );

        int Method_Ref( ref int x );

        //ref int Method_RefReturn(ref int x);

        T? GenericMethod<T>( T? x );

        T? GenericMethod_Multiple<T, U>( T? x, U? y );

        T? GenericMethod_MultipleReverse<T, U>( U? x, T? y );

        T? GenericMethod_NestedParam<T>( List<T> x );

        T? GenericMethod_DoubleNestedParam<T>( List<List<T>> x );

        T? GenericMethod_Ref<T>( ref T? x );

        void GenericMethod_Out<T>( out T? x );
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IInterface) );
        }

        [Introduce]
        public void VoidMethod() { }

        [Introduce]
        public int Method( int x, string y )
        {
            return x;
        }

        [Introduce]
        public int Method_Ref( ref int x )
        {
            return x;
        }

        [Introduce]
        public T? GenericMethod<T>( T? x )
        {
            return x;
        }

        [Introduce]
        public T? GenericMethod_Multiple<T, U>( T? x, U? y )
        {
            return x;
        }

        [Introduce]
        public T? GenericMethod_MultipleReverse<T, U>( U? x, T? y )
        {
            return y;
        }

        [Introduce]
        public T? GenericMethod_NestedParam<T>( List<T> x )
        {
            if (x.Count > 0)
            {
                return x[0];
            }
            else
            {
                return default;
            }
        }

        [Introduce]
        public T? GenericMethod_DoubleNestedParam<T>( List<List<T>> x )
        {
            if (x.Count > 0)
            {
                if (x[0].Count > 0)
                {
                    return x[0][0];
                }
                else
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }

        [Introduce]
        public T? GenericMethod_Ref<T>( ref T? x )
        {
            return x;
        }

        [Introduce]
        public void GenericMethod_Out<T>( out T? x )
        {
            x = default;
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}