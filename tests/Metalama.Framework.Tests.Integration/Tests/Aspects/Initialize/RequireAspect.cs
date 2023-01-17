using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Aspects.Initialize.RequireAspect;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1) )]

namespace Metalama.Framework.Tests.Integration.Aspects.Initialize.RequireAspect
{
    internal class Aspect1 : ParameterAspect
    {
        public override void BuildAspect( IAspectBuilder<IParameter> builder )
        {
            builder.Outbound.Select( t => (IMethod)t.DeclaringMember ).RequireAspect<Aspect2>();
        }
    }

    internal class Aspect2 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( string.Join( ",", meta.AspectInstance.Predecessors.Select( x => x.Instance.ToString() ).OrderBy( x => x ) ) );

            return meta.Proceed();
        }
    }

    internal class TargetCode
    {
        // <target>
        private int Method( [Aspect1] int a, [Aspect1] string b )
        {
            return a;
        }
    }
}