using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Initialize.HasAspect
{
    class Aspect : OverrideMethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            if (!builder.Target.Aspects<Aspect>().Any())
            {
                throw new Exception();
            }
            
            if (builder.Target.DeclaringType.Methods.OfName( "NoAspect" ).Single().Aspects<Aspect>().Any() )
            {
                throw new Exception();
            }
        }

        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException();
        }
    }

    class TargetCode
    {
        // <target>
        [Aspect]
        int Method(int a)
        {
            return a;
        }

        void NoAspect() { }
    }
}