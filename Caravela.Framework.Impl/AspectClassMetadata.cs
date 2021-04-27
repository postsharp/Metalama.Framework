// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// Represents the metadata of an aspect class. This class is compilation-independent. 
    /// </summary>
    internal class AspectClassMetadata : IAspectClassMetadata
    {
        private readonly IAspectDriver? _aspectDriver;
        private IReadOnlyList<AspectLayer>? _layers;

        /// <inheritdoc />
        public string FullName { get; }

        /// <inheritdoc />
        public string DisplayName { get; }

        /// <summary>
        /// Gets metadata of the base aspect class.
        /// </summary>
        public AspectClassMetadata? BaseClass { get; }

        /// <summary>
        /// Gets the aspect driver of the current class, responsible for executing the aspect.
        /// </summary>
        public IAspectDriver AspectDriver => this._aspectDriver.AssertNotNull();

        /// <summary>
        /// Gets the list of layers of the current aspect.
        /// </summary>
        public IReadOnlyList<AspectLayer> Layers => this._layers.AssertNotNull();

        public Location? DiagnosticLocation { get; }

        /// <inheritdoc />
        public bool IsAbstract { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectClassMetadata"/> class.
        /// </summary>
        /// <param name="aspectTypeSymbol"></param>
        /// <param name="aspectDriver">Can be null for testing.</param>
        private AspectClassMetadata( INamedTypeSymbol aspectTypeSymbol, AspectClassMetadata? baseClass, IAspectDriver? aspectDriver )
        {
            this.FullName = aspectTypeSymbol.GetReflectionName();
            this.DisplayName = aspectTypeSymbol.Name;
            this.IsAbstract = aspectTypeSymbol.IsAbstract;
            this.BaseClass = baseClass;
            this._aspectDriver = aspectDriver;
            this.DiagnosticLocation = aspectTypeSymbol.GetDiagnosticLocation();
        }

        /// <summary>
        /// Creates a new  <see cref="AspectInstance"/>.
        /// </summary>
        /// <param name="aspect">The instance of the aspect class.</param>
        /// <param name="target">The declaration on which the aspect was applied.</param>
        /// <returns></returns>
        public AspectInstance CreateAspectInstance( IAspect aspect, ICodeElement target ) => new( aspect, target, this );

        /// <summary>
        /// Creates an instance of the <see cref="AspectClassMetadata"/> class.
        /// </summary>
        public static bool TryCreate(
            INamedTypeSymbol aspectNamedType,
            AspectClassMetadata? baseAspectType,
            IAspectDriver? aspectDriver,
            IDiagnosticAdder diagnosticAdder,
            [NotNullWhen( true )] out AspectClassMetadata? aspectClassMetadata )
        {
            var layersBuilder = ImmutableArray.CreateBuilder<AspectLayer>();

            var newAspectType = new AspectClassMetadata( aspectNamedType, baseAspectType, aspectDriver );

            // Add the default part.
            layersBuilder.Add( new AspectLayer( newAspectType, null ) );

            // Add the parts defined in [ProvidesAspectLayers]. If it is not defined in the current type, look up in the base classes.

            for ( var type = aspectNamedType; type != null; type = type.BaseType )
            {
                var aspectLayersAttributeData =
                    type.GetAttributes().SingleOrDefault( a => a.AttributeClass?.Is( typeof(ProvidesAspectLayersAttribute) ) ?? false );

                if ( aspectLayersAttributeData != null )
                {
                    // TODO: Using global state makes it impossible to test.
                    if ( !AttributeDeserializer.SystemTypes.TryCreateAttribute<ProvidesAspectLayersAttribute>(
                        aspectLayersAttributeData,
                        diagnosticAdder,
                        out var aspectLayersAttribute ) )
                    {
                        aspectClassMetadata = null;

                        return false;
                    }

                    layersBuilder.AddRange( aspectLayersAttribute.Layers.Select( partName => new AspectLayer( newAspectType, partName ) ) );

                    break;
                }
            }

            newAspectType._layers = layersBuilder.ToImmutable();

            aspectClassMetadata = newAspectType;

            return true;
        }
    }
}