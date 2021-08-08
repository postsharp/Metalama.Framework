using Caravela.Framework.Aspects;
using Caravela.Framework.Code.Syntax;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// A compile-time representation of a run-time expression.
    /// </summary>
    [CompileTimeOnly]
    public interface IExpression : ISyntaxBuilder
    {
        /// <summary>
        /// Gets the expression type.
        /// </summary>
        IType Type { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Value"/> can be set.
        /// </summary>
        bool IsAssignable { get; }

        /// <summary>
        /// Gets or sets the expression value.
        /// </summary>
        dynamic? Value { get; set; }
    }
}