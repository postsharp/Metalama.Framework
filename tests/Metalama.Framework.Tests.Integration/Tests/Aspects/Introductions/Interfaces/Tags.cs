using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Tags
{
    /*
     * Tests that tags are not passed to interface members implemented using [Introduce].
     */

    public interface IInterface1
    {
        int InterfaceMethod1();

        event EventHandler Event1;

        int Property1 { get; set; }
    }

    public interface IInterface2
    {
        int InterfaceMethod2();

        event EventHandler Event2;

        int Property2 { get; set; }
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IInterface1), tags: new { TestTag = "TestValue_For_Interface1" } );
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IInterface2), tags: new { TestTag = "TestValue_For_Interface2" } );
        }

        [Introduce]
        public int InterfaceMethod1()
        {
            Console.WriteLine( $"This introduced member has Tag? {meta.Tags.ContainsKey("TestTag")}." );

            return meta.Proceed();
        }

        [Introduce]
        public event EventHandler? Event1
        {
            add
            {
                Console.WriteLine( $"This introduced member has Tag? {meta.Tags.ContainsKey("TestTag")}." );
            }

            remove
            {
                Console.WriteLine( $"This introduced member has Tag? {meta.Tags.ContainsKey("TestTag")}." );
            }
        }

        [Introduce]
        public int Property1
        {
            get
            {
                Console.WriteLine( $"This introduced member has Tag? {meta.Tags.ContainsKey("TestTag")}." );

                return 42;
            }

            set
            {
                Console.WriteLine( $"This introduced member has Tag? {meta.Tags.ContainsKey("TestTag")}." );
            }
        }

        [Introduce]
        public int InterfaceMethod2()
        {
            Console.WriteLine( $"This introduced member has Tag? {meta.Tags.ContainsKey("TestTag")}." );

            return meta.Proceed();
        }

        [Introduce]
        public event EventHandler? Event2
        {
            add
            {
                Console.WriteLine( $"This introduced member has Tag? {meta.Tags.ContainsKey("TestTag")}." );
            }

            remove
            {
                Console.WriteLine( $"This introduced member has Tag? {meta.Tags.ContainsKey("TestTag")}." );
            }
        }

        [Introduce]
        public int Property2
        {
            get
            {
                Console.WriteLine( $"This introduced member has Tag? {meta.Tags.ContainsKey("TestTag")}." );

                return 42;
            }

            set
            {
                Console.WriteLine( $"This introduced member has Tag? {meta.Tags.ContainsKey("TestTag")}." );
            }
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}