using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represents any introduction to the code model that modifies a syntax tree. 
    /// </summary>
    internal interface ISyntaxTreeIntroduction
    {
        SyntaxTree TargetSyntaxTree { get;  }
    }
}