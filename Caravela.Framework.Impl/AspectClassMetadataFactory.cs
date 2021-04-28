// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// Creates <see cref="AspectClassMetadata"/>.
    /// </summary>
    internal class AspectClassMetadataFactory
    {
        private readonly AspectDriverFactory _aspectDriverFactory;

        private readonly Dictionary<INamedTypeSymbol, AspectClassMetadata> _aspectTypes = new();

        public AspectClassMetadataFactory( AspectDriverFactory aspectDriverFactory )
        {
            this._aspectDriverFactory = aspectDriverFactory;
        }

        /// <summary>
        /// Creates a list of <see cref="AspectClassMetadata"/> given input list of aspect types.
        /// </summary>
        public IEnumerable<AspectClassMetadata> GetAspectClassMetadatas( IReadOnlyList<INamedTypeSymbol> attributeTypes, IDiagnosticAdder diagnosticAdder )
        {
            foreach ( var attributeType in attributeTypes.OrderByInheritance() )
            {
                AspectClassMetadata? baseAspectType;

                if ( attributeType.BaseType != null )
                {
                    _ = this._aspectTypes.TryGetValue( attributeType.BaseType, out baseAspectType );
                }
                else
                {
                    baseAspectType = null;
                }

                if ( !this._aspectTypes.TryGetValue( attributeType, out var aspectType ) )
                {
                    var aspectDriver = this._aspectDriverFactory.GetAspectDriver( attributeType );

                    if ( AspectClassMetadata.TryCreate( attributeType, baseAspectType, aspectDriver, diagnosticAdder, out aspectType ) )
                    {
                        this._aspectTypes.Add( attributeType, aspectType );
                    }
                }
            }

            return attributeTypes.Select( at => this._aspectTypes[at] );
        }
    }
}