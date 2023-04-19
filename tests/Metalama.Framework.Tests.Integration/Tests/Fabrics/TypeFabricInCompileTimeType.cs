using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Aspects.Fabrics.TypeFabricInCompileTimeType;

// <target>
[CompileTime]
internal class TargetCode
{
    private class Fabric : TypeFabric
    {
        public override void AmendType( ITypeAmender amender )
        {
            amender.Advice.IntroduceMethod(amender.Type, nameof(M));
        }

        [Template]
        private void M()
        {
            Console.WriteLine( "introduced" );
        }
    }
}