using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaConstructorInfoSerializer : TypedObjectSerializer<CaravelaConstructorInfo>
    {
        public override ExpressionSyntax Serialize( CaravelaConstructorInfo o )
        {
            return null;
        }
    }
}