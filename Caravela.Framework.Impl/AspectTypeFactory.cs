using Caravela.Framework.Code;
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
                var aspectDriver = this._aspectDriverFactory.GetAspectDriver( attributeType );

                // TODO: create AspectParts properly
                aspectType = new( attributeType.FullName, aspectDriver, new string?[] { null } );

                this._aspectTypes.Add( attributeType, aspectType );
            }

            return aspectType;
        }
    }
}
