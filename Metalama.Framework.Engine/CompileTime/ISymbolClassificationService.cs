using Metalama.Framework.Code;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// The public interface for symbol classification.
/// </summary>
public interface ISymbolClassificationService : IService
{
    ExecutionScope GetExecutionScope( Compilation compilation, ISymbol symbol );

    bool IsTemplate( Compilation compilation, ISymbol symbol );
}