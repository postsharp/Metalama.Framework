﻿using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.InterfaceImplementation_Implicit;
using System.Threading.Tasks;

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(OverrideAttribute), typeof(IntroduceAspectAttribute) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.InterfaceImplementation_Implicit
{
    /*
     * Tests overriding of implicit interface implementation methods.
     */

    internal class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( "Override." );

            return meta.Proceed();
        }
    }

    internal class IntroduceAspectAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.ImplementInterface( (INamedType)TypeFactory.GetType( typeof(IntroducedInterface) ) );

            builder.Outbound.SelectMany( x => x.Methods ).AddAspect( x => new OverrideAttribute() );
        }

        [InterfaceMember( IsExplicit = false )]
        public void IntroducedVoidMethod()
        {
            Console.WriteLine( "Introduced" );
        }

        [InterfaceMember( IsExplicit = false )]
        public int IntroducedMethod()
        {
            Console.WriteLine( "Introduced" );

            return 42;
        }

        [InterfaceMember( IsExplicit = false )]
        public T IntroducedGenericMethod<T>( T value )
        {
            Console.WriteLine( "Introduced" );

            return value;
        }

        [InterfaceMember( IsExplicit = false )]
        public async Task IntroducedAsyncVoidMethod()
        {
            Console.WriteLine( "Introduced" );
            await Task.Yield();
        }

        [InterfaceMember( IsExplicit = false )]
        public async Task<int> IntroducedAsyncMethod()
        {
            Console.WriteLine( "Introduced" );
            await Task.Yield();

            return 42;
        }

        [InterfaceMember( IsExplicit = false )]
        public IEnumerable<int> IntroducedIteratorMethod()
        {
            Console.WriteLine( "Introduced" );

            yield return 42;
        }

        [InterfaceMember( IsExplicit = false )]
        public async IAsyncEnumerable<int> IntroducedAsyncIteratorMethod()
        {
            Console.WriteLine( "Introduced" );
            await Task.Yield();

            yield return 42;
        }
    }

    public interface Interface
    {
        void VoidMethod();

        int Method();

        T GenericMethod<T>( T value );

        //Task AsyncVoidMethod();

        //Task<int> AsyncMethod();

        //IEnumerable<int> IteratorMethod();

        //IAsyncEnumerable<int> AsyncIteratorMethod();
    }

    public interface IntroducedInterface
    {
        void IntroducedVoidMethod();

        int IntroducedMethod();

        T IntroducedGenericMethod<T>( T value );

        //Task IntroducedAsyncVoidMethod();

        //Task<int> IntroducedAsyncMethod();

        //IEnumerable<int> IntroducedIteratorMethod();

        //IAsyncEnumerable<int> IntroducedAsyncIteratorMethod();
    }

    // <target>
    [IntroduceAspect]
    public class Target : Interface
    {
        public void VoidMethod()
        {
            Console.WriteLine( "Original" );
        }

        public int Method()
        {
            Console.WriteLine( "Original" );

            return 42;
        }

        public T GenericMethod<T>( T value )
        {
            Console.WriteLine( "Original" );

            return value;
        }

        //public async Task AsyncVoidMethod()
        //{
        //    Console.WriteLine("Original");
        //    await Task.Yield();
        //}

        //public async Task<int> AsyncMethod()
        //{
        //    Console.WriteLine("Original");
        //    await Task.Yield();
        //    return 42;
        //}

        //public IEnumerable<int> IteratorMethod()
        //{
        //    Console.WriteLine("Original");
        //    yield return 42;
        //}

        //public async IAsyncEnumerable<int> AsyncIteratorMethod()
        //{
        //    Console.WriteLine("Original");
        //    await Task.Yield();
        //    yield return 42;
        //}
    }
}