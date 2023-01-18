#if TEST_OPTIONS
// @Skipped(#28907 Linker: conditional access expression)
# endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictNew_ConditionalBaseInvoker;
using System;

[assembly: AspectOrder( typeof(IntroductionAttribute), typeof(OverrideAttribute) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.AdvisedIntroduction_ExistingConflictNew_ConditionalBaseInvoker
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.New )]
        public void BaseClass_Method()
        {
            meta.InsertComment( "Introduced." );
            meta.Target.Method.Invokers.ConditionalBase!.Invoke( meta.This );
        }

        [Introduce( WhenExists = OverrideStrategy.New )]
        public void TargetClass_Method()
        {
            meta.InsertComment( "Introduced." );
            meta.Target.Method.Invokers.ConditionalBase!.Invoke( meta.This );
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
        public dynamic? MethodTemplate()
        {
            Console.WriteLine( "Override" );

            if (meta.Target.Method.ReturnType.Is( SpecialType.Void ))
            {
                meta.Target.Method.Invokers.ConditionalBase!.Invoke( meta.This );

                return default;
            }
            else
            {
                return meta.Target.Method.Invokers.ConditionalBase!.Invoke( meta.This );
            }
        }
    }

    internal class BaseClass
    {
        public virtual void BaseClass_Method()
        {
            Console.WriteLine( "BaseClass_Method()" );
        }
    }

    // <target>
    [Introduction]
    [Override]
    internal class TargetClass : BaseClass
    {
        public void TargetClass_Method()
        {
            Console.WriteLine( "TargetClass_Method()" );
        }
    }
}