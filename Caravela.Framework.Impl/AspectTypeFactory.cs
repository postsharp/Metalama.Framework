// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl
{
    internal class AspectTypeFactory
    {
        private readonly CompilationModel _compilation;
        private readonly AspectDriverFactory _aspectDriverFactory;

        private readonly Dictionary<INamedType, AspectType> _aspectTypes = new();

        public AspectTypeFactory( CompilationModel compilation, AspectDriverFactory aspectDriverFactory )
        {
            this._compilation = compilation;
            this._aspectDriverFactory = aspectDriverFactory;
        }

        public IEnumerable<AspectType> GetAspectTypes( IReadOnlyList<INamedType> attributeTypes )
        {
            foreach ( var attributeType in attributeTypes.OrderBy( at => this._compilation.GetDepth( at ) ) )
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

                    aspectType = new AspectType( attributeType, baseAspectType, aspectDriver );

                    this._aspectTypes.Add( attributeType, aspectType );
                }
            }

            return attributeTypes.Select( at => this._aspectTypes[at] );
        }
    }
}