using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Initialize.HasAspect
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