using Caravela.Reactive;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl
{
    class AspectTypeFactory
    {
        private readonly AspectDriverFactory _aspectDriverFactory;

        private readonly Dictionary<INamedType, AspectType> _aspectTypes = new();

        public AspectTypeFactory( AspectDriverFactory aspectDriverFactory ) => this._aspectDriverFactory = aspectDriverFactory;

        public AspectType GetAspectType( INamedType attributeType, in ReactiveObserverToken observerToken = default )
        {
            if ( !this._aspectTypes.TryGetValue( attributeType, out var aspectType ) )
            {
                // TODO: handle AspectParts properly
                aspectType = new AspectType(
                    attributeType.FullName, this._aspectDriverFactory.GetAspectDriver( attributeType, observerToken ), ImmutableArray.Create( new AspectPart( null, 0 ) ) );
                this._aspectTypes.Add( attributeType, aspectType );
            }

            return aspectType;
        }
    }
}
