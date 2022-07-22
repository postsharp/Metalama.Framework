using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.TupleParameters
{
    internal class MyAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException();
        }
    }

    // <target>
    internal class C
    {
        [MyAspect]
        internal void M( (int[] A, (string?, string?[], int?[]) B) c ) { }
    }
}