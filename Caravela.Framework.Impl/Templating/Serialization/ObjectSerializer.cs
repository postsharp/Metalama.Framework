using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    public abstract class ObjectSerializer
    {
        public abstract ExpressionSyntax SerializeObject( object o );
    }
}