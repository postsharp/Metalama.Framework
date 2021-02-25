using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal class CaravelaConstructorInfoSerializer : TypedObjectSerializer<CompileTimeConstructorInfo>
    {
        private readonly CaravelaTypeSerializer _typeSerializer;

        public CaravelaConstructorInfoSerializer( CaravelaTypeSerializer typeSerializer )
        {
            this._typeSerializer = typeSerializer;
        }

        public override ExpressionSyntax Serialize( CompileTimeConstructorInfo o )
        {
            return CaravelaMethodInfoSerializer.CreateMethodBase( this._typeSerializer, o );
        }
    }
}