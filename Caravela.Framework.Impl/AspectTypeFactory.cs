﻿using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;

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

        public IEnumerable<AspectType> GetAspectTypes( IEnumerable<INamedType> attributeTypes )
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

                    aspectType = new( attributeType, baseAspectType, aspectDriver );

                    this._aspectTypes.Add( attributeType, aspectType );
                }
            }

            return attributeTypes.Select( at => this._aspectTypes[at] );
        }
    }
}
