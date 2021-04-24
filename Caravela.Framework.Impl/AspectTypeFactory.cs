// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl
{
    internal class AspectTypeFactory
    {
        private readonly Compilation _compilation;
        private readonly AspectDriverFactory _aspectDriverFactory;

        private readonly Dictionary<INamedTypeSymbol, AspectType> _aspectTypes = new();

        public AspectTypeFactory( Compilation compilation, AspectDriverFactory aspectDriverFactory )
        {
            this._compilation = compilation;
            this._aspectDriverFactory = aspectDriverFactory;
        }

        public IEnumerable<AspectType> GetAspectTypes( IReadOnlyList<INamedTypeSymbol> attributeTypes, IDiagnosticAdder diagnosticAdder )
        {
            foreach ( var attributeType in attributeTypes.OrderByInheritance() )
            {
                AspectType? baseAspectType;

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

                    if ( AspectType.TryCreateAspectType( attributeType, baseAspectType, aspectDriver, diagnosticAdder, out aspectType ) )
                    {
                        this._aspectTypes.Add( attributeType, aspectType );
                    }
                }
            }

            return attributeTypes.Select( at => this._aspectTypes[at] );
        }
    }
}