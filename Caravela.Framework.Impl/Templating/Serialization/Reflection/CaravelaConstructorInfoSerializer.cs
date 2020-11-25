using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaConstructorInfoSerializer : TypedObjectSerializer<CaravelaConstructorInfo>
    {
        public override ExpressionSyntax Serialize( CaravelaConstructorInfo o )
        {
            return CaravelaMethodInfoSerializer.CreateMethodBase( o );
        }
    }
}