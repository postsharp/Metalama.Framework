using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictOverride_ConditionalFinalInvoker;

[assembly: AspectOrder( typeof(TestAttribute), typeof(TestIntroductionAttribute) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictOverride_ConditionalFinalInvoker
{
    public class TestIntroductionAttribute : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Override )]
        public void BaseClass_VoidMethod()
        {
            meta.InsertComment( "Introduced." );
            Print();
            Console.WriteLine( "Introduced method print." );
        }

        [Introduce( WhenExists = OverrideStrategy.Override )]
        public int? BaseClass_ExistingMethod()
        {
            meta.InsertComment( "Introduced." );
            Print();

            return 100;
        }

        [Introduce( WhenExists = OverrideStrategy.Override )]
        public int? BaseClass_ExistingMethod_Parameterized( int? x )
        {
            meta.InsertComment( "Introduced." );
            Print();

            return x + 100;
        }

        [Introduce( WhenExists = OverrideStrategy.Override )]
        public void Print()
        {
            Console.WriteLine( "Print() called." );
        }
    }

    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var method in builder.Target.Methods)
            {
                builder.Advice.Override( method, nameof(MethodTemplate) );
            }
        }

        [Template]
        public dynamic? MethodTemplate()
        {
            TargetClass? local = null;

            if (meta.Target.Method.Parameters.Count == 0)
            {
                return meta.Target.Method.With( local, InvokerOptions.NullConditional | InvokerOptions.Final ).Invoke();
            }
            else
            {
                return meta.Target.Method.With( local, InvokerOptions.NullConditional | InvokerOptions.Final ).Invoke( meta.Target.Method.Parameters[0].Value );
            }
        }
    }

    internal class BaseClass
    {
        public virtual void BaseClass_VoidMethod()
        {
            Console.WriteLine( "BaseClass_VoidMethod()" );
        }

        public virtual int? BaseClass_ExistingMethod()
        {
            Console.WriteLine( "BaseClass_ExistingMethod()" );

            return 42;
        }

        public virtual int? BaseClass_ExistingMethod_Parameterized( int? x )
        {
            Console.WriteLine( "BaseClass_ExistingMethod_Parameterized()" );

            return x + 42;
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass : BaseClass
    {
        public void VoidMethod()
        {
            Console.WriteLine( "TargetClass_VoidMethod()" );
        }

        public int? ExistingMethod()
        {
            Console.WriteLine( "TargetClass_ExistingMethod()" );

            return 42;
        }

        public int? ExistingMethod_Parameterized( int? x )
        {
            Console.WriteLine( "TargetClass_ExistingMethod_Parameterized" );

            return x + 42;
        }
    }
}