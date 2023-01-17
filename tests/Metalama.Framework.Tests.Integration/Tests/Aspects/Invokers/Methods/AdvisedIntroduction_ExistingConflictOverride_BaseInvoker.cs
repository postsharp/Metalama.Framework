using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictOverride_BaseInvoker;
using System;

[assembly: AspectOrder( typeof(IntroductionAttribute), typeof(OverrideAttribute) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictOverride_BaseInvoker
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Override )]
        public void BaseClass_VoidMethod()
        {
            meta.InsertComment( "Introduced." );
            meta.Target.Method.Invokers.Base!.Invoke( meta.This );
        }

        [Introduce( WhenExists = OverrideStrategy.Override )]
        public int BaseClass_ExistingMethod()
        {
            meta.InsertComment( "Introduced." );

            return meta.Target.Method.Invokers.Base!.Invoke( meta.This );
        }

        [Introduce( WhenExists = OverrideStrategy.Override )]
        public int BaseClass_ExistingMethod_Parameterized( int x )
        {
            meta.InsertComment( "Introduced." );

            return meta.Target.Method.Invokers.Base!.Invoke( meta.This, meta.Target.Method.Parameters[0].Value );
        }
    }

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var method in builder.Target.Methods)
            {
                builder.Advice.Override( method, nameof(MethodTemplate) );
            }
        }

        [Template]
        public dynamic MethodTemplate()
        {
            Console.WriteLine( "Override" );

            if (meta.Target.Method.Parameters.Count == 0)
            {
                return meta.Target.Method.Invokers.Base!.Invoke( meta.This );
            }
            else
            {
                return meta.Target.Method.Invokers.Base!.Invoke( meta.This, meta.Target.Method.Parameters[0].Value );
            }
        }
    }

    internal class BaseClass
    {
        public virtual void BaseClass_VoidMethod()
        {
            Console.WriteLine( "BaseClass_VoidMethod()" );
        }

        public virtual int BaseClass_ExistingMethod()
        {
            Console.WriteLine( "BaseClass_ExistingMethod()" );

            return 42;
        }

        public virtual int BaseClass_ExistingMethod_Parameterized( int x )
        {
            Console.WriteLine( "BaseClass_ExistingMethod_Parameterized()" );

            return x + 42;
        }
    }

    // <target>
    [Introduction]
    [Override]
    internal class TargetClass : BaseClass
    {
        public void VoidMethod()
        {
            Console.WriteLine( "TargetClass_VoidMethod()" );
        }

        public int ExistingMethod()
        {
            Console.WriteLine( "TargetClass_ExistingMethod()" );

            return 42;
        }

        public int ExistingMethod_Parameterized( int x )
        {
            Console.WriteLine( "TargetClass_ExistingMethod_Parameterized" );

            return x + 42;
        }
    }
}