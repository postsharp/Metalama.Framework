using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.TupleParameters
{
    internal class MyAspect : TypeAspect
    {
        [Introduce]
        internal void M( (int[] A, (string? C, string?[] D, int?[] E) B) x )
        {
            // Check that the names have persisted.
            Console.WriteLine( $"{x.A}, {x.B.C}" );
        }
    }

#nullable disable

    // <target>
    [MyAspect]
    internal class C { }
}