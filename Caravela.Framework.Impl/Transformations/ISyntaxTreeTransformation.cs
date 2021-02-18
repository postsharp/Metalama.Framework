using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represents any introduction to the code model that modifies a syntax tree. 
    /// </summary>
    internal interface ISyntaxTreeTransformation
    {
        /// <summary>
        /// Gets the syntax tree that needs to be modified.
        /// </summary>
        SyntaxTree TargetSyntaxTree { get; }
    }
}