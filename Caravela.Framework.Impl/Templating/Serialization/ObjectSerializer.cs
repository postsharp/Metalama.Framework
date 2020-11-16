using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    public abstract class ObjectSerializer
    {
        public abstract ExpressionSyntax Serialize( object o );
    }

    public abstract class TypedObjectSerializer<T> : ObjectSerializer
    {
        public sealed override ExpressionSyntax Serialize( object o )
        {
            return this.Serialize( (T) o );
        }
        public abstract ExpressionSyntax Serialize( T o );
    }
}