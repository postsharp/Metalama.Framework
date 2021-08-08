using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code.Syntax
{
    /// <summary>
    /// Base interface for a compile-time object that represents syntax. The difference with <see cref="IExpression"/> is that
    /// this interface does not expose the <see cref="IExpression.Value"/> member that allows for interaction with template code.
    /// </summary>
    [CompileTimeOnly]
    public interface ISyntax
    {
        /// <summary>
        /// Gets a value indicating whether the expression represented by the syntax is assignable, i.e. can be on the left of an assignment.
        /// </summary>
        bool IsAssignable { get; }

        /// <summary>
        /// Gets the type of the expression represented by the current syntax.
        /// </summary>
        IType ExpressionType { get; }
    }
}