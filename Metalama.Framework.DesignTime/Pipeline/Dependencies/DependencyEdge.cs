using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Represents a single dependency edge between a master syntax tree and a dependent syntax tree.
/// </summary>
internal record DependencyEdge( Compilation MasterCompilation, string MasterFilePath, ulong MasterFileHash, string DependentFilePath );