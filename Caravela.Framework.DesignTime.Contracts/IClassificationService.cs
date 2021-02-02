using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.DesignTime.Contracts
{
    public interface IClassificationService : ICompilerService
    {
        bool TryGetClassifiedTextSpans( SemanticModel model, SyntaxNode root, [NotNullWhen(true)] out IReadOnlyClassifiedTextSpanCollection? classifiedTextSpans );
    }
}