// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl
{
    internal class AspectType
    {
        private readonly IAspectDriver? _aspectDriver;

        public string Name => this.Type.FullName;

        public AspectType? BaseAspectType { get; }

        public IAspectDriver AspectDriver => this._aspectDriver.AssertNotNull();

        public IReadOnlyList<AspectLayer> Layers { get; }

        public INamedType Type { get; }

        public bool IsAbstract => this.Type.IsAbstract;

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectType"/> class.
        /// </summary>
        /// <param name="aspectType"></param>
        /// <param name="aspectDriver">Can be null for testing.</param>
        public AspectType( INamedType aspectType, AspectType? baseAspectType, IAspectDriver? aspectDriver )
        {
            this.Type = aspectType;
            this.BaseAspectType = baseAspectType;
            this._aspectDriver = aspectDriver;

            var partArrayBuilder = ImmutableArray.CreateBuilder<AspectLayer>();

            // Add the default part.
            partArrayBuilder.Add( new AspectLayer( this, null ) );

            // Add the parts defined in [ProvidesAspectLayers]. If it is not defined in the current type, look up in the base classes.
            var aspectLayersAttributeType = ((ICodeElement) aspectType).Compilation.TypeFactory.GetTypeByReflectionType( typeof( ProvidesAspectLayersAttribute ) );

            for ( var type = this; type != null; type = type.BaseAspectType )
            {
                var aspectLayersAttributeData = type.Type.Attributes.SingleOrDefault( a => a.Type.Is( aspectLayersAttributeType ) );

                if ( aspectLayersAttributeData != null )
                {
                    var aspectLayersAttribute =
                        AttributeDeserializer.SystemTypesDeserializer.CreateAttribute<ProvidesAspectLayersAttribute>( aspectLayersAttributeData );
                    partArrayBuilder.AddRange( aspectLayersAttribute.Layers.Select( partName => new AspectLayer( this, partName ) ) );
                    break;
                }
            }

            this.Layers = partArrayBuilder.ToImmutable();
        }
    }
}
