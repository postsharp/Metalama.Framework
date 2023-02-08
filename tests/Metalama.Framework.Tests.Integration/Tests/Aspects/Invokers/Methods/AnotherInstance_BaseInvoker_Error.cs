#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_HIGHER)
# endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AnotherInstance_BaseInvoker_Error
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

            var methodOverrideBuilder = aspectBuilder.Advice.IntroduceMethod(
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
            TargetClass local = null!;
            
            if (meta.Target.Method.Parameters.Count == 0)
            {
                return meta.Target.Method.With( local, InvokerOptions.Base ).Invoke();
            }
            else
            {
                return meta.Target.Method.With( local, InvokerOptions.Base ).Invoke( meta.Target.Method.Parameters[0].Value );
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