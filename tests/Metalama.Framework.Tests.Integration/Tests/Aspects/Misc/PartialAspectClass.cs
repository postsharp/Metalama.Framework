using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.PartialAspectClass
{
    internal partial class MyAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var m in builder.Target.Methods)
            {
                builder.Advise.Override( m, nameof(MethodTemplate) );
            }
        }
    }

    // <target>
    [MyAspect]
    internal class C
    {
        private void M() { }
    }
}