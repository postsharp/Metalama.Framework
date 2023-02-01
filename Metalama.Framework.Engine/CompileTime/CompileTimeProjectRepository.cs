// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CompileTime;

internal sealed partial class CompileTimeProjectRepository : IProjectService
{
    public CompileTimeProject RootProject { get; }

    // Maps the identity of the run-time project to the compile-time project.
    private readonly Dictionary<AssemblyIdentity, CompileTimeProject?> _projects;

    public bool TryGetCompileTimeProject( AssemblyIdentity assemblyIdentity, [NotNullWhen( true )] out CompileTimeProject? compileTimeProject )
        => this._projects.TryGetValue( assemblyIdentity, out compileTimeProject );

    private CompileTimeProjectRepository(
        CompileTimeDomain domain,
        ProjectServiceProvider serviceProvider,
        Dictionary<AssemblyIdentity, CompileTimeProject?> projects,
        CompileTimeProject rootProject )
    {
        this.RootProject = rootProject;
        this._projects = projects;

        // Register assemblies into the domain.
        var referenceAssemblyLocator = serviceProvider.GetReferenceAssemblyLocator();
        domain.RegisterAssemblyPaths( referenceAssemblyLocator.SystemAssemblyPaths );
    }

    internal AttributeDeserializer CreateAttributeDeserializer( ProjectServiceProvider serviceProvider )
        => new( serviceProvider, new ProjectSpecificCompileTimeTypeResolver( serviceProvider, this ) );
}