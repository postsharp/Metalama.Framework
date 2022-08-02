using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal class SyntaxTreeDependency
{
    public string Path { get; }

    public ulong Hash { get; }

    public ImmutableHashSet<string> DependentSyntaxTrees { get; }
}