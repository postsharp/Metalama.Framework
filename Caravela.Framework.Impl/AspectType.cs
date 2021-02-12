using Caravela.Framework.Code;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl
{
    internal class AspectType
    {
        public string Name => this.Type.FullName;

        public IAspectDriver AspectDriver { get; }

        public IImmutableList<AspectPart> Parts { get; }

        public INamedType Type { get; }
        

        public AspectType( INamedType aspectType, IAspectDriver aspectDriver, IEnumerable<string?> partNames )
        {
            this.Type = aspectType;
            this.AspectDriver = aspectDriver;
            this.Parts = partNames.Select( partName => new AspectPart( this, partName ) ).ToImmutableArray();
        }
        
    }
}
