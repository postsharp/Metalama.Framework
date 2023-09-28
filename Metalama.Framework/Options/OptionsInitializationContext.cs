using Metalama.Framework.Aspects;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Project;

namespace Metalama.Framework.Options;

/// <summary>
/// Context of the <see cref="IHierarchicalOptions.GetDefaultOptions"/> method.
/// </summary>
[CompileTime]
public sealed class OptionsInitializationContext
{
    /// <summary>
    /// Gets the current project.
    /// </summary>
    public IProject Project { get; }

    /// <summary>
    /// Gets a service allowing to report diagnostics.
    /// </summary>
    public ScopedDiagnosticSink Diagnostics { get; }

    internal OptionsInitializationContext( IProject project, ScopedDiagnosticSink diagnostics )
    {
        this.Project = project;
        this.Diagnostics = diagnostics;
    }
}