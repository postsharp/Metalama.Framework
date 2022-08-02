using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Pipeline.DesignTime;

public interface IDependencyCollector : IService
{
    void AddDependency( ISymbol masterSymbol, ISymbol dependentSymbol );
}