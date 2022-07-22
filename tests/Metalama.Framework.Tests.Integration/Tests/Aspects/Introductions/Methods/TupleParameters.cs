using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.TupleParameters
{
    internal class MyAspect : TypeAspect
    {
        [Introduce]
        internal void M( (int[] A, (string?, string?[], int?[]) B) c ) { }
    }

#nullable disable

    // <target>
    [MyAspect]
    internal class C { }
}