#nullable disable

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Nullable.NullableContextErrors
{
    internal class Aspect : Attribute, IAspect<INamedType>
    {
        [Introduce]
        private string? Introduced1( string? a ) => a!.ToString();
        
          #nullable enable
        [Introduce]
        private string? Introduced2( string? a ) => a!.ToString();
        
        
        [Introduce]
        private string Introduced3(int x)
        {
            #nullable disable
            return "";
        }
    }

}