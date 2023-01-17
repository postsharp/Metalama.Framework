#if TEST_OPTIONS
// @Skipped(#30509 Introduced method's parameters are not included in the initial lexical scope)
# endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AnotherInstance_FinalInvoker
{
    [AttributeUsage( AttributeTargets.Class )]
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.IntroduceMethod(
                aspectBuilder.Target,
                nameof(OverrideMethod),
                whenExists: OverrideStrategy.Override,
                buildMethod: m =>
                {
                    m.Name = "VoidMethod";
                    m.ReturnType = TypeFactory.GetType( SpecialType.Void );
                } );

            aspectBuilder.Advice.IntroduceMethod(
                aspectBuilder.Target,
                nameof(OverrideMethod),
                whenExists: OverrideStrategy.Override,
                buildMethod: m =>
                {
                    m.Name = "Method";
                    m.ReturnType = TypeFactory.GetType( SpecialType.Int32 );
                    m.AddParameter( "x", TypeFactory.GetType( SpecialType.Int32 ) );
                } );
        }

        [Template]
        public dynamic? OverrideMethod()
        {
            var x = meta.This;

            if (meta.Target.Method.Parameters.Count == 0)
            {
                return meta.Target.Method.Invokers.Final!.Invoke( x );
            }
            else
            {
                return meta.Target.Method.Invokers.Final!.Invoke( x, meta.Target.Method.Parameters[0].Value );
            }
        }
    }

    internal class BaseClass
    {
        public virtual void VoidMethod() { }

        public virtual int Method( int x )
        {
            return x;
        }
    }

    // <target>
    [TestAttribute]
    internal class TargetClass : BaseClass { }
}