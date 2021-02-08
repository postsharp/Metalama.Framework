using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.DesignTime.Contracts
{
    [Guid( "0a2a7b74-a701-468b-a000-3f1bbd7eda4d" )]
    public interface IClassificationService : ICompilerService
    {
        bool TryGetClassifiedTextSpans( SemanticModel model, SyntaxNode root, [NotNullWhen( true )] out IReadOnlyClassifiedTextSpanCollection? classifiedTextSpans );
    }
}