using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code.Syntax
{
    /// <summary>
    /// Base interface for compile-time objects that can be converted to run-time syntax.
    /// </summary>
    [CompileTimeOnly]
    public interface ISyntaxBuilder
    {
        ISyntax ToSyntax();
    }
}