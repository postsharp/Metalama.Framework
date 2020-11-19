using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    public abstract class TypedObjectSerializer<T> : ObjectSerializer
    {
        public sealed override ExpressionSyntax SerializeObject( object o )
        {
            return this.Serialize( (T) o );
        }
        public abstract ExpressionSyntax Serialize( T o );
    }
}