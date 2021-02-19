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
        private IReadOnlyList<OrderedAspectPart>? _parts;
        public string Name => this.Type.FullName;

        public IAspectDriver AspectDriver { get; }

        public IReadOnlyList<AspectPart> UnorderedParts { get; }

        public IReadOnlyList<OrderedAspectPart> Parts 
            => this._parts ?? throw new InvalidOperationException("Method UpdateFromOrderedParts has not been called.");

        internal void UpdateFromOrderedParts( IReadOnlyList<OrderedAspectPart> allOrderedParts )
        {
            this._parts = allOrderedParts.Where( p => p.AspectType == this ).ToImmutableArray();
        }

        public INamedType Type { get; }

        public AspectType( INamedType aspectType, IAspectDriver aspectDriver, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
        {
            this.Type = aspectType;
            this.AspectDriver = aspectDriver;


            var partArrayBuilder = ImmutableArray.CreateBuilder<AspectPart>();
            
            // Add the default part.
            partArrayBuilder.Add( new AspectPart( this ) );

            // Add the parts defined in [ProvidesAspectParts].
            var aspectPartsAttributeType = aspectType.Compilation.TypeFactory.GetTypeByReflectionType( typeof(ProvidesAspectPartsAttribute) );
            var aspectPartsAttributeData = aspectType.Attributes.Where( a => a.Type.Is( aspectPartsAttributeType ) ).SingleOrDefault();

            if ( aspectPartsAttributeData != null )
            {
                var aspectPartsAttribute = (ProvidesAspectPartsAttribute) compileTimeAssemblyLoader.CreateAttributeInstance( aspectPartsAttributeData );
                partArrayBuilder.AddRange( aspectPartsAttribute.Parts.Select( partName => new AspectPart( this, partName ) ) );
            }

            this.UnorderedParts = partArrayBuilder.ToImmutable();
        }
    }
}
