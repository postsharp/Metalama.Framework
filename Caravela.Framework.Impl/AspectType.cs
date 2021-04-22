// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl
{
    // TODO: Consider having an abstract base for simple testing.
    internal class AspectType : IAspectType
    {
        private readonly IAspectDriver? _aspectDriver;
        private IReadOnlyList<AspectLayer>? _layers;

        public string Name => this.Type.FullName;

        public AspectType? BaseAspectType { get; }

        public IAspectDriver AspectDriver => this._aspectDriver.AssertNotNull();

        public IReadOnlyList<AspectLayer> Layers => this._layers.AssertNotNull();

        public INamedType Type { get; }

        public bool IsAbstract => this.Type.IsAbstract;

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectType"/> class.
        /// </summary>
        /// <param name="aspectType"></param>
        /// <param name="aspectDriver">Can be null for testing.</param>
        private AspectType( INamedType aspectType, AspectType? baseAspectType, IAspectDriver? aspectDriver )
        {
            this.Type = aspectType;
            this.BaseAspectType = baseAspectType;
            this._aspectDriver = aspectDriver;
        }

        public AspectInstance CreateAspectInstance( IAspect aspect, ICodeElement target ) => new( aspect, target, this );

        public static bool TryCreateAspectType(
            INamedType aspectNamedType,
            AspectType? baseAspectType,
            IAspectDriver? aspectDriver,
            IDiagnosticAdder diagnosticAdder,
            [NotNullWhen( true )] out AspectType? aspectType )
        {
            var layersBuilder = ImmutableArray.CreateBuilder<AspectLayer>();

            var newAspectType = new AspectType( aspectNamedType, baseAspectType, aspectDriver );

            // Add the default part.
            layersBuilder.Add( new AspectLayer( newAspectType, null ) );

            // Add the parts defined in [ProvidesAspectLayers]. If it is not defined in the current type, look up in the base classes.
            var aspectLayersAttributeType = newAspectType.Type.Compilation.TypeFactory.GetTypeByReflectionType( typeof(ProvidesAspectLayersAttribute) );

            for ( var type = newAspectType; type != null; type = type.BaseAspectType )
            {
                var aspectLayersAttributeData = type.Type.Attributes.SingleOrDefault( a => a.Type.Is( aspectLayersAttributeType ) );

                if ( aspectLayersAttributeData != null )
                {
                    // TODO: Using global state makes it impossible to test.
                    if ( !AttributeDeserializer.SystemTypesDeserializer.TryCreateAttribute<ProvidesAspectLayersAttribute>(
                        aspectLayersAttributeData,
                        diagnosticAdder,
                        out var aspectLayersAttribute ) )
                    {
                        aspectType = null;

                        return false;
                    }

                    layersBuilder.AddRange( aspectLayersAttribute.Layers.Select( partName => new AspectLayer( newAspectType, partName ) ) );

                    break;
                }
            }

            newAspectType._layers = layersBuilder.ToImmutable();

            aspectType = newAspectType;

            return true;
        }
    }
}