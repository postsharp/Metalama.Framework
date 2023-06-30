using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AddAspect.HasAspect
{
    internal class Aspect : OverrideMethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            if (!builder.Target.Enhancements().GetAspects<Aspect>().Any())
            {
                throw new Exception();
            }

            if (builder.Target.DeclaringType.Methods.OfName( "NoAspect" ).Single().Enhancements().GetAspects<Aspect>().Any())
            {
                throw new Exception();
            }
        }

        public override dynamic OverrideMethod()
        {
            throw new NotImplementedException();
        }
    }

    internal class TargetCode
    {
        // <target>
        [Aspect]
        private int Method( int a )
        {
            return a;
        }

        private void NoAspect() { }
    }
}