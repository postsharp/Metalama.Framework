using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaConstructorInfoSerializer : TypedObjectSerializer<CaravelaConstructorInfo>
    {
        private readonly CaravelaTypeSerializer _typeSerializer;

        public CaravelaConstructorInfoSerializer( CaravelaTypeSerializer typeSerializer )
        {
            this._typeSerializer = typeSerializer;
        }
        public override ExpressionSyntax Serialize( CaravelaConstructorInfo o )
        {
            return CaravelaMethodInfoSerializer.CreateMethodBase( this._typeSerializer, o );
        }
    }
}