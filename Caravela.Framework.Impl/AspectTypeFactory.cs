using Caravela.Framework.Sdk;
using System.Collections.Generic;

namespace Caravela.Framework.Impl
{
    class AspectTypeFactory
    {
        private readonly AspectDriverFactory _aspectDriverFactory;

        private readonly Dictionary<INamedType, AspectType> _aspectTypes = new();

        public AspectTypeFactory( AspectDriverFactory aspectDriverFactory ) => this._aspectDriverFactory = aspectDriverFactory;

        public AspectType GetAspectType( INamedType attributeType )
        {
            if ( !this._aspectTypes.TryGetValue( attributeType, out var aspectType ) )
            {
                // TODO: handle AspectParts properly
                aspectType = new AspectType(
                    attributeType.FullName, this._aspectDriverFactory.GetAspectDriver( attributeType ), new string?[] { null } );
                this._aspectTypes.Add( attributeType, aspectType );
            }

            return aspectType;
        }
    }
}
