using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaParameterInfoSerializer : TypedObjectSerializer<CaravelaParameterInfo>
    {
        public override ExpressionSyntax Serialize( CaravelaParameterInfo o )
        {
            return null;
        }
    }
}