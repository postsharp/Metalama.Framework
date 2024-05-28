﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.InterfaceImplementation_Explicit;
using System.Threading.Tasks;

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(OverrideAttribute), typeof(IntroduceAspectAttribute) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.InterfaceImplementation_Explicit
{
    /*
     * Tests overriding of explicit interface implementation methods.
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
            builder.Advice.ImplementInterface( builder.Target, (INamedType)TypeFactory.GetType( typeof(IntroducedInterface) ) );

            builder.Outbound.SelectMany( x => x.Methods ).AddAspect( x => new OverrideAttribute() );
        }

        [ExplicitInterfaceMember]
        private void IntroducedVoidMethod()
        {
            Console.WriteLine( "Introduced" );
        }

        [ExplicitInterfaceMember]
        private int IntroducedMethod()
        {
            Console.WriteLine( "Introduced" );

            return 42;
        }

        [ExplicitInterfaceMember]
        private T IntroducedGenericMethod<T>( T value )
        {
            Console.WriteLine( "Introduced" );

            return value;
        }

        [ExplicitInterfaceMember]
        private async Task IntroducedAsyncVoidMethod()
        {
            Console.WriteLine( "Introduced" );
            await Task.Yield();
        }

        [ExplicitInterfaceMember]
        private async Task<int> IntroducedAsyncMethod()
        {
            Console.WriteLine( "Introduced" );
            await Task.Yield();

            return 42;
        }

        [ExplicitInterfaceMember]
        private IEnumerable<int> IntroducedIteratorMethod()
        {
            Console.WriteLine( "Introduced" );

            yield return 42;
        }

        [ExplicitInterfaceMember]
        private async IAsyncEnumerable<int> IntroducedAsyncIteratorMethod()
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
        void Interface.VoidMethod()
        {
            Console.WriteLine( "Original" );
        }

        int Interface.Method()
        {
            Console.WriteLine( "Original" );

            return 42;
        }

        T Interface.GenericMethod<T>( T value )
        {
            Console.WriteLine( "Original" );

            return value;
        }

        //async Task Interface.AsyncVoidMethod()
        //{
        //    Console.WriteLine("Original");
        //    await Task.Yield();
        //}

        //async Task<int> Interface.AsyncMethod()
        //{
        //    Console.WriteLine("Original");
        //    await Task.Yield();
        //    return 42;
        //}

        //IEnumerable<int> Interface.IteratorMethod()
        //{
        //    Console.WriteLine("Original");
        //    yield return 42;
        //}

        //async IAsyncEnumerable<int> Interface.AsyncIteratorMethod()
        //{
        //    Console.WriteLine("Original");
        //    await Task.Yield();
        //    yield return 42;
        //}
    }
}