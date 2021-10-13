#nullable disable

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Nullable.NullableContextErrors
{
    internal class Aspect : TypeAspect
    {
        [Introduce]
        private string Introduced1( string a ) => a.ToString();

#nullable enable
        [Introduce]
        private string? Introduced2( string? a ) => a!.ToString();

        [Introduce]
        private string Introduced3( int x )
        {
#nullable disable
            return "";
        }
    }
}