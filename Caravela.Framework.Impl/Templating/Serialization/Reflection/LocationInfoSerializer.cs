using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class LocationInfoSerializer : TypedObjectSerializer<LocationInfo>
    {
        public override ExpressionSyntax Serialize( LocationInfo o )
        {
            return null;
        }
    }
}