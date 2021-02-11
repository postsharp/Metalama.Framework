using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Transformations
{
    internal interface IIntroducedElement : ICodeElementBuilder
    {
        SyntaxTree TargetSyntaxTree { get;  }
    }
}