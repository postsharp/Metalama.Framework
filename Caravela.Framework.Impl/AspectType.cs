using Caravela.Framework.Aspects;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using System;

namespace Caravela.Framework.Impl
{
    internal class AspectType
    {
        private IReadOnlyList<OrderedAspectLayer>? _parts;
        public string Name => this.Type.FullName;

        public IAspectDriver AspectDriver { get; }

        public IReadOnlyList<AspectLayer> UnorderedLayers { get; }

        public IReadOnlyList<OrderedAspectLayer> Parts 
            => this._parts ?? throw new InvalidOperationException("Method UpdateFromOrderedParts has not been called.");

        internal void UpdateFromOrderedParts( IReadOnlyList<OrderedAspectLayer> allOrderedParts )
        {
            this._parts = allOrderedParts.Where( p => p.AspectLayerId.AspectName == this.Name ).ToImmutableArray();
        }

        public INamedType Type { get; }

        public AspectType( INamedType aspectType, IAspectDriver aspectDriver, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
        {
            this.Type = aspectType;
            this.AspectDriver = aspectDriver;


            var partArrayBuilder = ImmutableArray.CreateBuilder<AspectLayer>();
            
            // Add the default part.
            partArrayBuilder.Add( new AspectLayer( this, null ) );

            // Add the parts defined in [ProvidesAspectLayers].
            var aspectLayersAttributeType = aspectType.Compilation.TypeFactory.GetTypeByReflectionType( typeof(ProvidesAspectLayersAttribute) );
            var aspectLayersAttributeData = aspectType.Attributes.Where( a => a.Type.Is( aspectLayersAttributeType ) ).SingleOrDefault();

            if ( aspectLayersAttributeData != null )
            {
                var aspectLayersAttribute = (ProvidesAspectLayersAttribute) compileTimeAssemblyLoader.CreateAttributeInstance( aspectLayersAttributeData );
                partArrayBuilder.AddRange( aspectLayersAttribute.Parts.Select( partName => new AspectLayer( this, partName ) ) );
            }

            this.UnorderedLayers = partArrayBuilder.ToImmutable();
        }
    }
}
